using Quartz.Application;
using static Quartz.Application.Interpreter;

namespace Quartz.Presentation;

class Program
{
	private static void Main(string[] paths)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Options options = new() { LogLexing = false, LogParsing = false };
		Interpreter interpreter = new(options);
		if (paths.Length < 1)
		{
			RunInteractiveMode(interpreter);
			return;
		}
		RunScriptMode(interpreter, paths);
		Console.ReadKey();
	}

	private static void RunScriptMode(Interpreter interpreter, string[] paths)
	{
		foreach (string path in paths)
		{
			string? code = Fetch(path);
			if (code == null)
			{
				Console.WriteLine($"Unable to read code at '{path}'");
				continue;
			}
			interpreter.Run(code);
		}
	}

	private static void RunInteractiveMode(Interpreter interpreter)
	{
		foreach (string instruction in ReadInstructions())
		{
			interpreter.Run(instruction);
		}
	}

	private static string? ExternalFetch(string address)
	{
		try
		{
			using HttpClient client = new();
			HttpResponseMessage response = client.GetAsync(address).Result;
			response.EnsureSuccessStatusCode();
			return response.Content.ReadAsStringAsync().Result;
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static string? InternalFetch(string address)
	{
		FileInfo file = new(address);
		try
		{
			using StreamReader reader = file.OpenText();
			return reader.ReadToEnd();
		}
		catch (FileNotFoundException)
		{
			Console.WriteLine(file.FullName);
			return null;
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static string? Fetch(string address)
	{
		return InternalFetch(address) ?? ExternalFetch(address);
	}

	private static IEnumerable<string> ReadInstructions()
	{
		while (true)
		{
			string? input = Console.ReadLine();
			if (input == null) yield break;
			yield return input;
		}
	}
}
