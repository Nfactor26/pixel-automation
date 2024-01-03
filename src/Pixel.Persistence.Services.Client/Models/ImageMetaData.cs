using System;

namespace Pixel.Persistence.Services.Client.Models;

/// <summary>
/// Metadata for a control image
/// </summary>
/// <param name="ApplicationId"></param>
/// <param name="ControlId"></param>
/// <param name="Version"></param>
/// <param name="FileName"></param>
/// <param name="LastUpdated"></param>
public record ControlImageMetaData(string ApplicationId, string ControlId, string Version, string FileName, DateTime LastUpdated);

/// <summary>
/// Metadata for an image captured as part of tracing
/// </summary>
/// <param name="SessionId"></param>
/// <param name="TestResultId"></param>
public record TraceImageMetaData(string SessionId, string TestResultId);