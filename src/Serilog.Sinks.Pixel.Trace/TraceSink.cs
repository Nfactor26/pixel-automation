using Pixel.Automation.Core;
using Pixel.Automation.Core.Enums;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.Pixel.Trace;

/// <summary>
/// TraceSink emits the logs to TraceManager which are later processed and saved along with the test case result which was executing
/// while the logs were emitted.
/// </summary>
public class TraceSink : ILogEventSink
{
    private readonly IFormatProvider formatProvider;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="formatProvider"></param>
    public TraceSink(IFormatProvider formatProvider)
    {      
        this.formatProvider = formatProvider;
    }

    ///<inheritdoc/>
    public void Emit(LogEvent logEvent)
    {        
        if (TraceManager.IsEnabled && logEvent.Properties.ContainsKey(TraceManager.CaptureTrace))
        {
            var message = logEvent.RenderMessage(formatProvider);
            if(Enum.TryParse<TraceLevel>(logEvent.Level.ToString(), out TraceLevel traceLevel))
            {
                TraceManager.AddMessage(traceLevel, message);
            }
        }       
    }
}

/// <summary>
/// Extension methods for TraceSink configuration
/// </summary>
public static class TraceSinkExtensions
{
    public static LoggerConfiguration Trace(this LoggerSinkConfiguration loggerConfiguration, IFormatProvider formatProvider = null)
    {
        return loggerConfiguration.Sink(new TraceSink(formatProvider));
    }
}