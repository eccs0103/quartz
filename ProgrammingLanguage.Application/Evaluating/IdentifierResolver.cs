using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Application.Parsing;

namespace ProgrammingLanguage.Application.Evaluating;

internal class IdentifierResolver(Module module) : IResolverVisitor<IdentifierNode>
{
	public ValueResolver Valuator { get; set; } = default!;

	public IdentifierNode Visit(ValueNode node)
	{
		throw new NotImplementedException();
	}

	public IdentifierNode Visit(IdentifierNode node)
	{
		return node;
	}

	public IdentifierNode Visit(DeclarationNode node)
	{
		throw new NotImplementedException();
	}

	public IdentifierNode Visit(AssignmentNode node)
	{
		throw new NotImplementedException();
	}

	public IdentifierNode Visit(InvokationNode node)
	{
		throw new NotImplementedException();
	}

	public IdentifierNode Visit(UnaryOperatorNode node)
	{
		throw new NotImplementedException();
	}

	public IdentifierNode Visit(BinaryOperatorNode node)
	{
		throw new NotImplementedException();
	}

	public IdentifierNode Visit(BlockNode node)
	{
		throw new NotImplementedException();
	}

	public IdentifierNode Visit(IfStatementNode node)
	{
		throw new NotImplementedException();
	}
}
