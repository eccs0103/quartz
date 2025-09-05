using ProgrammingLanguage.Application.Evaluating;
using ProgrammingLanguage.Application.Lexing;
using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Exceptions;

namespace ProgrammingLanguage.Application;

public class Interpreter(Interpreter.Options options)
{
	public enum RunModes : byte
	{
		Debug,
		Run,
	}
	public class Options
	{
		public RunModes? Mode { get; set; }
		public void Deconstruct(out RunModes mode)
		{
			mode = Mode ?? RunModes.Run;
		}
	}

	private static readonly Tokenizer Tokenizer = new();
	private static readonly Parser Parser = new();
	private static readonly Evalutor Evalutor = new();
	public void Run(in string input)
	{
		options.Deconstruct(out RunModes mode);

		ConsoleColor foreground = Console.ForegroundColor;
		try
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Token[] tokens = Tokenizer.Tokenize(input);
			if (mode == RunModes.Debug && tokens.Length > 0) Console.WriteLine(string.Join<Token>('\n', tokens));

			Console.ForegroundColor = ConsoleColor.Blue;
			List<Node> trees = Parser.Parse(tokens);
			if (mode == RunModes.Debug && tokens.Length > 0) Console.WriteLine(string.Join('\n', trees));

			Console.ForegroundColor = ConsoleColor.Yellow;
			Evaluate(trees);
		}
		catch (Issue error)
		{
			Console.WriteLine(error.Message);
		}
		catch (Exception)
		{
			throw;
		}
		Console.ForegroundColor = foreground;
	}
}
