using Serilog;

namespace rbbl.buildingblocks.Logging;

public class SerilogLogger<T> : IAppLogger<T>
{
    private readonly ILogger _logger;
    public SerilogLogger() => _logger = Log.ForContext<T>();
    public void Trace(string message, params object[] args) => _logger.Debug(message, args);
    public void Info(string message, params object[] args)  => _logger.Information(message, args);
    public void Warn(string message, params object[] args)  => _logger.Warning(message, args);
    public void Error(Exception ex, string message, params object[] args) => _logger.Error(ex, message, args);
}
