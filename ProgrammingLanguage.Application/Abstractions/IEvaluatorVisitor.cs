using ProgrammingLanguage.Application.Parsing;

namespace ProgrammingLanguage.Application.Abstractions;

public interface IEvaluatorVisitor<T>
{
	public T Visit(ValueNode node);
	public T Visit(IdentifierNode node);
	public T Visit(DeclarationNode node);
	public T Visit(InvokationNode node);
	public T Visit(UnaryOperatorNode node);
	public T Visit(BinaryOperatorNode node);
}
