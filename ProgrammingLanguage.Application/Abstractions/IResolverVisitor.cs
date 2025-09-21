using ProgrammingLanguage.Application.Parsing;

namespace ProgrammingLanguage.Application.Abstractions;

internal interface IResolverVisitor<out T>
{
	public T Visit(ValueNode node);
	public T Visit(IdentifierNode node);
	public T Visit(DeclarationNode node);
	public T Visit(AssignmentNode node);
	public T Visit(InvokationNode node);
	public T Visit(UnaryOperatorNode node);
	public T Visit(BinaryOperatorNode node);
}
