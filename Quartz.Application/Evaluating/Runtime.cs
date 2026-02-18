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
			module.DeclareClass(TypeConstants.Any, null, [], static (type, _) =>
			{
				type.DeclareOperation("=", [TypeConstants.Any], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value other = arguments[0];
					Value isEqual = @this.RunOperation("is_equal", [other], scope, range);
					return isEqual;
				});
				type.DeclareOperation("!=", [TypeConstants.Any], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value other = arguments[0];
					Value isEqual = @this.RunOperation("=", [other], scope, range);
					Value isNotEqual = isEqual.RunOperation("!", [], scope, range);
					return isNotEqual;
				});
				type.DeclareOperation("is_equal", [TypeConstants.Any], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value other = arguments[0];
					object left = @this.Content;
					object right = other.Content;
					bool result = left.Equals(right);
					return new Value<bool>(TypeConstants.Boolean, result);
				});
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					return new Value<string>(TypeConstants.String, $"[{@this.Tag}]");
				});
			});
			module.DeclareClass(TypeConstants.Null, TypeConstants.Any, [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					return new Value<string>(TypeConstants.String, "null");
				});
			});
			module.DeclareClass(TypeConstants.Nullable, TypeConstants.Any, ["Content"], static (type, generics) =>
			{
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					return @this.RunOperation("to_string", [], scope, range);
				});
				type.DeclareOperation("get_value", [], generics[0].Name, static (@this, arguments, scope, range) =>
				{
					return TypeHelper.Unwrap(@this);
				});
			});
			module.DeclareClass(TypeConstants.Number, TypeConstants.Any, [], static (type, _) =>
			{
				type.DeclareOperation("+", [], TypeConstants.Number, static (@this, arguments, scope, range) =>
				{
					double result = +@this.As<double>().Content;
					return new Value<double>(TypeConstants.Number, result);
				});
				type.DeclareOperation("+", [TypeConstants.Number], TypeConstants.Number, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content + other.Content;
					return new Value<double>(TypeConstants.Number, result);
				});
				type.DeclareOperation("-", [], TypeConstants.Number, static (@this, arguments, scope, range) =>
				{
					double result = -@this.As<double>().Content;
					return new Value<double>(TypeConstants.Number, result);
				});
				type.DeclareOperation("-", [TypeConstants.Number], TypeConstants.Number, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content - other.Content;
					return new Value<double>(TypeConstants.Number, result);
				});
				type.DeclareOperation("*", [TypeConstants.Number], TypeConstants.Number, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content * other.Content;
					return new Value<double>(TypeConstants.Number, result);
				});
				type.DeclareOperation("/", [TypeConstants.Number], TypeConstants.Number, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content / other.Content;
					return new Value<double>(TypeConstants.Number, result);
				});
				type.DeclareOperation("<", [TypeConstants.Number], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					bool result = @this.As<double>().Content < other.Content;
					return new Value<bool>(TypeConstants.Boolean, result);
				});
				type.DeclareOperation("<=", [TypeConstants.Number], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					Value<bool> isLess = @this.RunOperation("<", [other], scope, range).As<bool>();
					if (isLess.Content) return new Value<bool>(TypeConstants.Boolean, true);
					Value<bool> isEqual = @this.RunOperation("=", [other], scope, range).As<bool>();
					return new Value<bool>(TypeConstants.Boolean, isEqual.Content);
				});
				type.DeclareOperation(">", [TypeConstants.Number], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					bool result = @this.As<double>().Content > other.Content;
					return new Value<bool>(TypeConstants.Boolean, result);
				});
				type.DeclareOperation(">=", [TypeConstants.Number], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					Value<bool> isGreater = @this.RunOperation(">", [other], scope, range).As<bool>();
					if (isGreater.Content) return new Value<bool>(TypeConstants.Boolean, true);
					Value<bool> isEqual = @this.RunOperation("=", [other], scope, range).As<bool>();
					return new Value<bool>(TypeConstants.Boolean, isEqual.Content);
				});
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					string result = @this.As<double>().Content.ToString(CultureInfo.InvariantCulture);
					return new Value<string>(TypeConstants.String, result);
				});
			});
			module.DeclareClass(TypeConstants.Boolean, TypeConstants.Any, [], static (type, _) =>
			{
				type.DeclareOperation("!", [], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					bool result = !@this.As<bool>().Content;
					return new Value<bool>(TypeConstants.Boolean, result);
				});
				type.DeclareOperation("&", [TypeConstants.Boolean], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<bool> other = arguments[0].As<bool>();
					bool result = @this.As<bool>().Content && other.Content;
					return new Value<bool>(TypeConstants.Boolean, result);
				});
				type.DeclareOperation("|", [TypeConstants.Boolean], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<bool> other = arguments[0].As<bool>();
					bool result = @this.As<bool>().Content || other.Content;
					return new Value<bool>(TypeConstants.Boolean, result);
				});
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					bool result = @this.As<bool>().Content;
					return new Value<string>(TypeConstants.String, result ? "true" : "false");
				});
			});
			module.DeclareClass(TypeConstants.Character, TypeConstants.Any, [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					return new Value<string>(TypeConstants.String, @this.As<char>().Content.ToString());
				});
				type.DeclareOperation("to_number", [], TypeConstants.Number, static (@this, arguments, scope, range) =>
				{
					return new Value<double>(TypeConstants.Number, (double) @this.As<char>().Content);
				});
				type.DeclareOperation("<", [TypeConstants.Character], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					char other = arguments[0].As<char>().Content;
					return new Value<bool>(TypeConstants.Boolean, @this.As<char>().Content < other);
				});
				type.DeclareOperation("<=", [TypeConstants.Character], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					char other = arguments[0].As<char>().Content;
					return new Value<bool>(TypeConstants.Boolean, @this.As<char>().Content <= other);
				});
				type.DeclareOperation(">", [TypeConstants.Character], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					char other = arguments[0].As<char>().Content;
					return new Value<bool>(TypeConstants.Boolean, @this.As<char>().Content > other);
				});
				type.DeclareOperation(">=", [TypeConstants.Character], TypeConstants.Boolean, static (@this, arguments, scope, range) =>
				{
					char other = arguments[0].As<char>().Content;
					return new Value<bool>(TypeConstants.Boolean, @this.As<char>().Content >= other);
				});
				type.DeclareOperation("+", [TypeConstants.String], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					Value<string> other = arguments[0].As<string>();
					string result = @this.As<char>().Content.ToString() + other.Content;
					return new Value<string>(TypeConstants.String, result);
				});
			});
			module.DeclareClass(TypeConstants.String, TypeConstants.Any, [], static (type, _) =>
			{
				type.DeclareOperation("+", [TypeConstants.String], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					Value<string> other = arguments[0].As<string>();
					string result = @this.As<string>().Content + other.Content;
					return new Value<string>(TypeConstants.String, result);
				});
				type.DeclareOperation("+", [TypeConstants.Character], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					Value<char> other = arguments[0].As<char>();
					string result = @this.As<string>().Content + other.Content;
					return new Value<string>(TypeConstants.String, result);
				});
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					return @this;
				});
			});
			module.DeclareClass(TypeConstants.Function, TypeConstants.Any, [], static (type, _) =>
			{
			});
			module.DeclareClass(TypeConstants.Type, TypeConstants.Any, [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					Class value = @this.As<Class>().Content;
					return new Value<string>(TypeConstants.String, value.Name);
				});
			});
			module.DeclareClass(TypeConstants.Template, TypeConstants.Any, [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					Template value = @this.As<Template>().Content;
					return new Value<string>(TypeConstants.String, value.Name);
				});
			});
			module.DeclareClass(RuntimeBuilder.NameWorkspace, TypeConstants.Any, [], static (type, _) =>
			{
				type.DeclareConstant("pi", TypeConstants.Number, PI);
				type.DeclareConstant("e", TypeConstants.Number, E);
				type.DeclareOperation("read", [TypeConstants.String], TypeConstants.String, static (@this, arguments, scope, range) =>
				{
					Value<string> message = arguments[0].As<string>();
					Console.Write(message.Content);
					string? input = Console.ReadLine();
					ArgumentNullException.ThrowIfNull(input);
					return new Value<string>(TypeConstants.String, input);
				});
				type.DeclareOperation("write", [TypeConstants.Any], TypeConstants.Null, static (@this, arguments, scope, range) =>
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
