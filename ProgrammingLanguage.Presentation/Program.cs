using ProgrammingLanguage.Application;
using static ProgrammingLanguage.Application.Interpreter;

namespace ProgrammingLanguage.Presentation;

class Program
{
	private static IEnumerable<string> GenerateInstructions(IEnumerable<string> initial)
	{
		foreach (string instruction in initial)
		{
			string input = $"import \"{instruction}\";";
			Console.WriteLine(input);
			yield return input;
		}
		while (true)
		{
			yield return Console.ReadLine() ?? throw new NullReferenceException($"Input cant be null");
		}
	}
	private static void Main(string[] args)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Options options = new() { Mode = RunModes.Run };
		Interpreter interpreter = new(options);
		foreach (string instruction in GenerateInstructions(args))
		{
			interpreter.Run(instruction);
		}
	}
}
