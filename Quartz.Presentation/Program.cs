using System;
using System.Linq;
using Quartz.Application;

namespace Quartz.Presentation;

class Program
{
	private static void Main(string[] args)
	{
		Interpreter interpreter = new();

		if (args.Length < 1)
		{
			interpreter.RunInteractiveMode();
			return;
		}

		if (args.SingleOrDefault() == "--header")
		{
			interpreter.WriteHeader();
			return;
		}

		interpreter.RunScriptMode(args);
	}
}
