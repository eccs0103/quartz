using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Lexing;
using Quartz.Domain.Parsing;

namespace Quartz.Application;

public class Interpreter(Interpreter.Options options)
{
	public class Options
	{
		public bool LogLexing { get; set; } = false;
		public bool LogParsing { get; set; } = false;
	}

	private static Tokenizer Tokenizer { get; } = new();
	private static Parser Parser { get; } = new();
	private static Runtime Runtime { get; } = new();
	private bool LogLexing { get; } = options.LogLexing;
	private bool LogParsing { get; } = options.LogParsing;

	public Interpreter() : this(new Options())
	{
	}

	public void Run(string input)
	{
		ConsoleColor foreground = Console.ForegroundColor;
		try
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Token[] tokens = Tokenizer.Tokenize(input);
			if (LogLexing && tokens.Length > 0) Console.WriteLine(string.Join<Token>('\n', tokens));

			Console.ForegroundColor = ConsoleColor.Magenta;
			List<Node> trees = Parser.Parse(tokens);
			if (LogParsing && tokens.Length > 0) Console.WriteLine(string.Join('\n', trees));

			Console.ForegroundColor = ConsoleColor.Yellow;
			Runtime.Evaluate(trees);
		}
		catch (Issue issue)
		{
			Console.WriteLine(issue.Message);
		}
		catch (Exception exception)
		{
			ConsoleColor temporary = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(exception.ToString());
			Console.ForegroundColor = temporary;
		}
		Console.ForegroundColor = foreground;
	}
}
