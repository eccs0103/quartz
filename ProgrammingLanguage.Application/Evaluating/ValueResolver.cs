using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Application.Parsing;

namespace ProgrammingLanguage.Application.Evaluating;

internal class ValueResolver(Module module) : IResolverVisitor<ValueNode>
{
	public IdentifierResolver Nominator { get; set; } = default!;

	public ValueNode Visit(ValueNode node)
	{
		return node;
	}

	public ValueNode Visit(IdentifierNode node)
	{
		Datum datum = module.ReadDatum(node.Name, node.RangePosition);
		return new ValueNode(datum.Tag, datum.Value, node.RangePosition);
	}

	public ValueNode Visit(DeclarationNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		ValueNode nodeValue = node.Value.Accept(this);
		module.RegisterVariable(nodeValue.Tag, nodeIdentifier.Name, nodeValue.Value, nodeIdentifier.RangePosition);
		return ValueNode.NullableAt("Number", node.RangePosition);
	}

	public ValueNode Visit(AssignmentNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		ValueNode nodeValue = node.Value.Accept(this);
		module.WriteDatum(nodeIdentifier.Name, nodeValue.Tag, nodeValue.Value, nodeIdentifier.RangePosition);
		return ValueNode.NullableAt("Number", node.RangePosition);
	}

	public ValueNode Visit(InvokationNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		Operation operation = module.ReadOperation(nodeTarget.Name, nodeTarget.RangePosition);
		Node result = operation.Invoke(nodeTarget, node.Arguments, node.RangePosition >> nodeTarget.RangePosition);
		return result.Accept(this);
	}

	public ValueNode Visit(UnaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		ValueNode nodeTarget = node.Target.Accept(this);
		Structure type = module.ReadType(nodeTarget.Tag, nodeTarget.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, nodeOperator.RangePosition);
		Node result = operation.Invoke(nodeOperator, [nodeTarget], node.RangePosition >> nodeTarget.RangePosition);
		return result.Accept(this);
	}

	public ValueNode Visit(BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		ValueNode nodeLeft = node.Left.Accept(this);
		Structure type = module.ReadType(nodeLeft.Tag, nodeLeft.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, nodeOperator.RangePosition);
		ValueNode nodeRight = node.Right.Accept(this);
		Node result = operation.Invoke(nodeOperator, [nodeLeft, nodeRight], nodeLeft.RangePosition >> nodeRight.RangePosition);
		return result.Accept(this);
	}
}
