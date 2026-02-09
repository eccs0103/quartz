using System.Globalization;
using Quartz.Domain.Evaluating;
using Quartz.Domain.Parsing;
using static System.Math;

namespace Quartz.Application.Evaluating;

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
				type.DeclareOperation("is_equal", ["Any"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					object left = @this.Value;
					object right = other.Value;
					bool result = left.Equals(right);
					return new Instance("Boolean", result);
				});
				type.DeclareOperation("=", ["Any"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					Instance isEqual = @this.RunOperation("is_equal", [other], scope, range);
					return isEqual;
				});
				type.DeclareOperation("!=", ["Any"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					Instance isEqual = @this.RunOperation("=", [other], scope, range);
					Instance isNotEqual = isEqual.RunOperation("!", [], scope, range);
					return isNotEqual;
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					object value = @this.Value;
					string result = value.ToString() ?? "null";
					return new Instance("String", result);
				});
			});
			module.DeclareClass("Nullable", "Any", static (type) =>
			{
			});
			module.DeclareClass("Number", "Any", static (type) =>
			{
				type.DeclareOperation("+", [], "Number", static (@this, arguments, scope, range) =>
				{
					double result = +@this.ValueAs<double>();
					return new Instance("Number", result);
				});
				type.DeclareOperation("+", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					double result = @this.ValueAs<double>() + other.ValueAs<double>();
					return new Instance("Number", result);
				});
				type.DeclareOperation("-", [], "Number", static (@this, arguments, scope, range) =>
				{
					double result = -@this.ValueAs<double>();
					return new Instance("Number", result);
				});
				type.DeclareOperation("-", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					double result = @this.ValueAs<double>() - other.ValueAs<double>();
					return new Instance("Number", result);
				});
				type.DeclareOperation("*", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					double result = @this.ValueAs<double>() * other.ValueAs<double>();
					return new Instance("Number", result);
				});
				type.DeclareOperation("/", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					double result = @this.ValueAs<double>() / other.ValueAs<double>();
					return new Instance("Number", result);
				});
				type.DeclareOperation("<", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.ValueAs<double>() < other.ValueAs<double>();
					return new Instance("Boolean", result);
				});
				type.DeclareOperation("<=", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.ValueAs<double>() <= other.ValueAs<double>();
					return new Instance("Boolean", result);
				});
				type.DeclareOperation(">", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.ValueAs<double>() > other.ValueAs<double>();
					return new Instance("Boolean", result);
				});
				type.DeclareOperation(">=", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.ValueAs<double>() >= other.ValueAs<double>();
					return new Instance("Boolean", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					string result = @this.ValueAs<double>().ToString(CultureInfo.InvariantCulture);
					return new Instance("String", result);
				});
			});
			module.DeclareClass("Boolean", "Any", static (type) =>
			{
				type.DeclareOperation("!", [], "Boolean", static (@this, arguments, scope, range) =>
				{
					bool result = !@this.ValueAs<bool>();
					return new Instance("Boolean", result);
				});
				type.DeclareOperation("&", ["Boolean"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.ValueAs<bool>() && other.ValueAs<bool>();
					return new Instance("Boolean", result);
				});
				type.DeclareOperation("|", ["Boolean"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.ValueAs<bool>() || other.ValueAs<bool>();
					return new Instance("Boolean", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					bool result = @this.ValueAs<bool>();
					return new Instance("String", result ? "true" : "false");
				});
			});
			module.DeclareClass("String", "Any", static (type) =>
			{
				type.DeclareOperation("+", ["String"], "String", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					string result = @this.ValueAs<string>() + other.ValueAs<string>();
					return new Instance("String", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return @this;
				});
			});
			module.DeclareClass("Null", "Any", static (type) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return new Instance("String", "null");
				});
			});
			module.DeclareClass("Function", "Any", static (type) =>
			{
			});
			module.DeclareClass("Type", "Any", static (type) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					Class value = @this.ValueAs<Class>();
					return new Instance("String", value.Name);
				});
			});
			module.DeclareClass(RuntimeBuilder.NameWorkspace, "Any", static (type) =>
			{
				type.DeclareConstant("pi", "Number", PI);
				type.DeclareConstant("e", "Number", E);
				type.DeclareOperation("read", ["String"], "String", static (@this, arguments, scope, range) =>
				{
					Instance message = arguments[0];
					Console.Write(message.ValueAs<string>());
					string? input = Console.ReadLine();
					ArgumentNullException.ThrowIfNull(input);
					return new Instance("String", input);
				});
				type.DeclareOperation("write", ["Any"], "Null", static (@this, arguments, scope, range) =>
				{
					Instance value = arguments[0];
					Instance text = value.RunOperation("to_string", [], scope, range);
					Console.WriteLine(text.ValueAs<string>());
					return Instance.Null;
				});
			});
		});
	}

	public void Evaluate(IEnumerable<Node> nodes)
	{
		foreach (Node node in nodes) node.Accept(Evaluator, RuntimeBuilder.Workspace);
	}
}
