using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;

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
		Symbol symbol = location.Read(node.Name, node.RangePosition);
		if (symbol is Datum datum) return datum.Value;
		if (symbol is Class type) return new Value<Class>("Type", type);
		if (symbol is Template template) return new Value<Template>("Template", template);
		throw new NotExistIssue($"Identifier '{node.Name}' in {location}", node.RangePosition);
	}

	public Value Evaluate(Scope location, GenericNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		Symbol symbol = location.Read(nodeTarget.Name, nodeTarget.RangePosition);
		if (symbol is not Template template) throw new TypeMismatchIssue("Template", symbol.Name, nodeTarget.RangePosition);
		List<Class> types = [];
		foreach (IdentifierNode nodeGeneric in node.Generics)
		{
			Value value = nodeGeneric.Accept(this, location);
			if (value.Tag != "Type" || value.Content is not Class type) throw new TypeMismatchIssue("Class", value.Tag, nodeGeneric.RangePosition);
			types.Add(type);
		}

		if (location.TryRead(node.Name, out Symbol? existing))
		{
			if (existing is Class type) return new Value<Class>("Type", type);
			throw new UnexpectedIssue($"Identifier '{node.Name}' is not a Class", node.RangePosition);
		}

		Class type2 = template.Construct(node.Name, types, node.RangePosition);
		location.Register(node.Name, type2, node.RangePosition);
		return new Value<Class>("Type", type2);
	}

	public Value Evaluate(Scope location, DeclarationNode node)
	{
		IdentifierNode nodeType = node.Type;
		nodeType.Accept(this, location);
		IdentifierNode nodeIdentifier = node.Identifier;

		if (node.Value == null && !TypeHelper.IsOptional(nodeType.Name)) throw new InitializationRequiredIssue(nodeIdentifier.Name, nodeType.Name, nodeIdentifier.RangePosition);

		Value value = node.Value?.Accept(this, location) ?? Value.Null;

		if (node.Value != null && !TypeHelper.IsCompatible(nodeType.Name, value.Tag)) throw new TypeMismatchIssue(nodeType.Name, value.Tag, node.Value.RangePosition);

		Datum variable = new(nodeIdentifier.Name, nodeType.Name, value, true);
		location.Register(nodeIdentifier.Name, variable, nodeIdentifier.RangePosition);

		return Value.Null;
	}

	public Value Evaluate(Scope location, AssignmentNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		Value value = node.Value.Accept(this, location);
		Symbol symbol = location.Read(nodeIdentifier.Name, nodeIdentifier.RangePosition);
		symbol.Assign(value, nodeIdentifier.RangePosition);
		return Value.Null;
	}

	public Value Evaluate(Scope location, InvokationNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		IEnumerable<Value> arguments = node.Arguments.Select(argument => TypeHelper.Unwrap(argument.Accept(this, location)));
		Symbol symbol = location.Read(nodeTarget.Name, nodeTarget.RangePosition);
		if (symbol is not Operator @operator) throw new NotExistIssue($"Operator '{nodeTarget.Name}' in {location}", nodeTarget.RangePosition);
		Operation operation = @operator.TryReadOperation(arguments.Select(result => result.Tag)) ?? throw new NotExistIssue($"Operation '{nodeTarget.Name}'", nodeTarget.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke(arguments, scope, node.RangePosition);
	}

	public Value Evaluate(Scope location, UnaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Value target = TypeHelper.Unwrap(node.Target.Accept(this, location));
		Symbol symbol = location.Read(target.Tag, node.Target.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{target.Tag}' in {location}", node.Target.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [target.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([target], scope, node.RangePosition);
	}

	public Value Evaluate(Scope location, BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Value left = TypeHelper.Unwrap(node.Left.Accept(this, location));
		Value right = TypeHelper.Unwrap(node.Right.Accept(this, location));
		Symbol symbol = location.Read(left.Tag, node.Left.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{left.Tag}' in {location}", node.Left.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [left.Tag, right.Tag], nodeOperator.RangePosition);
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
		if (condition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", condition.Tag, node.Condition.RangePosition);
		Node? nodeBranch = condition.As<bool>().Content ? node.Then : node.Else;
		nodeBranch?.Accept(this, location);
		return Value.Null;
	}

	public Value Evaluate(Scope location, WhileStatementNode node)
	{
		while (true)
		{
			Value condition = node.Condition.Accept(this, location);
			if (condition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", condition.Tag, node.Condition.RangePosition);
			if (!condition.As<bool>().Content) break;
			try { node.Body.Accept(this, location); }
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
