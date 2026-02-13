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
					Value other = arguments[0];
					Value isEqual = @this.RunOperation("is_equal", [other], scope, range);
					return isEqual;
				});
				type.DeclareOperation("!=", ["Any"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Value other = arguments[0];
					Value isEqual = @this.RunOperation("=", [other], scope, range);
					Value isNotEqual = isEqual.RunOperation("!", [], scope, range);
					return isNotEqual;
				});
				type.DeclareOperation("is_equal", ["Any"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Value other = arguments[0];
					object left = @this.Content;
					object right = other.Content;
					bool result = left.Equals(right);
					return new Value<bool>("Boolean", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return new Value<string>("String", $"[{@this.Tag}]");
				});
			});
			module.DeclareClass("Null", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return new Value<string>("String", "null");
				});
			});
			module.DeclareClass("Nullable", "Any", ["Content"], static (type, generics) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					return @this.RunOperation("to_string", [], scope, range);
				});
				type.DeclareOperation("get_value", [], generics[0].Name, static (@this, arguments, scope, range) =>
				{
					return TypeHelper.Unwrap(@this);
				});
			});
			module.DeclareClass("Number", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("+", [], "Number", static (@this, arguments, scope, range) =>
				{
					double result = +@this.As<double>().Content;
					return new Value<double>("Number", result);
				});
				type.DeclareOperation("+", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content + other.Content;
					return new Value<double>("Number", result);
				});
				type.DeclareOperation("-", [], "Number", static (@this, arguments, scope, range) =>
				{
					double result = -@this.As<double>().Content;
					return new Value<double>("Number", result);
				});
				type.DeclareOperation("-", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content - other.Content;
					return new Value<double>("Number", result);
				});
				type.DeclareOperation("*", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content * other.Content;
					return new Value<double>("Number", result);
				});
				type.DeclareOperation("/", ["Number"], "Number", static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content / other.Content;
					return new Value<double>("Number", result);
				});
				type.DeclareOperation("<", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					bool result = @this.As<double>().Content < other.Content;
					return new Value<bool>("Boolean", result);
				});
				type.DeclareOperation("<=", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					Value<bool> isLess = @this.RunOperation("<", [other], scope, range).As<bool>();
					if (isLess.Content) return new Value<bool>("Boolean", true);
					Value<bool> isEqual = @this.RunOperation("=", [other], scope, range).As<bool>();
					return new Value<bool>("Boolean", isEqual.Content);
				});
				type.DeclareOperation(">", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					bool result = @this.As<double>().Content > other.Content;
					return new Value<bool>("Boolean", result);
				});
				type.DeclareOperation(">=", ["Number"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					Value<bool> isGreater = @this.RunOperation(">", [other], scope, range).As<bool>();
					if (isGreater.Content) return new Value<bool>("Boolean", true);
					Value<bool> isEqual = @this.RunOperation("=", [other], scope, range).As<bool>();
					return new Value<bool>("Boolean", isEqual.Content);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					string result = @this.As<double>().Content.ToString(CultureInfo.InvariantCulture);
					return new Value<string>("String", result);
				});
			});
			module.DeclareClass("Boolean", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("!", [], "Boolean", static (@this, arguments, scope, range) =>
				{
					bool result = !@this.As<bool>().Content;
					return new Value<bool>("Boolean", result);
				});
				type.DeclareOperation("&", ["Boolean"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Value<bool> other = arguments[0].As<bool>();
					bool result = @this.As<bool>().Content && other.Content;
					return new Value<bool>("Boolean", result);
				});
				type.DeclareOperation("|", ["Boolean"], "Boolean", static (@this, arguments, scope, range) =>
				{
					Value<bool> other = arguments[0].As<bool>();
					bool result = @this.As<bool>().Content || other.Content;
					return new Value<bool>("Boolean", result);
				});
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					bool result = @this.As<bool>().Content;
					return new Value<string>("String", result ? "true" : "false");
				});
			});
			module.DeclareClass("String", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("+", ["String"], "String", static (@this, arguments, scope, range) =>
				{
					Value<string> other = arguments[0].As<string>();
					string result = @this.As<string>().Content + other.Content;
					return new Value<string>("String", result);
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
					Class value = @this.As<Class>().Content;
					return new Value<string>("String", value.Name);
				});
			});
			module.DeclareClass("Template", "Any", [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], "String", static (@this, arguments, scope, range) =>
				{
					Template value = @this.As<Template>().Content;
					return new Value<string>("String", value.Name);
				});
			});
			module.DeclareClass(RuntimeBuilder.NameWorkspace, "Any", [], static (type, _) =>
			{
				type.DeclareConstant("pi", "Number", PI);
				type.DeclareConstant("e", "Number", E);
				type.DeclareOperation("read", ["String"], "String", static (@this, arguments, scope, range) =>
				{
					Value<string> message = arguments[0].As<string>();
					Console.Write(message.Content);
					string? input = Console.ReadLine();
					ArgumentNullException.ThrowIfNull(input);
					return new Value<string>("String", input);
				});
				type.DeclareOperation("write", ["Any"], "Null", static (@this, arguments, scope, range) =>
				{
					Value value = arguments[0];
					Value text = value.RunOperation("to_string", [], scope, range).As<string>();
					Console.WriteLine(text.Content);
					return Value.Null;
				});
			});
		});
	}

	public void Evaluate(IEnumerable<Node> nodes)
	{
		foreach (Node node in nodes) node.Accept(Evaluator, RuntimeBuilder.Workspace);
	}
}
