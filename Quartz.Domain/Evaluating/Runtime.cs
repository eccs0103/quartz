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
			module.DeclareClass("Any", null, static (type) =>
			{
				type.DeclareOperation("is_equal", ["Any", "Any"], "Boolean", static (object @this, object other) =>
				{
					if (@this == null) return other == null;
					return @this.Equals(other);
				});
				type.DeclareOperation("=", ["Any", "Any"], "Boolean", static (object @this, object other) =>
				{
					if (@this == null) return other == null;
					return @this.Equals(other);
				});
				type.DeclareOperation("!=", ["Any", "Any"], "Boolean", static (object @this, object other) =>
				{
					if (@this == null) return other != null;
					return !@this.Equals(other);
				});
			});
			module.DeclareClass("Number", "Any", static (type) =>
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
			module.DeclareClass("Boolean", "Any", static (type) =>
			{
				type.DeclareOperation("!", ["Boolean"], "Boolean", static (bool @this) =>
				{
					return !@this;
				});
				type.DeclareOperation("&", ["Boolean", "Boolean"], "Boolean", static (bool @this, bool other) =>
				{
					return @this && other;
				});
				type.DeclareOperation("|", ["Boolean", "Boolean"], "Boolean", static (bool @this, bool other) =>
				{
					return @this || other;
				});
			});
			module.DeclareClass("String", "Any", static (type) =>
			{
				type.DeclareOperation("+", ["String", "String"], "String", static (string @this, string other) =>
				{
					return @this + other;
				});
			});
			module.DeclareClass("Null", "Any", static (type) =>
			{
			});
			module.DeclareClass("Function", "Any", static (type) =>
			{
			});
			module.DeclareClass("Type", "Any", static (type) =>
			{
			});
			module.DeclareClass(RuntimeBuilder.NameWorkspace, "Any", static (type) =>
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
				type.DeclareOperation("write", ["Null"], "Null", static (object? value) =>
				{
					Console.WriteLine(value?.ToString() ?? "null");
				});
			});
		});
	}

	public void Evaluate(IEnumerable<Node> nodes)
	{
		foreach (Node node in nodes) node.Accept(Evaluator, RuntimeBuilder.Workspace);
	}
}
