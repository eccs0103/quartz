using System.Globalization;
using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions.Semantic;
using Quartz.Domain.Parsing;
using static System.Math;
using static Quartz.Domain.Definitions;

namespace Quartz.Application.Evaluating;

public class Runtime
{
	private RuntimeBuilder Builder { get; } = new();
	private Evaluator Evaluator { get; } = new();

	public Runtime()
	{
		Builder.DeclareModule(static (module) =>
		{
			module.DeclareClass(Types.Any, null, [], static (type, _) =>
			{
				type.DeclareOperation(Operators.Equal, [Types.Any], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value other = arguments[0];
					Value isEqual = @this.RunOperation("is_equal", [other], scope, range);
					return isEqual;
				});
				type.DeclareOperation(Operators.NotEqual, [Types.Any], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value other = arguments[0];
					Value isEqual = @this.RunOperation(Operators.Equal, [other], scope, range);
					Value isNotEqual = isEqual.RunOperation(Operators.Not, [], scope, range);
					return isNotEqual;
				});
				type.DeclareOperation("is_equal", [Types.Any], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value other = arguments[0];
					object left = @this.Content;
					object right = other.Content;
					bool result = left.Equals(right);
					return result ? Value.True : Value.False;
				});
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					return new Value<string>(Types.String, $"[instance {@this.Tag}]");
				});
			});
			module.DeclareClass(Types.Null, Types.Any, [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					return new Value<string>(Types.String, Keywords.Null);
				});
			});
			module.DeclareClass(Types.Nullable, Types.Any, ["Content"], static (type, generics) =>
			{
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					return @this.RunOperation("to_string", [], scope, range);
				});
				type.DeclareOperation("get_value", [], generics[0].Name, static (@this, arguments, scope, range) =>
				{
					return TypeHelper.Unwrap(@this);
				});
			});
			module.DeclareClass(Types.Number, Types.Any, [], static (type, _) =>
			{
				type.DeclareOperation(Operators.Plus, [], Types.Number, static (@this, arguments, scope, range) =>
				{
					double result = +@this.As<double>().Content;
					return new Value<double>(Types.Number, result);
				});
				type.DeclareOperation(Operators.Plus, [Types.Number], Types.Number, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content + other.Content;
					return new Value<double>(Types.Number, result);
				});
				type.DeclareOperation(Operators.Minus, [], Types.Number, static (@this, arguments, scope, range) =>
				{
					double result = -@this.As<double>().Content;
					return new Value<double>(Types.Number, result);
				});
				type.DeclareOperation(Operators.Minus, [Types.Number], Types.Number, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content - other.Content;
					return new Value<double>(Types.Number, result);
				});
				type.DeclareOperation(Operators.Multiply, [Types.Number], Types.Number, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content * other.Content;
					return new Value<double>(Types.Number, result);
				});
				type.DeclareOperation(Operators.Divide, [Types.Number], Types.Number, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					double result = @this.As<double>().Content / other.Content;
					return new Value<double>(Types.Number, result);
				});
				type.DeclareOperation(Operators.Less, [Types.Number], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					bool result = @this.As<double>().Content < other.Content;
					return result ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.LessOrEqual, [Types.Number], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					Value<bool> isLess = @this.RunOperation(Operators.Less, [other], scope, range).As<bool>();
					if (isLess.Content) return Value.True;
					Value<bool> isEqual = @this.RunOperation(Operators.Equal, [other], scope, range).As<bool>();
					return isEqual.Content ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.Greater, [Types.Number], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					bool result = @this.As<double>().Content > other.Content;
					return result ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.GreaterOrEqual, [Types.Number], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<double> other = arguments[0].As<double>();
					Value<bool> isGreater = @this.RunOperation(Operators.Greater, [other], scope, range).As<bool>();
					if (isGreater.Content) return Value.True;
					Value<bool> isEqual = @this.RunOperation(Operators.Equal, [other], scope, range).As<bool>();
					return isEqual.Content ? Value.True : Value.False;
				});
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					string result = @this.As<double>().Content.ToString(CultureInfo.InvariantCulture);
					return new Value<string>(Types.String, result);
				});
			});
			module.DeclareClass(Types.Boolean, Types.Any, [], static (type, _) =>
			{
				type.DeclareOperation(Operators.Not, [], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					bool result = !@this.As<bool>().Content;
					return result ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.And, [Types.Boolean], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<bool> other = arguments[0].As<bool>();
					bool result = @this.As<bool>().Content && other.Content;
					return result ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.Or, [Types.Boolean], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					Value<bool> other = arguments[0].As<bool>();
					bool result = @this.As<bool>().Content || other.Content;
					return result ? Value.True : Value.False;
				});
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					bool result = @this.As<bool>().Content;
					return new Value<string>(Types.String, result ? Keywords.True : Keywords.False);
				});
			});
			module.DeclareClass(Types.Character, Types.Any, [], static (type, _) =>
			{
				type.DeclareOperation(Operators.Less, [Types.Character], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					char other = arguments[0].As<char>().Content;
					return @this.As<char>().Content < other ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.LessOrEqual, [Types.Character], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					char other = arguments[0].As<char>().Content;
					return @this.As<char>().Content <= other ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.Greater, [Types.Character], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					char other = arguments[0].As<char>().Content;
					return @this.As<char>().Content > other ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.GreaterOrEqual, [Types.Character], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					char other = arguments[0].As<char>().Content;
					return @this.As<char>().Content >= other ? Value.True : Value.False;
				});
				type.DeclareOperation(Operators.Plus, [Types.String], Types.String, static (@this, arguments, scope, range) =>
				{
					Value<string> other = arguments[0].As<string>();
					string result = @this.As<char>().Content.ToString() + other.Content;
					return new Value<string>(Types.String, result);
				});
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					return new Value<string>(Types.String, @this.As<char>().Content.ToString());
				});
				type.DeclareOperation("to_number", [], Types.Number, static (@this, arguments, scope, range) =>
				{
					return new Value<double>(Types.Number, (double) @this.As<char>().Content);
				});
			});
			module.DeclareClass(Types.String, $"{Types.Array}<{Types.Character}>", [], static (type, _) =>
			{
				type.DeclareOperation(Operators.Spread, [], $"{Types.Sequence}<{Types.Character}>", (@this, arguments, scope, range) =>
				{
					string text = @this.As<string>().Content;
					IEnumerator<Value> enumerator = text.Select(character => new Value<char>(Types.Character, character)).GetEnumerator();
					return new Value<IEnumerator<Value>>($"{Types.Sequence}<{Types.Character}>", enumerator);
				});
				type.DeclareOperation(Operators.Indexer, [Types.Number], Types.Character, static (@this, arguments, scope, range) =>
				{
					string text = @this.As<string>().Content;
					int index = (int) arguments[0].As<double>().Content;
					if (index < 0 || index >= text.Length) throw new OutOfRangeIssue(index, text.Length, range);
					return new Value<char>(Types.Character, text[index]);
				});
				type.DeclareOperation(Operators.Indexer, [Types.Number, Types.Character], Types.Null, static (@this, arguments, scope, range) =>
				{
					throw new ImmutableAssignmentIssue(Types.String, range);
				});
				type.DeclareOperation(Operators.Plus, [Types.Character], Types.String, static (@this, arguments, scope, range) =>
				{
					Value<char> other = arguments[0].As<char>();
					string result = @this.As<string>().Content + other.Content;
					return new Value<string>(Types.String, result);
				});
				type.DeclareOperation(Operators.Plus, [Types.String], Types.String, static (@this, arguments, scope, range) =>
				{
					Value<string> other = arguments[0].As<string>();
					string result = @this.As<string>().Content + other.Content;
					return new Value<string>(Types.String, result);
				});
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					return @this;
				});
				type.DeclareOperation("length", [], Types.Number, static (@this, arguments, scope, range) =>
				{
					string text = @this.As<string>().Content;
					return new Value<double>(Types.Number, text.Length);
				});
			});
			module.DeclareClass(Types.Function, Types.Any, [], static (type, _) =>
			{
			});
			module.DeclareClass(Types.Type, Types.Any, [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					Class value = @this.As<Class>().Content;
					return new Value<string>(Types.String, value.Name);
				});
			});
			module.DeclareClass(Types.Template, Types.Any, [], static (type, _) =>
			{
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					Template value = @this.As<Template>().Content;
					return new Value<string>(Types.String, value.Name);
				});
			});
			module.DeclareClass(Types.Sequence, Types.Any, ["Content"], static (type, generics) =>
			{
				type.DeclareOperation("next", [], Types.Boolean, static (@this, arguments, scope, range) =>
				{
					IEnumerator<Value> enumerator = @this.As<IEnumerator<Value>>().Content;
					return enumerator.MoveNext() ? Value.True : Value.False;
				});
				type.DeclareOperation("current", [], generics[0].Name, static (@this, arguments, scope, range) =>
				{
					IEnumerator<Value> enumerator = @this.As<IEnumerator<Value>>().Content;
					return enumerator.Current;
				});
			});
			module.DeclareClass(Types.Array, Types.Any, ["Content"], static (type, generics) =>
			{
				type.DeclareOperation(Operators.Spread, [], $"{Types.Sequence}<{generics[0].Name}>", (@this, arguments, scope, range) =>
				{
					Value[] elements = @this.As<Value[]>().Content;
					IEnumerator<Value> enumerator = elements.AsEnumerable().GetEnumerator();
					return new Value<IEnumerator<Value>>($"{Types.Sequence}<{generics[0].Name}>", enumerator);
				});
				type.DeclareOperation(Operators.Indexer, [Types.Number], generics[0].Name, static (@this, arguments, scope, range) =>
				{
					Value[] elements = @this.As<Value[]>().Content;
					int index = (int) arguments[0].As<double>().Content;
					if (index < 0 || index >= elements.Length) throw new OutOfRangeIssue(index, elements.Length, range);
					return elements[index];
				});
				type.DeclareOperation(Operators.Indexer, [Types.Number, generics[0].Name], Types.Null, static (@this, arguments, scope, range) =>
				{
					Value[] elements = @this.As<Value[]>().Content;
					int index = (int) arguments[0].As<double>().Content;
					Value value = arguments[1];
					if (index < 0 || index >= elements.Length) throw new OutOfRangeIssue(index, elements.Length, range);
					elements[index] = value;
					return Value.Null;
				});
				type.DeclareOperation("to_string", [], Types.String, static (@this, arguments, scope, range) =>
				{
					Value[] elements = @this.As<Value[]>().Content;
					IEnumerable<string> strings = elements.Select(value => value.RunOperation("to_string", [], scope, range).As<string>().Content);
					return new Value<string>(Types.String, Mangler.Collection(strings));
				});
				type.DeclareOperation("length", [], Types.Number, static (@this, arguments, scope, range) =>
				{
					Value[] elements = @this.As<Value[]>().Content;
					return new Value<double>(Types.Number, elements.Length);
				});
			});
			module.DeclareClass(Types.Workspace, Types.Any, [], static (type, _) =>
			{
				type.DeclareConstant("pi", Types.Number, PI);
				type.DeclareConstant("e", Types.Number, E);
				type.DeclareOperation("read", [Types.String], Types.String, static (@this, arguments, scope, range) =>
				{
					Value<string> message = arguments[0].As<string>();
					Console.Write(message.Content);
					string? input = Console.ReadLine();
					ArgumentNullException.ThrowIfNull(input);
					return new Value<string>(Types.String, input);
				});
				type.DeclareOperation("write", [Types.Any], Types.Null, static (@this, arguments, scope, range) =>
				{
					Value value = arguments[0];
					Value text = value.RunOperation("to_string", [], scope, range).As<string>();
					Console.WriteLine(text.Content);
					return Value.Null;
				});
				type.DeclareOperation("range", [Types.Number], $"{Types.Sequence}<{Types.Number}>", static (@this, arguments, scope, range) =>
				{
					Value<double> start = new(Types.Number, 0);
					Value end = arguments[0];
					return @this.RunOperation("range", [start, end], scope, range);
				});
				type.DeclareOperation("range", [Types.Number, Types.Number], $"{Types.Sequence}<{Types.Number}>", static (@this, arguments, scope, range) =>
				{
					double min = arguments[0].As<double>().Content;
					double max = arguments[1].As<double>().Content;
					static IEnumerator<Value> Generate(double start, double end)
					{
						for (double index = start; index < end; index++) yield return new Value<double>(Types.Number, index);
					}
					IEnumerator<Value> enumerator = Generate(min, max);
					return new Value<IEnumerator<Value>>($"{Types.Sequence}<{Types.Number}>", enumerator);
				});
			});
		});
	}

	public void Evaluate(IEnumerable<Node> nodes)
	{
		foreach (Node node in nodes) node.Accept(Evaluator, RuntimeBuilder.Workspace);

		if (RuntimeBuilder.Workspace.TryRead("main", out Operator? mainOperator) && mainOperator.TryReadOperation([], out Operation? main))
		{
			main.Invoke([], RuntimeBuilder.Workspace, ~Shared.Helpers.Position.Zero);
		}
	}
}
