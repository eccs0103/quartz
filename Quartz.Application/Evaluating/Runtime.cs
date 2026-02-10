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
			module.DeclareClass("Any", null, [], static (type, _) =>
			{
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
				type.DeclareOperation("is_equal", ["Any"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					object left = @this.Value;
					object right = other.Value;
					bool result = left.Equals(right);
					return new Instance<bool>("Boolean", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return new Instance<string>("String", $"[{@this.Tag}]");
				});
			});
			module.DeclareClass("Null", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return new Instance<string>("String", "null");
				});
			});
			module.DeclareClass("Nullable", "Any", ["Content"], static (type, args) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return @this.RunOperation("to_string", [], scope, range);
				});
				type.DeclareOperation("get_value", [], args.First().Name, static (@this, arguments, scope, range) =>
				{
					return TypeHelper.Unwrap(@this);
				});
			});
			module.DeclareClass("Number", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("+", [], "Number", static (@this, arguments, scope, range) =>
				{
					double result = +@this.As<double>().Value;
					return new Instance<double>("Number", result);
				});
				type.DeclareOperation("+", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					double result = @this.As<double>().Value + other.As<double>().Value;
					return new Instance<double>("Number", result);
				});
				type.DeclareOperation("-", [], "Number", static (@this, arguments, scope, range) =>
				{
					double result = -@this.As<double>().Value;
					return new Instance<double>("Number", result);
				});
				type.DeclareOperation("-", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					double result = @this.As<double>().Value - other.As<double>().Value;
					return new Instance<double>("Number", result);
				});
				type.DeclareOperation("*", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					double result = @this.As<double>().Value * other.As<double>().Value;
					return new Instance<double>("Number", result);
				});
				type.DeclareOperation("/", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					double result = @this.As<double>().Value / other.As<double>().Value;
					return new Instance<double>("Number", result);
				});
				type.DeclareOperation("<", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.As<double>().Value < other.As<double>().Value;
					return new Instance<bool>("Boolean", result);
				});
				type.DeclareOperation("<=", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.As<double>().Value <= other.As<double>().Value;
					return new Instance<bool>("Boolean", result);
				});
				type.DeclareOperation(">", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.As<double>().Value > other.As<double>().Value;
					return new Instance<bool>("Boolean", result);
				});
				type.DeclareOperation(">=", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.As<double>().Value >= other.As<double>().Value;
					return new Instance<bool>("Boolean", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					string result = @this.As<double>().Value.ToString(CultureInfo.InvariantCulture);
					return new Instance<string>("String", result);
				});
			});
			module.DeclareClass("Boolean", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("!", [], "Boolean", static (@this, arguments, scope, range) =>
				{
					bool result = !@this.As<bool>().Value;
					return new Instance<bool>("Boolean", result);
				});
				type.DeclareOperation("&", ["Boolean"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.As<bool>().Value && other.As<bool>().Value;
					return new Instance<bool>("Boolean", result);
				});
				type.DeclareOperation("|", ["Boolean"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					bool result = @this.As<bool>().Value || other.As<bool>().Value;
					return new Instance<bool>("Boolean", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					bool result = @this.As<bool>().Value;
					return new Instance<string>("String", result ? "true" : "false");
				});
			});
			module.DeclareClass("String", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("+", ["String"], "String", static (@this, arguments, scope, range) =>
				{
					Instance other = arguments[0];
					string result = @this.As<string>().Value + other.As<string>().Value;
					return new Instance<string>("String", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return @this;
				});
			});
			module.DeclareClass("Function", "Any", [], static (type, _) =>
			{
			});
			module.DeclareClass("Type", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					Class value = @this.As<Class>().Value;
					return new Instance<string>("String", value.Name);
				});
			});
			module.DeclareClass("Generic", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					Generic value = @this.As<Generic>().Value;
					return new Instance<string>("String", value.Name);
				});
			});
			module.DeclareClass(RuntimeBuilder.NameWorkspace, "Any", [], static (type, _) =>
			{
				type.DeclareConstant("pi", "Number", PI);
				type.DeclareConstant("e", "Number", E);
				type.DeclareOperation("read", ["String"], "String", static (@this, arguments, scope, range) =>
				{
					Instance message = arguments[0];
					Console.Write(message.As<string>().Value);
					string? input = Console.ReadLine();
					ArgumentNullException.ThrowIfNull(input);
					return new Instance<string>("String", input);
				});
				type.DeclareOperation("write", ["Any"], "Null", static (@this, arguments, scope, range) =>
				{
					Instance value = arguments[0];
					Instance text = value.RunOperation("to_string", [], scope, range);
					Console.WriteLine(text.As<string>().Value);
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
