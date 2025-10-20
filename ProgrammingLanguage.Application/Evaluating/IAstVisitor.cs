using ProgrammingLanguage.Application.Parsing;

namespace ProgrammingLanguage.Application.Evaluating;

internal interface IAstVisitor<out TReturn>
{
	public TReturn Visit(Scope location, ValueNode node);
	public TReturn Visit(Scope location, IdentifierNode node);
	public TReturn Visit(Scope location, DeclarationNode node);
	public TReturn Visit(Scope location, AssignmentNode nod);
	public TReturn Visit(Scope location, InvokationNode node);
	public TReturn Visit(Scope location, UnaryOperatorNode node);
	public TReturn Visit(Scope location, BinaryOperatorNode node);
	public TReturn Visit(Scope location, BlockNode node);
	public TReturn Visit(Scope location, IfStatementNode node);
}
