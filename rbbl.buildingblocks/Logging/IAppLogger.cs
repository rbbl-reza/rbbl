namespace rbbl.buildingblocks.Logging;

public interface IAppLogger<T>
{
    void Trace(string message, params object[] args);
    void Info(string message, params object[] args);
    void Warn(string message, params object[] args);
    void Error(Exception ex, string message, params object[] args);
}
