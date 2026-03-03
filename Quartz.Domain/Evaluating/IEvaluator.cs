using Quartz.Domain.Parsing;

namespace Quartz.Domain.Evaluating;

public interface IEvaluator<out TReturn>
{
	public TReturn Evaluate(Scope location, ValueNode node);
	public TReturn Evaluate(Scope location, IdentifierNode node);
	public TReturn Evaluate(Scope location, TemplateNode node);
	public TReturn Evaluate(Scope location, DeclarationNode node);
	public TReturn Evaluate(Scope location, AssignmentNode node);
	public TReturn Evaluate(Scope location, ArrayNode node);
	public TReturn Evaluate(Scope location, IndexNode node);
	public TReturn Evaluate(Scope location, InvocationNode node);
	public TReturn Evaluate(Scope location, FieldNode node);
	public TReturn Evaluate(Scope location, UnaryOperatorNode node);
	public TReturn Evaluate(Scope location, BinaryOperatorNode node);
	public TReturn Evaluate(Scope location, BlockNode node);
	public TReturn Evaluate(Scope location, IfStatementNode node);
	public TReturn Evaluate(Scope location, WhileStatementNode node);
	public TReturn Evaluate(Scope location, ForStatementNode node);
	public TReturn Evaluate(Scope location, BreakStatementNode node);
	public TReturn Evaluate(Scope location, ContinueStatementNode node);
}
