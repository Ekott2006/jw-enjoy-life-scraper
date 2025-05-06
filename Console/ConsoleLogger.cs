using Lib.Interfaces;
using Spectre.Console;

namespace Console;

public class ConsoleLogger : ILogger
{
    public string Prompt(string message, IEnumerable<string> list)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(LogTextGenerator(message, ILogger.Color.Blue))
                .AddChoices(list));
    }

    public T Prompt<T>(string message) where T : struct, Enum
    {
        string[] options = Enum.GetNames<T>();
        string selected = Prompt(message, options);
        return Enum.Parse<T>(selected, true);
    }

    public string Prompt(string message, bool allowEmpty)
    {
        TextPrompt<string> prompt = new(LogTextGenerator(message, ILogger.Color.Blue));
        if (allowEmpty) prompt.AllowEmpty();
        return AnsiConsole.Prompt(prompt);
    }

    public void Log(ILogger.Level level, string message, ILogger.Options? options = null)
    {
        switch (level)
        {
            case ILogger.Level.Info:
                AnsiConsole.MarkupLine(LogTextGenerator(message, ILogger.Color.Blue, options));
                break;
            case ILogger.Level.Error:
                AnsiConsole.MarkupLine(LogTextGenerator("ERROR: " + message, ILogger.Color.Red,
                    options ?? ILogger.Options.Bold));
                break;
            case ILogger.Level.Figlet:
                AnsiConsole.Write(new FigletText(message).Color(Color.Blue));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }

    public async Task<T> WithProgress<T>(string title, Func<Action<decimal>, Task<T>> operation)
    {
        return await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                ProgressTask task = ctx.AddTask(LogTextGenerator(title, ILogger.Color.Green));
                return await operation(value => task.Value = (double)value);
            });
    }


    private static string LogTextGenerator(string message, ILogger.Color? color = null, ILogger.Options? options = null)
    {
        string styles = options switch
        {
            ILogger.Options.Italics => " italic",
            ILogger.Options.Bold => " bold",
            _ => ""
        };
        string colorText = color switch
        {
            ILogger.Color.Blue => "blue",
            ILogger.Color.Green => "green",
            ILogger.Color.Red => "red",
            _ => "blue"
        };
        string text = $"[{colorText}{styles}]{message}[/]";
        return text;
    }
}