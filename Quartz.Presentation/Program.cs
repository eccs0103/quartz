using System;
using System.Linq;
using Quartz.Application;

namespace Quartz.Presentation;

class Program
{
	private static void Main(string[] arguments)
	{
		Interpreter interpreter = new();

		if (arguments.Length < 1)
		{
			interpreter.RunInteractiveMode();
			return;
		}

		if (arguments.SingleOrDefault() == "--header")
		{
			interpreter.WriteHeader();
			return;
		}

		interpreter.RunScriptMode(arguments);
	}
}
