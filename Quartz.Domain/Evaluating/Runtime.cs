using Quartz.Domain.Parsing;
using static System.Math;

namespace Quartz.Domain.Evaluating;

public class Runtime
{
	private RuntimeBuilder Builder { get; } = new();
	private Evaluator Evaluator { get; } = new();

	public Runtime()
	{
		Builder.DeclareModule(static (module) =>
		{
			module.DeclareClass("Type", static (type) =>
			{
			});
			module.DeclareClass("Function", static (type) =>
			{
			});
			module.DeclareClass("Null", static (type) =>
			{
			});
			module.DeclareClass("Boolean", static (type) =>
			{
			});
			module.DeclareClass("String", static (type) =>
			{
				type.DeclareOperation("+", ["String", "String"], "String", static (string @this, string other) =>
				{
					return @this + other;
				});
				type.DeclareOperation("=", ["String", "String"], "Boolean", static (string @this, string other) =>
				{
					return @this == other;
				});
				type.DeclareOperation("!=", ["String", "String"], "Boolean", static (string @this, string other) =>
				{
					return @this != other;
				});
			});
			module.DeclareClass("Number", static (type) =>
			{
				type.DeclareOperation("+", ["Number"], "Number", static (double @this) =>
				{
					return +@this;
				});
				type.DeclareOperation("+", ["Number", "Number"], "Number", static (double @this, double other) =>
				{
					return @this + other;
				});
				type.DeclareOperation("-", ["Number"], "Number", static (double @this) =>
				{
					return -@this;
				});
				type.DeclareOperation("-", ["Number", "Number"], "Number", static (double @this, double other) =>
				{
					return @this - other;
				});
				type.DeclareOperation("*", ["Number", "Number"], "Number", static (double @this, double other) =>
				{
					return @this * other;
				});
				type.DeclareOperation("/", ["Number", "Number"], "Number", static (double @this, double other) =>
				{
					return @this / other;
				});
				type.DeclareOperation("=", ["Number", "Number"], "Boolean", static (double @this, double other) =>
				{
					return @this == other;
				});
				type.DeclareOperation("!=", ["Number", "Number"], "Boolean", static (double @this, double other) =>
				{
					return @this != other;
				});
				type.DeclareOperation("<", ["Number", "Number"], "Boolean", static (double @this, double other) =>
				{
					return @this < other;
				});
				type.DeclareOperation("<=", ["Number", "Number"], "Boolean", static (double @this, double other) =>
				{
					return @this <= other;
				});
				type.DeclareOperation(">", ["Number", "Number"], "Boolean", static (double @this, double other) =>
				{
					return @this > other;
				});
				type.DeclareOperation(">=", ["Number", "Number"], "Boolean", static (double @this, double other) =>
				{
					return @this >= other;
				});
			});
			module.DeclareClass(RuntimeBuilder.NameWorkspace, static (type) =>
			{
				type.DeclareConstant("pi", "Number", PI);
				type.DeclareConstant("e", "Number", E);
				type.DeclareOperation("read", ["String"], "String", static (string message) =>
				{
					Console.Write(message);
					string? input = Console.ReadLine();
					ArgumentNullException.ThrowIfNull(input);
					return input;
				});
				type.DeclareOperation("write", ["Number"], "Null", static (double value) =>
				{
					Console.WriteLine(value);
				});
				type.DeclareOperation("write", ["Boolean"], "Null", static (bool value) =>
				{
					Console.WriteLine(value);
				});
				type.DeclareOperation("write", ["String"], "Null", static (string value) =>
				{
					Console.WriteLine(value);
				});
			});
		});
	}

	public void Evaluate(IEnumerable<Node> nodes)
	{
		foreach (Node node in nodes) node.Accept(Evaluator, RuntimeBuilder.Workspace);
	}
}
