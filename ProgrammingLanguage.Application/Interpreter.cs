using ProgrammingLanguage.Application.Evaluating;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Lexing;
using ProgrammingLanguage.Application.Parsing;

namespace ProgrammingLanguage.Application;

public class Interpreter(Interpreter.Options options)
{
	public class Options
	{
		public bool LogLexing { get; set; } = false;
		public bool LogParsing { get; set; } = false;
	}

	private static readonly Tokenizer Tokenizer = new();
	private static readonly Parser Parser = new();
	private static readonly Evaluator Evaluator = new();
	private readonly bool LogLexing = options.LogLexing;
	private readonly bool LogParsing = options.LogParsing;

	public Interpreter() : this(new())
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
			Evaluator.Evaluate(trees);
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
