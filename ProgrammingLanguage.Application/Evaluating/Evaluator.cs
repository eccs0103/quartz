using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Evaluator() : IAstVisitor<ValueNode>
{
	public ValueNode Visit(Scope location, ValueNode node)
	{
		return node;
	}

	public ValueNode Visit(Scope location, IdentifierNode node)
	{
		Property datum = location.Read(node.Name, node.RangePosition);
		return new ValueNode(datum.Tag, datum.Value, node.RangePosition);
	}

	public ValueNode Visit(Scope location, DeclarationNode node)
	{
		IdentifierNode nodeType = node.Type;
		IdentifierNode nodeIdentifier = node.Identifier;
		ValueNode nodeValue = node.Value.Accept(this, location);
		if (nodeType.Name != nodeValue.Tag) throw new TypeMismatchIssue(nodeValue.Tag, nodeType.Name, nodeValue.RangePosition);
		Property variable = new(nodeIdentifier.Name, nodeType.Name, nodeValue.Value!, Property.MutableOptions);
		location.Register(nodeIdentifier.Name, variable, nodeIdentifier.RangePosition);
		return ValueNode.NullableAt("Number", node.RangePosition);
	}

	public ValueNode Visit(Scope location, AssignmentNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		ValueNode nodeValue = node.Value.Accept(this, location);
		location.Write(nodeIdentifier.Name, nodeValue.Tag, nodeValue.Value!, nodeIdentifier.RangePosition);
		return ValueNode.NullableAt("Number", node.RangePosition);
	}

	public ValueNode Visit(Scope location, InvokationNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		IEnumerable<ValueNode> arguments = node.Arguments.Select(argument => argument.Accept(this, location));
		Property property = location.Read(nodeTarget.Name, nodeTarget.RangePosition);
		if (property is not OverloadSet set) throw new NotExistIssue($"Function '{nodeTarget.Name}' in {location}", nodeTarget.RangePosition);
		Operation operation = set.ReadOperation(arguments.Select(result => result.Tag), nodeTarget.RangePosition);
		return operation.Invoke(arguments, node.RangePosition);
	}

	public ValueNode Visit(Scope location, UnaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		ValueNode nodeTarget = node.Target.Accept(this, location);
		Property property = location.Read(nodeTarget.Tag, nodeTarget.RangePosition);
		if (property is not Structure type) throw new NotExistIssue($"Type '{nodeTarget.Tag}' not found in {location}", nodeTarget.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [nodeTarget.Tag], nodeOperator.RangePosition);
		return operation.Invoke([nodeTarget], node.RangePosition);
	}

	public ValueNode Visit(Scope location, BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		ValueNode nodeLeft = node.Left.Accept(this, location);
		ValueNode nodeRight = node.Right.Accept(this, location);
		Property property = location.Read(nodeLeft.Tag, nodeLeft.RangePosition);
		if (property is not Structure type) throw new NotExistIssue($"Type '{nodeLeft.Tag}' not found in {location}", nodeLeft.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [nodeLeft.Tag, nodeRight.Tag], nodeOperator.RangePosition);
		return operation.Invoke([nodeLeft, nodeRight], node.RangePosition);
	}

	public ValueNode Visit(Scope location, BlockNode node)
	{
		Scope scope = new("Block", location);
		foreach (Node statement in node.Statements) statement.Accept(this, scope);
		return ValueNode.NullableAt("Number", node.RangePosition);
	}

	public ValueNode Visit(Scope location, IfStatementNode node)
	{
		ValueNode condition = node.Condition.Accept(this, location);
		if (condition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", condition.Tag, node.Condition.RangePosition);
		(condition.ValueAs<bool>() ? node.Then : node.Else)?.Accept(this, location);
		return ValueNode.NullableAt("Number", node.RangePosition);
	}
}
