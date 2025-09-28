using ProgrammingLanguage.Application;
using static ProgrammingLanguage.Application.Interpreter;

namespace ProgrammingLanguage.Presentation;

class Program
{
	private static void Main(string[] paths)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Options options = new() { LogLexing = false, LogParsing = true };
		Interpreter interpreter = new(options);

		foreach (string path in paths)
		{
			string? code = Fetch(path);
			if (code == null) continue;
			interpreter.Run(code);
		}
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
		catch (Exception exception)
		{
			Console.WriteLine(exception.ToString());
			return null;
		}
	}

	private static string? InternalFetch(string address)
	{
		try
		{
			FileInfo file = new(address);
			using StreamReader reader = file.OpenText();
			return reader.ReadToEnd();
		}
		catch (Exception exception)
		{
			Console.WriteLine(exception.ToString());
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
			ArgumentNullException.ThrowIfNull(input, nameof(input));
			yield return input;
		}
	}
}
