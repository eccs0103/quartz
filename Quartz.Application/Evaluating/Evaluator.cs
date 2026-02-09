using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;
using Quartz.Domain.Evaluating;

namespace Quartz.Application.Evaluating;

internal class Evaluator : IAstVisitor<Instance>
{
	public Instance Visit(Scope location, ValueNode node)
	{
		object value = node.Value ?? Null.Instance;
		return new Instance(node.Tag, value, location);
	}

	public Instance Visit(Scope location, IdentifierNode node)
	{
		Symbol symbol = location.Read(node.Name, node.RangePosition);
		if (symbol is Datum datum) return datum.Value;
		throw new NotExistIssue($"Identifier '{node.Name}' in {location}", node.RangePosition);
	}

	/// TODO Refactor
	public Instance Visit(Scope location, DeclarationNode node)
	{
		IdentifierNode nodeType = node.Type;
		IdentifierNode nodeIdentifier = node.Identifier;

		Instance nodeValue;
		if (node.Value != null)
		{
			nodeValue = node.Value.Accept(this, location);
			if (!TypeHelper.IsCompatible(nodeType.Name, nodeValue.Tag)) throw new TypeMismatchIssue(nodeType.Name, nodeValue.Tag, node.Value.RangePosition);
		}
		else
		{
			if (!TypeHelper.IsOptional(nodeType.Name)) throw new InitializationRequiredIssue(nodeIdentifier.Name, nodeType.Name, nodeIdentifier.RangePosition);
			nodeValue = new Instance("Null", Null.Instance, location);
		}

		Datum variable = new(nodeIdentifier.Name, nodeType.Name, nodeValue, true);
		location.Register(nodeIdentifier.Name, variable, nodeIdentifier.RangePosition);

		return new Instance("Null", Null.Instance, location);
	}

	public Instance Visit(Scope location, AssignmentNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		Instance nodeValue = node.Value.Accept(this, location);
		Symbol symbol = location.Read(nodeIdentifier.Name, nodeIdentifier.RangePosition);
		symbol.Assign(nodeValue, nodeIdentifier.RangePosition);
		return new Instance("Null", Null.Instance, location);
	}

	public Instance Visit(Scope location, InvokationNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		IEnumerable<Instance> arguments = node.Arguments.Select(argument => TypeHelper.Unwrap(argument.Accept(this, location)));
		Symbol symbol = location.Read(nodeTarget.Name, nodeTarget.RangePosition);
		if (symbol is not Operator @operator) throw new NotExistIssue($"Operator '{nodeTarget.Name}' in {location}", nodeTarget.RangePosition);
		Operation operation = @operator.TryReadOperation(arguments.Select(result => result.Tag)) ?? throw new NotExistIssue($"Operation '{nodeTarget.Name}'", nodeTarget.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke(arguments, scope, node.RangePosition);
	}

	public Instance Visit(Scope location, UnaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Instance nodeTarget = TypeHelper.Unwrap(node.Target.Accept(this, location));
		Symbol symbol = location.Read(nodeTarget.Tag, node.Target.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{nodeTarget.Tag}' in {location}", node.Target.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [nodeTarget.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([nodeTarget], scope, node.RangePosition);
	}

	public Instance Visit(Scope location, BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Instance nodeLeft = TypeHelper.Unwrap(node.Left.Accept(this, location));
		Instance nodeRight = TypeHelper.Unwrap(node.Right.Accept(this, location));
		Symbol symbol = location.Read(nodeLeft.Tag, node.Left.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{nodeLeft.Tag}' in {location}", node.Left.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [nodeLeft.Tag, nodeRight.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([nodeLeft, nodeRight], scope, node.RangePosition);
	}

	public Instance Visit(Scope location, BlockNode node)
	{
		Scope scope = location.GetSubscope("Block");
		foreach (Node statement in node.Statements) statement.Accept(this, scope);
		return new Instance("Null", Null.Instance, location);
	}

	public Instance Visit(Scope location, IfStatementNode node)
	{
		Instance nodeCondition = node.Condition.Accept(this, location);
		if (nodeCondition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", nodeCondition.Tag, node.Condition.RangePosition);
		Node? nodeBranch = nodeCondition.ValueAs<bool>() ? node.Then : node.Else;
		nodeBranch?.Accept(this, location);
		return new Instance("Null", Null.Instance, location);
	}

	public Instance Visit(Scope location, WhileStatementNode node)
	{
		while (true)
		{
			Instance nodeCondition = node.Condition.Accept(this, location);
			if (nodeCondition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", nodeCondition.Tag, node.Condition.RangePosition);
			if (!nodeCondition.ValueAs<bool>()) break;
			try { node.Body.Accept(this, location); }
			catch (ContinueSignal) { continue; }
			catch (BreakSignal) { break; }
		}
		return new Instance("Null", Null.Instance, location);
	}

	public Instance Visit(Scope location, BreakStatementNode node)
	{
		throw new BreakSignal();
	}

	public Instance Visit(Scope location, ContinueStatementNode node)
	{
		throw new ContinueSignal();
	}
}
