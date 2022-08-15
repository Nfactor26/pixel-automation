using Dawn;
using OpenCvSharp;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Image.Matching.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Image Locator", "Control Locators", iconSource: null, description: "Identify a image control on screen", tags: new string[] { "Locator" })]
public class ImageControlLocatorComponent : ServiceComponent, IControlLocator, ICoordinateProvider
{
    private readonly ILogger logger = Log.ForContext<ImageControlLocatorComponent>();   
    private readonly AsyncRetryPolicy policy;    
    private readonly List<TimeSpan> retrySequence = new();   
    private IHighlightRectangle highlightRectangle;
    private int retryAttempts = 5;
    private double retryInterval = 2;

    /// <summary>
    /// Toggle if bounding box is shown during playback on controls.
    /// This can help visuall debug the control location process in hierarchy
    /// </summary>
    public bool ShowBoundingBox { get; set; }  

    /// <summary>
    /// Set the theme used by the application. Target image will be retrieved based on the matching theme.
    /// </summary>
    [DataMember]
    public Argument Theme { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, DefaultValue = string.Empty };

  
    public ImageControlLocatorComponent() : base("Image Control Locator", "ImageControlLocator")
    {       
        foreach (var i in Enumerable.Range(1, retryAttempts))
        {
            retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
        }
        policy = Policy.Handle<ElementNotFoundException>()
        .WaitAndRetryAsync(retrySequence, (exception, timeSpan, retryCount, context) =>
        {
            logger.Error(exception, "Failed to locate image control."); ;
            if (retryCount < retrySequence.Count)
            {
                logger.Information($"Control lookup  will be attempated again upto {retrySequence.Count - retryCount} times.");
            }
        });

    }

    public bool CanProcessControlOfType(IControlIdentity controlIdentity)
    {
        return controlIdentity is ImageControlIdentity;
    }


    public async Task<UIControl> FindControlAsync(IControlIdentity controlDetails, UIControl searchRoot)
    {
        Guard.Argument(controlDetails).NotNull().Compatible<ImageControlIdentity>();

        ImageControlIdentity controlIdentity = controlDetails as ImageControlIdentity;
        ConfigureRetryPolicy(controlIdentity);

        var searchArea = searchRoot?.GetApiControl<BoundingBox>();
        var foundControl = await policy.ExecuteAsync(async () =>
        {
            HighlightElement(searchArea);
            var boundingBox = await TryFindMatch(controlIdentity, searchArea);
            //Transform the control bounding box relative to desktop top-left (0,0)
            if (searchArea != null)
            {
                boundingBox.X += searchArea.X;
                boundingBox.Y += searchArea.Y;
            }
            return boundingBox;
        });
        HighlightElement(foundControl);
        return await Task.FromResult(new ImageUIControl(controlIdentity, foundControl ?? throw new ElementNotFoundException($"Failed to find any control matching image  {controlDetails}")));
    }


    public async Task<IEnumerable<UIControl>> FindAllControlsAsync(IControlIdentity controlDetails, UIControl searchRoot)
    {
        Guard.Argument(controlDetails).NotNull().Compatible<ImageControlIdentity>();

        ImageControlIdentity controlIdentity = controlDetails as ImageControlIdentity;
        ConfigureRetryPolicy(controlIdentity);
        var searchArea = searchRoot?.GetApiControl<BoundingBox>();
        var foundControls = await policy.ExecuteAsync(async () =>
        {
            HighlightElement(searchArea);
            return await TryFindAllMatch(controlIdentity, searchArea);
        });
        if (!foundControls.Any())
        {
            throw new ElementNotFoundException($"Failed to find any control matching image  {controlDetails}");
        }
        return await Task.FromResult(foundControls.Select(s => new ImageUIControl(controlDetails, s)));
    }


    private async Task<BoundingBox> TryFindMatch(ImageControlIdentity controlDetails, BoundingBox searchArea)
    {
        string templateFile = await GetTemplateFile(controlDetails);
        var regionOfInterest = CaptureRegionOfInterest(searchArea);

        OpenCvSharp.Point matchingPoint;
        using (Mat templateImg = Cv2.ImRead(templateFile, ImreadModes.Grayscale))
        {
            using (Mat sourceImg = Cv2.ImDecode(regionOfInterest, ImreadModes.Grayscale))
            {
                int cols = sourceImg.Cols - templateImg.Cols + 1;
                int rows = sourceImg.Rows - templateImg.Rows + 1;
                using (Mat resultImg = new Mat(rows, cols, MatType.CV_32F))
                {
                    Cv2.MatchTemplate(sourceImg, templateImg, resultImg, controlDetails.MatchStrategy);
                    //resultImg.Normalize(0, 1, OpenCvSharp.NormTypes.MinMax);
                    Cv2.Threshold(resultImg, resultImg, controlDetails.ThreshHold, 1.0, ThresholdTypes.Tozero);

                    if (controlDetails.Index > 1)
                    {
                        List<OpenCvSharp.Point> matchingPoints = GetAllMatchingPoints(resultImg, controlDetails.MatchStrategy, controlDetails.ThreshHold);
                        if (matchingPoints.Count >= controlDetails.Index)
                        {
                            matchingPoint = matchingPoints[controlDetails.Index - 1];
                            return new BoundingBox(matchingPoint.X, matchingPoint.Y, templateImg.Width, templateImg.Height);
                        }

                        throw new ElementNotFoundException($"Element at index {controlDetails.Index} could not be located");
                    }
                    matchingPoint = GetMatchingPoint(resultImg, controlDetails.MatchStrategy, controlDetails.ThreshHold);
                    return new BoundingBox(matchingPoint.X, matchingPoint.Y, templateImg.Width, templateImg.Height);
                }
            }
        }

    }

    private async Task<IEnumerable<BoundingBox>> TryFindAllMatch(ImageControlIdentity controlDetails, BoundingBox searchArea)
    {         
        string templateFile = await GetTemplateFile(controlDetails);
        var regionOfInterest = CaptureRegionOfInterest(searchArea);
        var foundMatches = new List<BoundingBox>();
        using (Mat templateImg = Cv2.ImRead(templateFile, ImreadModes.Grayscale))
        {
            using (Mat sourceImg = Cv2.ImDecode(regionOfInterest, ImreadModes.Grayscale))
            {
                int cols = sourceImg.Cols - templateImg.Cols + 1;
                int rows = sourceImg.Rows - templateImg.Rows + 1;
                using (Mat resultImg = new Mat(rows, cols, MatType.CV_32F))
                {
                    Cv2.MatchTemplate(sourceImg, templateImg, resultImg, controlDetails.MatchStrategy);
                    //resultImg.Normalize(0, 1, OpenCvSharp.NormTypes.MinMax);
                    Cv2.Threshold(resultImg, resultImg, controlDetails.ThreshHold, 1.0, ThresholdTypes.Tozero);

                    List<OpenCvSharp.Point> matchingPoints = GetAllMatchingPoints(resultImg, controlDetails.MatchStrategy, controlDetails.ThreshHold);
                    foreach (var matchingPoint in matchingPoints)
                    {
                        foundMatches.Add(new BoundingBox(matchingPoint.X, matchingPoint.Y, templateImg.Width, templateImg.Height));
                    }                      
                }
            }
        }
        return foundMatches;
    }

    private async Task<string> GetTemplateFile(ImageControlIdentity controlDetails)
    {
        var screenCapture = this.EntityManager.GetServiceOfType<IScreenCapture>();
        var argumentProcessor = this.EntityManager.GetServiceOfType<IArgumentProcessor>();       
        (short width, short height) = screenCapture.GetScreenResolution();
        var allImages = controlDetails.GetImages();

        ImageDescription targetImage = default;

        //Find image description that matches the current theme and resolution or just the theme if a theme is specified
        if (this.Theme.IsConfigured())
        {
            string theme = await argumentProcessor.GetValueAsync<string>(this.Theme);
            logger.Information($"Configured theme is {theme}");
           
            if (!string.IsNullOrEmpty(theme))
            {
                targetImage = allImages.FirstOrDefault(a => a.Theme.Equals(theme) && a.ScreenWidth.Equals(width) && a.ScreenHeight.Equals(height))
                    ?? allImages.FirstOrDefault(a => a.Theme.Equals(theme) && a.ScreenWidth.Equals(0) && a.ScreenHeight.Equals(0));
            }
            if(targetImage == null)
            {
                logger.Warning($"Theme is specified. However, no template image could be found for configured theme : {theme}.");
            }
        }

        //Find image description that matches the current resolution if no theme is specified else use default image.
        targetImage = targetImage ?? allImages.FirstOrDefault(a => string.IsNullOrEmpty(a.Theme) && a.ScreenWidth.Equals(width) && a.ScreenHeight.Equals(height))
            ?? allImages.FirstOrDefault(a => a.IsDefault);
               
        string templateFile = targetImage?.ControlImage ?? throw new ConfigurationException($"No template image could be located. Total number of configured templates : {allImages.Count()}." +
            $"Current screen resolution is {width},{height}");;
        if (!File.Exists(templateFile))
        {
            throw new FileNotFoundException($"{templateFile} doesn't exist.");
        }
        return templateFile;
    }

    private OpenCvSharp.Point GetMatchingPoint(Mat matchData, TemplateMatchModes matchMode, float threshHold)
    {
        double minVal, maxVal;
        OpenCvSharp.Point minLoc, maxLoc;
        Cv2.MinMaxLoc(matchData, out minVal, out maxVal, out minLoc, out maxLoc);

        switch (matchMode)
        {
            case TemplateMatchModes.SqDiff:
            case TemplateMatchModes.SqDiffNormed:
                if (minVal > threshHold)
                {
                    logger.Information("Image match located {@maxLoc} with {maxVal} %", maxLoc, maxVal * 100);
                    return new OpenCvSharp.Point(minLoc.X, minLoc.Y);
                }
                else
                {
                    throw new ElementNotFoundException($"Image could not be located against configured threshold of {threshHold}.Best match located has only {maxVal}% accuracy");
                }
            case TemplateMatchModes.CCoeff:
            case TemplateMatchModes.CCoeffNormed:
            case TemplateMatchModes.CCorr:
            case TemplateMatchModes.CCorrNormed:
                if (maxVal > threshHold)
                {
                    logger.Information("Image match located {@maxLoc} with {maxVal} %", maxVal, maxLoc);
                    return new OpenCvSharp.Point(maxLoc.X, maxLoc.Y);
                }
                else
                {
                    throw new ElementNotFoundException($"Image could not be located against configured threshold of {threshHold}.Best match located has only {maxVal}% accuracy");
                }
            default:
                throw new ArgumentException($"Configured MatchMode is not supported by OpenCv for template matching");
        }
    }

    private List<OpenCvSharp.Point> GetAllMatchingPoints(Mat matchData, TemplateMatchModes matchMode, float threshHold)
    {
        List<OpenCvSharp.Point> matchingPoints = new List<OpenCvSharp.Point>();
        while (true)
        {
            try
            {
                OpenCvSharp.Point matchingPoint = GetMatchingPoint(matchData, matchMode, threshHold);
                matchingPoints.Add(matchingPoint);
                Rect outRect;
                Cv2.FloodFill(matchData, new OpenCvSharp.Point(matchingPoint.X, matchingPoint.Y), new Scalar(0), out outRect, new Scalar(0.1), new Scalar(1.0), FloodFillFlags.FixedRange);
            }
            catch
            {
                if (matchingPoints.Count >= 1)
                    break;
                else
                    throw;
            }

        }
        return matchingPoints;

    }

    private byte[] CaptureRegionOfInterest(BoundingBox searchArea)
    {
        var screenCapture = this.EntityManager.GetServiceOfType<IScreenCapture>();
        if (searchArea != null)
        {
            return screenCapture.CaptureArea(searchArea);
        }
        else
        {
            return screenCapture.CaptureDesktop();
        }
    }

    private void ConfigureRetryPolicy(IControlIdentity controlIdentity)
    {
        if (this.retryAttempts != controlIdentity.RetryAttempts || this.retryInterval != controlIdentity.RetryInterval)
        {
            this.retryAttempts = controlIdentity.RetryAttempts;
            this.retryInterval = controlIdentity.RetryInterval;
            retrySequence.Clear();
            foreach (var i in Enumerable.Range(1, this.retryAttempts))
            {
                retrySequence.Add(TimeSpan.FromSeconds(this.retryInterval));
            }
        }
    }

    private void HighlightElement(BoundingBox foundControl)
    {

        if (ShowBoundingBox && foundControl != null)
        {
            if (highlightRectangle == null)
            {
                highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
            }

            var boundingBox = new BoundingBox(foundControl.X, foundControl.Y, foundControl.Width, foundControl.Height);
            if (!boundingBox.Equals(BoundingBox.Empty))
            {
                highlightRectangle.Visible = true;

                highlightRectangle.Location = boundingBox;
                Thread.Sleep(500);

                highlightRectangle.Visible = false;

            }
        }

    }

    #region ICoordinateProvider   

    ///<inheritdoc/>
    public async Task<(double, double)> GetClickablePoint(IControlIdentity controlDetails)
    {
        var bounds = await GetScreenBounds(controlDetails);
        controlDetails.GetClickablePoint(bounds, out double x, out double y);
        return await Task.FromResult((x, y));
    }

    ///<inheritdoc/>
    public async Task<BoundingBox> GetScreenBounds(IControlIdentity controlIdentity)
    {
        var targetControl = await this.FindControlAsync(controlIdentity, null);
        var screenBounds = await GetBoundingBox(targetControl);
        return screenBounds;
    }

    ///<inheritdoc/>
    public async Task<BoundingBox> GetBoundingBox(object control)
    {
        if (control is BoundingBox boundingBox)
        {
            return await Task.FromResult(new BoundingBox(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height));
        }
        throw new ArgumentException("control must be of type BoundingBox");
    }

    #endregion ICoordinateProvider  
}
