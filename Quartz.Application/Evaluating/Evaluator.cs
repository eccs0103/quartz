using System.Runtime.CompilerServices;
using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Exceptions.Semantic;
using Quartz.Domain.Parsing;
using static Quartz.Domain.Definitions;

namespace Quartz.Application.Evaluating;

internal class Evaluator : IEvaluator<Value>
{
	public Value Evaluate(Scope location, ValueNode node)
	{
		object value = node.Value ?? Value.Empty;
		return new Value<object>(node.Tag, value);
	}

	public Value Evaluate(Scope location, IdentifierNode node)
	{
		if (location.TryRead(node.Name, out Variable? variable)) return variable.Value;
		throw new SymbolNotFoundIssue(node.Name, "Variable", node.RangePosition);
	}

	public Value Evaluate(Scope location, TemplateNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		if (!location.TryRead(nodeTarget.Name, out Template? template)) throw new SymbolNotFoundIssue(nodeTarget.Name, Types.Template, nodeTarget.RangePosition);
		IEnumerable<Class> generics = node.Generics.Select((nodeGeneric) =>
		{
			Value value = nodeGeneric.Accept(this, location);
			if (value.Tag != Types.Type || value.Content is not Class type) throw new TypeMismatchIssue(Types.Type, value.Tag, nodeGeneric.RangePosition);
			return type;
		});
		if (location.TryRead(node.Name, out Variable? variable))
		{
			if (variable.Value is Value<Class> type) return type;
			throw new InvalidSymbolUsageIssue(node.Name, "Class", node.RangePosition);
		}
		Class type2 = template.Assemble(node.Name, generics, node.RangePosition);
		if (!location.TryRegister(node.Name, Types.Type, new Value<Class>(Types.Type, type2))) throw new SymbolAlreadyDeclaredIssue(node.Name, node.RangePosition);
		return new Value<Class>(Types.Type, type2);
	}

	public Value Evaluate(Scope location, DeclarationNode node)
	{
		IdentifierNode nodeType = node.Type;
		Value typeValue = nodeType.Accept(this, location);
		if (typeValue.Content is not Class typeClass) throw new TypeMismatchIssue(Types.Type, typeValue.Tag, nodeType.RangePosition);
		string typeName = typeClass.Name;
		IdentifierNode nodeIdentifier = node.Identifier;
		if (node.Value == null && !TypeHelper.IsOptional(typeName)) throw new VariableNotInitializedIssue(nodeIdentifier.Name, nodeIdentifier.RangePosition);
		Value value = node.Value?.Accept(this, location) ?? Value.Null;
		if (!TypeHelper.IsCompatible(typeName, value.Tag, location)) throw new TypeMismatchIssue(typeName, value.Tag, node.Value?.RangePosition ?? nodeIdentifier.RangePosition);
		if (!location.TryRegister(nodeIdentifier.Name, typeName, value, true)) throw new SymbolAlreadyDeclaredIssue(nodeIdentifier.Name, nodeIdentifier.RangePosition);
		return Value.Null;
	}

	public Value Evaluate(Scope location, AssignmentNode node)
	{
		Value value = node.Value.Accept(this, location);
		Node nodeTarget = node.Target;
		if (nodeTarget is IdentifierNode nodeIdentifier)
		{
			if (!location.TryRead(nodeIdentifier.Name, out Variable? variable)) throw new SymbolNotFoundIssue(nodeIdentifier.Name, "Variable", nodeIdentifier.RangePosition);
			variable.Assign(value, location, nodeIdentifier.RangePosition);
			return Value.Null;
		}
		if (nodeTarget is IndexNode nodeIndex)
		{
			Value target = nodeIndex.Target.Accept(this, location);
			Value index = nodeIndex.Index.Accept(this, location);
			return target.RunOperation("[]", [index, value], location, node.RangePosition);
		}
		throw new InvalidSymbolUsageIssue(nodeTarget.ToString()!, "Assignment Target", node.Target.RangePosition);
	}

	public Value Evaluate(Scope location, ArrayNode node)
	{
		List<Value> elements = node.Elements.Select(element => element.Accept(this, location)).ToList();
		string tag = elements.Count > 0 ? elements[0].Tag : Types.Any;
		if (elements.Any(element => element.Tag != tag)) tag = Types.Any;
		return new Value<List<Value>>(Mangler.Generics(Types.Array, [tag]), elements);
	}

	public Value Evaluate(Scope location, IndexNode node)
	{
		Value target = node.Target.Accept(this, location);
		Value index = node.Index.Accept(this, location);
		return target.RunOperation("[]", [index], location, node.RangePosition);
	}

	public Value Evaluate(Scope location, InvocationNode node)
	{
		IEnumerable<Value> arguments = node.Arguments.Select(argument => TypeHelper.Unwrap(argument.Accept(this, location)));
		if (node.Target is FieldNode nodeField)
		{
			Value target = TypeHelper.Unwrap(nodeField.Target.Accept(this, location));
			return target.RunOperation(nodeField.Member.Name, arguments, location, node.RangePosition);
		}
		if (node.Target is IdentifierNode nodeIdentifier)
		{
			if (location.TryRead(nodeIdentifier.Name, out Operator? @operator))
			{
				if (!@operator.TryReadOperation(arguments.Select(result => result.Tag), out Operation? operation)) throw new NoMatchingOverloadIssue(nodeIdentifier.Name, arguments.Select(a => a.Tag), nodeIdentifier.RangePosition);
				Scope scope = location.GetSubscope("Call");
				return operation.Invoke(arguments, scope, node.RangePosition);
			}
			if (location.TryRead(nodeIdentifier.Name, out Class? type))
			{
				if (arguments.Count() == 1) return arguments.First();
			}
			throw new SymbolNotFoundIssue(nodeIdentifier.Name, "Operator", nodeIdentifier.RangePosition);
		}
		throw new NotCallableIssue(node.Target.ToString()!, node.Target.RangePosition);
	}

	public Value Evaluate(Scope location, FieldNode node)
	{
		Value target = TypeHelper.Unwrap(node.Target.Accept(this, location));
		if (!location.TryRead(target.Tag, out Class? type)) throw new SymbolNotFoundIssue(target.Tag, "Type class", node.Target.RangePosition);
		if (!type.TryReadProperty(node.Member.Name, out Variable? variable)) throw new SymbolNotFoundIssue(node.Member.Name, "Member variable", node.Member.RangePosition);
		return variable.Value;
	}

	public Value Evaluate(Scope location, UnaryOperatorNode node)
	{
		Node nodeTarget = node.Target;
		IdentifierNode nodeOperator = node.Operator;
		Value target = TypeHelper.Unwrap(nodeTarget.Accept(this, location));
		if (!location.TryRead(target.Tag, out Class? type)) throw new SymbolNotFoundIssue(target.Tag, "Type class", nodeTarget.RangePosition);
		if (!type.TryReadOperation(nodeOperator.Name, [target.Tag], out Operation? operation)) throw new NoMatchingOverloadIssue(nodeOperator.Name, [target.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([target], scope, node.RangePosition);
	}

	public Value Evaluate(Scope location, BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Value left = TypeHelper.Unwrap(node.Left.Accept(this, location));
		Value right = TypeHelper.Unwrap(node.Right.Accept(this, location));
		if (!location.TryRead(left.Tag, out Class? type)) throw new SymbolNotFoundIssue(left.Tag, "Type class", node.Left.RangePosition);
		if (!type.TryReadOperation(nodeOperator.Name, [left.Tag, right.Tag], out Operation? operation)) throw new NoMatchingOverloadIssue(nodeOperator.Name, [left.Tag, right.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([left, right], scope, node.RangePosition);
	}

	public Value Evaluate(Scope location, BlockNode node)
	{
		Scope scope = location.GetSubscope("Block");
		foreach (Node statement in node.Statements) statement.Accept(this, scope);
		return Value.Null;
	}

	public Value Evaluate(Scope location, IfStatementNode node)
	{
		Value condition = node.Condition.Accept(this, location);
		if (condition.Tag != Types.Boolean) throw new TypeMismatchIssue(Types.Boolean, condition.Tag, node.Condition.RangePosition);
		Node? nodeBranch = condition.As<bool>().Content ? node.Then : node.Else;
		nodeBranch?.Accept(this, location);
		return Value.Null;
	}

	public Value Evaluate(Scope location, WhileStatementNode node)
	{
		while (true)
		{
			Value condition = node.Condition.Accept(this, location);
			if (condition.Tag != Types.Boolean) throw new TypeMismatchIssue(Types.Boolean, condition.Tag, node.Condition.RangePosition);
			if (!condition.As<bool>().Content) break;
			Scope scope = location.GetSubscope("While");
			try { node.Body.Accept(this, scope); }
			catch (ContinueSignal) { continue; }
			catch (BreakSignal) { break; }
		}
		return Value.Null;
	}

	public Value Evaluate(Scope location, ForStatementNode node)
	{
		Value generator = node.Collection.Accept(this, location);
		if (location.TryRead(generator.Tag, out Class? type) && type.TryReadOperation("...", [generator.Tag], out Operation? operation))
			generator = operation.Invoke([generator], location, node.RangePosition);

		while (true)
		{
			Value hasNext = generator.RunOperation("next", [], location, node.RangePosition);
			if (hasNext.Tag != Types.Boolean) throw new TypeMismatchIssue(Types.Boolean, hasNext.Tag, node.RangePosition);
			if (!hasNext.As<bool>().Content) break;
			Value current = generator.RunOperation("current", [], location, node.RangePosition);
			Scope scope = location.GetSubscope("ForIn");
			scope.TryRegister(node.Identifier.Name, node.Type.Name, current, false);
			try { node.Body.Accept(this, scope); }
			catch (ContinueSignal) { continue; }
			catch (BreakSignal) { break; }
		}
		return Value.Null;
	}

	public Value Evaluate(Scope location, BreakStatementNode node)
	{
		throw new BreakSignal();
	}

	public Value Evaluate(Scope location, ContinueStatementNode node)
	{
		throw new ContinueSignal();
	}
}
