using System;
using System.IO;
using System.Net.Http;

namespace Quartz.Shared;

public static class Source
{
	public static IEnumerable<string> ReadInstructions()
	{
		while (true)
		{
			string? input = Console.ReadLine();
			if (input == null) yield break;
			yield return input;
		}
	}

	public static string? Fetch(string address)
	{
		return InternalFetch(address) ?? ExternalFetch(address);
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
			Console.WriteLine($"Unable to read code at '{address}'");
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
			Console.WriteLine($"File at '{file.FullName}' not found");
			return null;
		}
		catch (Exception)
		{
			Console.WriteLine($"Unable to read code at '{file.FullName}'");
			return null;
		}
	}
}
