using Dawn;
using OpenCvSharp;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Pixel.Automation.Image.Matching.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Image Locator", "Control Locators", iconSource: null, description: "Identify a image control on screen", tags: new string[] { "Locator" })]
    public class ImageControlLocatorComponent : ServiceComponent, IControlLocator<BoundingBox, BoundingBox>, ICoordinateProvider
    {
        private readonly ILogger logger = Log.ForContext<ImageControlLocatorComponent>();


        [NonSerialized]
        bool showBoundingBox;
        /// <summary>
        /// Toggle if bounding box is shown during playback on controls.
        /// This can help visuall debug the control location process in hierarchy
        /// </summary>
        public bool ShowBoundingBox
        {
            get
            {
                return showBoundingBox;
            }
            set
            {
                showBoundingBox = value;
            }
        }

        [NonSerialized]
        int retryAttempts = 2;
        [Browsable(false)]
        [IgnoreDataMember]
        protected int RetryAttempts
        {
            get
            {
                return retryAttempts;
            }
            set
            {
                if (value == retryAttempts)
                {
                    return;
                }
                retryAttempts = value;
                retrySequence.Clear();
                foreach (var i in Enumerable.Range(1, value))
                {
                    retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
                }
            }
        }

        [NonSerialized]
        double retryInterval = 5;
        [DataMember]
        [Browsable(false)]
        [IgnoreDataMember]
        protected double RetryInterval
        {
            get
            {
                return retryInterval;
            }
            set
            {
                if (value == retryInterval)
                {
                    return;
                }
                retryInterval = value;
                retrySequence.Clear();
                foreach (var i in Enumerable.Range(1, retryAttempts))
                {
                    retrySequence.Add(TimeSpan.FromSeconds(value));
                }
            }
        }

        [NonSerialized]
        private RetryPolicy policy;

        [NonSerialized]
        private List<TimeSpan> retrySequence;

        [NonSerialized]
        private IHighlightRectangle highlightRectangle;

        [NonSerialized]
        private IScreenCapture screenCapture;

        public ImageControlLocatorComponent() : base("Image Control Locator", "ImageControlLocator")
        {
            retrySequence = new List<TimeSpan>();
            foreach (var i in Enumerable.Range(1, retryAttempts))
            {
                retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
            }
            policy = Policy.Handle<ElementNotFoundException>()
            .WaitAndRetry(retrySequence, (exception, timeSpan, retryCount, context) =>
            {
                logger.Error(exception, exception.Message); ;
                if (retryCount < retrySequence.Count)
                {
                    logger.Information("Control lookup  will be attempated again.");
                }
            });

        }

        public bool CanProcessControlOfType(IControlIdentity controlIdentity)
        {
            return controlIdentity is ImageControlIdentity;
        }


        public BoundingBox FindControl(IControlIdentity controlDetails, BoundingBox searchArea)
        {
            Guard.Argument(controlDetails).NotNull().Compatible<ImageControlIdentity>();

            ImageControlIdentity controlIdentity = controlDetails as ImageControlIdentity;
            ConfigureRetryPolicy(controlIdentity);

            var foundControl = policy.Execute(() =>
            {
                HighlightElement(searchArea);
                var boundingBox = TryFindMatch(controlIdentity, searchArea);
                //Transform the control bounding box relative to desktop top-left (0,0)
                if (searchArea != null)
                {
                    boundingBox.X += searchArea.X;
                    boundingBox.Y += searchArea.Y;
                }
                return boundingBox;
            });
            HighlightElement(foundControl);
            return foundControl ?? throw new ElementNotFoundException($"Failed to find any control matching image  {controlDetails}");
        }


        public IEnumerable<BoundingBox> FindAllControls(IControlIdentity controlDetails, BoundingBox searchArea)
        {
            Guard.Argument(controlDetails).NotNull().Compatible<ImageControlIdentity>();

            ImageControlIdentity controlIdentity = controlDetails as ImageControlIdentity;
            ConfigureRetryPolicy(controlIdentity);

            var foundControls = policy.Execute(() =>
            {
                HighlightElement(searchArea);
                return TryFindAllMatch(controlIdentity, searchArea);
            });
            if (!foundControls.Any())
            {
                throw new ElementNotFoundException($"Failed to find any control matching image  {controlDetails}");
            }
            return foundControls;
        }


        private BoundingBox TryFindMatch(ImageControlIdentity controlDetails, BoundingBox searchArea)
        {
            //Make sure the template file exists
            string templateFile = controlDetails.GetTemplateImage(System.Drawing.Size.Empty);
            if (!File.Exists(templateFile))
            {
                throw new FileNotFoundException($"{templateFile} doesn't exist.");
            }

            CaptureRegionOfInterest(searchArea);

            OpenCvSharp.Point matchingPoint;
            using (Mat templateImg = Cv2.ImRead(templateFile, ImreadModes.Grayscale))
            {
                using (Mat sourceImg = Cv2.ImRead("RegionOfInterest.Png", ImreadModes.Grayscale))
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

        private IEnumerable<BoundingBox> TryFindAllMatch(ImageControlIdentity controlDetails, BoundingBox searchArea)
        {
            //Make sure the template file exists
            string templateFile = controlDetails.ControlImage;
            if (!File.Exists(templateFile))
            {
                throw new FileNotFoundException($"{templateFile} doesn't exist.");
            }

            CaptureRegionOfInterest(searchArea);

            using (Mat templateImg = Cv2.ImRead(templateFile, ImreadModes.Grayscale))
            {
                using (Mat sourceImg = Cv2.ImRead("RegionOfInterest.Png", ImreadModes.Grayscale))
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
                            yield return new BoundingBox(matchingPoint.X, matchingPoint.Y, templateImg.Width, templateImg.Height);
                        }
                        yield break;
                    }
                }
            }

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

        private void CaptureRegionOfInterest(BoundingBox searchArea)
        {
            if (this.screenCapture == null)
            {
                this.screenCapture = this.EntityManager.GetServiceOfType<IScreenCapture>();
            }
            if (searchArea != null)
            {
                this.screenCapture.CaptureArea(searchArea.GetBoundingBoxAsRectangle()).Save("RegionOfInterest.Png", System.Drawing.Imaging.ImageFormat.Png);
            }
            else
            {
                this.screenCapture.CaptureDesktop().Save("RegionOfInterest.Png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void ConfigureRetryPolicy(IControlIdentity controlIdentity)
        {
            RetryAttempts = controlIdentity.RetryAttempts;
            RetryInterval = controlIdentity.RetryInterval;
        }

        private void HighlightElement(BoundingBox foundControl)
        {

            if (showBoundingBox && foundControl != null)
            {
                if (highlightRectangle == null)
                {
                    highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
                }

                var boundingBox = new Rectangle(foundControl.X, foundControl.Y, foundControl.Width, foundControl.Height);
                if (boundingBox != Rectangle.Empty)
                {
                    highlightRectangle.Visible = true;

                    highlightRectangle.Location = boundingBox;
                    Thread.Sleep(500);

                    highlightRectangle.Visible = false;

                }
            }

        }

        public void GetClickablePoint(IControlIdentity controlIdentity, out double x, out double y)
        {
            GetScreenBounds(controlIdentity, out Rectangle bounds);
            controlIdentity.GetClickablePoint(bounds, out x, out y);
        }

        public void GetScreenBounds(IControlIdentity controlIdentity, out Rectangle screenBounds)
        {
            var targetControl = this.FindControl(controlIdentity, null);
            screenBounds = GetBoundingBox(targetControl);
        }

        public Rectangle GetBoundingBox(object control)
        {
            if (control is BoundingBox boundingBox)
            {
                return new Rectangle(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
            }
            throw new ArgumentException("");
        }
    }
}
