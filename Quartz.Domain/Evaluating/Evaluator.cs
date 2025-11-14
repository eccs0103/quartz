using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;

namespace Quartz.Domain.Evaluating;

internal class Evaluator() : IAstVisitor<ValueNode>
{
	public ValueNode Visit(Scope location, ValueNode node)
	{
		return node;
	}

	public ValueNode Visit(Scope location, IdentifierNode node)
	{
		Symbol symbol = location.Read(node.Name, node.RangePosition);
		if (symbol is Datum datum) return new ValueNode(datum.Tag, datum.Value, node.RangePosition);
		throw new NotExistIssue($"Identifier '{node.Name}' in {location}", node.RangePosition);
	}

	public ValueNode Visit(Scope location, DeclarationNode node)
	{
		IdentifierNode nodeType = node.Type;
		IdentifierNode nodeIdentifier = node.Identifier;
		ValueNode nodeValue = node.Value.Accept(this, location);
		if (nodeType.Name != nodeValue.Tag) throw new TypeMismatchIssue(nodeValue.Tag, nodeType.Name, nodeValue.RangePosition);
		Datum variable = new(nodeIdentifier.Name, nodeType.Name, nodeValue.Value!, true);
		location.Register(nodeIdentifier.Name, variable, nodeIdentifier.RangePosition);
		return ValueNode.NullAt(node.RangePosition);
	}

	public ValueNode Visit(Scope location, AssignmentNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		ValueNode nodeValue = node.Value.Accept(this, location);
		Symbol symbol = location.Read(nodeIdentifier.Name, nodeIdentifier.RangePosition);
		symbol.Assign(nodeValue, nodeIdentifier.RangePosition);
		return ValueNode.NullAt(node.RangePosition);
	}

	public ValueNode Visit(Scope location, InvokationNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		IEnumerable<ValueNode> arguments = node.Arguments.Select(argument => argument.Accept(this, location));
		Symbol symbol = location.Read(nodeTarget.Name, nodeTarget.RangePosition);
		if (symbol is not Operator @operator) throw new NotExistIssue($"Operator '{nodeTarget.Name}' in {location}", nodeTarget.RangePosition);
		Operation operation = @operator.ReadOperation(arguments.Select(result => result.Tag), nodeTarget.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke(arguments, scope, node.RangePosition);
	}

	public ValueNode Visit(Scope location, UnaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		ValueNode nodeTarget = node.Target.Accept(this, location);
		Symbol symbol = location.Read(nodeTarget.Tag, nodeTarget.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{nodeTarget.Tag}' in {location}", nodeTarget.RangePosition);
		Operator @operator = type.ReadOperator(nodeOperator.Name, nodeOperator.RangePosition);
		Operation operation = @operator.ReadOperation([nodeTarget.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([nodeTarget], scope, node.RangePosition);
	}

	public ValueNode Visit(Scope location, BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		ValueNode nodeLeft = node.Left.Accept(this, location);
		ValueNode nodeRight = node.Right.Accept(this, location);
		Symbol symbol = location.Read(nodeLeft.Tag, nodeLeft.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{nodeLeft.Tag}' in {location}", nodeLeft.RangePosition);
		Operator @operator = type.ReadOperator(nodeOperator.Name, nodeOperator.RangePosition);
		Operation operation = @operator.ReadOperation([nodeLeft.Tag, nodeRight.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([nodeLeft, nodeRight], scope, node.RangePosition);
	}

	public ValueNode Visit(Scope location, BlockNode node)
	{
		Scope scope = location.GetSubscope("Block");
		foreach (Node statement in node.Statements) statement.Accept(this, scope);
		return ValueNode.NullAt(node.RangePosition);
	}

	public ValueNode Visit(Scope location, IfStatementNode node)
	{
		ValueNode condition = node.Condition.Accept(this, location);
		if (condition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", condition.Tag, node.Condition.RangePosition);
		Node? nodeBranch = condition.ValueAs<bool>() ? node.Then : node.Else;
		nodeBranch?.Accept(this, location);
		return ValueNode.NullAt(node.RangePosition);
	}
}
