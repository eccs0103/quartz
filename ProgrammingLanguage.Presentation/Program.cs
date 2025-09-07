using ProgrammingLanguage.Application;
using static ProgrammingLanguage.Application.Interpreter;

namespace ProgrammingLanguage.Presentation;

class Program
{
	private static void Main(string[] args)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Options options = new() { LogParsing = false };
		Interpreter interpreter = new(options);
		foreach (string instruction in GenerateInstructions(args)) interpreter.Run(instruction);
	}

	private static IEnumerable<string> GenerateInstructions(IEnumerable<string> initial)
	{
		// foreach (string instruction in initial)
		// {
		// 	string input = $"import \"{instruction}\";";
		// 	Console.WriteLine(input);
		// 	yield return input;
		// }
		while (true)
		{
			string? input = Console.ReadLine();
			ArgumentNullException.ThrowIfNull(input, nameof(input));
			yield return input;
		}
	}
}
