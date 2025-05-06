namespace Lib.Interfaces;

public interface ILogger
{
    enum Color
    {
        Red,
        Blue,
        Green
    }

    public enum Level
    {
        Info,
        Error,
        Figlet
    }

    public enum Options
    {
        Bold,
        Italics
    }

    public void Log(Level level, string message, Options? options = null);
    public Task<T> WithProgress<T>(string title, Func<Action<decimal>, Task<T>> operation);
    public T Prompt<T>(string message) where T : struct, Enum;
    public string Prompt(string message, IEnumerable<string> list);
    public string Prompt(string message, bool allowEmpty);
}