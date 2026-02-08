using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;

namespace Quartz.Domain.Evaluating;

internal class Evaluator : IAstVisitor<Instance>
{
	public Instance Visit(Scope location, ValueNode node)
	{
		return new Instance(node.Tag, node.Value, node.RangePosition, location);
	}

	public Instance Visit(Scope location, IdentifierNode node)
	{
		Symbol symbol = location.Read(node.Name, node.RangePosition);
		if (symbol is Datum datum) return new Instance(datum.Tag, datum.Value, node.RangePosition, location);
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
			if (!TypeHelper.IsCompatible(nodeType.Name, nodeValue.Tag)) throw new TypeMismatchIssue(nodeType.Name, nodeValue.Tag, nodeValue.RangePosition);
		}
		else
		{
			if (!TypeHelper.IsOptional(nodeType.Name)) throw new InitializationRequiredIssue(nodeIdentifier.Name, nodeType.Name, nodeIdentifier.RangePosition);
			nodeValue = new Instance("Null", null, nodeIdentifier.RangePosition, location);
		}

		Datum variable = new(nodeIdentifier.Name, nodeType.Name, nodeValue.ValueAs<object>(), true);
		location.Register(nodeIdentifier.Name, variable, nodeIdentifier.RangePosition);

		return new Instance("Null", null, node.RangePosition, location);
	}

	public Instance Visit(Scope location, AssignmentNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		Instance nodeValue = node.Value.Accept(this, location);
		Symbol symbol = location.Read(nodeIdentifier.Name, nodeIdentifier.RangePosition);
		symbol.Assign(nodeValue, nodeIdentifier.RangePosition);
		return new Instance("Null", null, node.RangePosition, location);
	}

	public Instance Visit(Scope location, InvokationNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		IEnumerable<Instance> arguments = node.Arguments.Select(argument => argument.Accept(this, location).Unwrap());
		Symbol symbol = location.Read(nodeTarget.Name, nodeTarget.RangePosition);
		if (symbol is not Operator @operator) throw new NotExistIssue($"Operator '{nodeTarget.Name}' in {location}", nodeTarget.RangePosition);
		Operation operation = @operator.TryReadOperation(arguments.Select(result => result.Tag)) ?? throw new NotExistIssue($"Operation '{nodeTarget.Name}'", nodeTarget.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke(arguments, scope, node.RangePosition);
	}

	public Instance Visit(Scope location, UnaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Instance nodeTarget = node.Target.Accept(this, location).Unwrap();
		Symbol symbol = location.Read(nodeTarget.Tag, nodeTarget.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{nodeTarget.Tag}' in {location}", nodeTarget.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [nodeTarget.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([nodeTarget], scope, node.RangePosition);
	}

	public Instance Visit(Scope location, BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Instance nodeLeft = node.Left.Accept(this, location).Unwrap();
		Instance nodeRight = node.Right.Accept(this, location).Unwrap();
		Symbol symbol = location.Read(nodeLeft.Tag, nodeLeft.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{nodeLeft.Tag}' in {location}", nodeLeft.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [nodeLeft.Tag, nodeRight.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([nodeLeft, nodeRight], scope, node.RangePosition);
	}

	public Instance Visit(Scope location, BlockNode node)
	{
		Scope scope = location.GetSubscope("Block");
		foreach (Node statement in node.Statements) statement.Accept(this, scope);
		return new Instance("Null", null, node.RangePosition, location);
	}

	public Instance Visit(Scope location, IfStatementNode node)
	{
		Instance nodeCondition = node.Condition.Accept(this, location);
		if (nodeCondition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", nodeCondition.Tag, nodeCondition.RangePosition);
		Node? nodeBranch = nodeCondition.ValueAs<bool>() ? node.Then : node.Else;
		nodeBranch?.Accept(this, location);
		return new Instance("Null", null, node.RangePosition, location);
	}

	public Instance Visit(Scope location, WhileStatementNode node)
	{
		while (true)
		{
			Instance nodeCondition = node.Condition.Accept(this, location);
			if (nodeCondition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", nodeCondition.Tag, nodeCondition.RangePosition);
			if (!nodeCondition.ValueAs<bool>()) break;
			try { node.Body.Accept(this, location); }
			catch (ContinueSignal) { continue; }
			catch (BreakSignal) { break; }
		}
		return new Instance("Null", null, node.RangePosition, location);
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
