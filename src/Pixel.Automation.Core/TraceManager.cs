using Pixel.Automation.Core.Enums;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core;

/// <summary>
/// TraceData represents a log message or an image captured while executing a test case
/// </summary>
/// <param name="RecordedAt"></param>
/// <param name="TraceType"></param>
/// <param name="TraceLevel"></param>
/// <param name="Content"></param>
public record TraceData(DateTime RecordedAt, TraceType TraceType, TraceLevel TraceLevel, string Content);

/// <summary>
/// TraceManager is responsible for collecting emitted messages and images and processing these in to <see cref="TraceData"/> during execution of test case.
/// </summary>
public static class TraceManager
{
    public static readonly string CaptureTrace = nameof(CaptureTrace);
    
    private static readonly ICollection<TraceData> traces = new List<TraceData>(32);
       
    private static bool isCapturing;
       
    private static bool isEnabled;
    /// <summary>
    /// Indicates if TraceManager is enabled
    /// </summary>
    public static bool IsEnabled 
    {
        get => isEnabled && isCapturing;
        set => isEnabled = value;        
    }

    /// <summary>
    /// Add a message to the TraceManager
    /// </summary>
    /// <param name="traceLevel"></param>
    /// <param name="message"></param>
    public static void AddMessage(TraceLevel traceLevel, string message)
    {
        if (IsEnabled)
        {
            traces.Add(new TraceData(DateTime.UtcNow, TraceType.Message, traceLevel, message));
        }
    }

    /// <summary>
    /// Add an image to the TraceManager
    /// </summary>
    /// <param name="imageFile"></param>
    public static void AddImage(string imageFile)
    {
        if (IsEnabled)
        {
            traces.Add(new TraceData(DateTime.UtcNow, TraceType.Image, TraceLevel.Information, imageFile));
        }
    }

    /// <summary>
    /// Signal the TraceManager to start capturing of <see cref="TraceData"/>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static void StartCapture() 
    {
        if(traces.Count > 0)
        {
            throw new InvalidOperationException("Capture still in progress. EndCapture should be invoked before starting a new capture.");
        }
        isCapturing = true;
    }

    /// <summary>
    /// Retrieve all the <see cref="TraceData"/> captured so far and stop capturing
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<TraceData> EndCapture()
    {
        foreach(var trace in traces)
        {
            yield return trace;
        }       
        traces.Clear();
        isCapturing = false;
        yield break;
    }
}
