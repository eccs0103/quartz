using Quartz.Application.Evaluating;
using Quartz.Application.Lexing;
using Quartz.Application.Metadata;
using Quartz.Application.Parsing;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Lexing;
using Quartz.Domain.Parsing;
using Quartz.Shared;

namespace Quartz.Application;

public class Interpreter(Interpreter.Options options)
{
	public class Options
	{
		public bool LogLexing { get; set; } = false;
		public bool LogParsing { get; set; } = false;
	}

	private static Lexer Lexer { get; } = new();
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
			Token[] tokens = Lexer.Tokenize(input);
#if DEBUG
			if (LogLexing && tokens.Length > 0) Console.WriteLine(string.Join<Token>(Environment.NewLine, tokens));
#endif
			Console.ForegroundColor = ConsoleColor.Magenta;
			List<Node> trees = Parser.Parse(tokens);
#if DEBUG
			if (LogParsing && tokens.Length > 0) Console.WriteLine(string.Join(Environment.NewLine, trees));
#endif
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

	public void WriteHeader()
	{
		Console.WriteLine(Reflection.Generate(Runtime));
	}

	public void RunInteractiveMode()
	{
		foreach (string instruction in Source.ReadInstructions())
		{
			this.Run(instruction);
		}
	}

	public void RunScriptMode(string[] paths)
	{
		foreach (string path in paths)
		{
			string? code = Source.Fetch(path);
			if (code == null)
			{
				Console.WriteLine($"Unable to read code at '{path}'");
				continue;
			}
			this.Run(code);
		}
	}
}
