using Quartz.Domain.Parsing;

namespace Quartz.Domain.Evaluating;

public interface IAstVisitor<out TReturn>
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
	public TReturn Visit(Scope location, WhileStatementNode node);
	public TReturn Visit(Scope location, RepeatStatementNode node);
	public TReturn Visit(Scope location, BreakStatementNode node);
	public TReturn Visit(Scope location, ContinueStatementNode node);
}
