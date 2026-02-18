using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;

namespace Quartz.Application.Evaluating;

internal class Evaluator : IEvaluator<Value>
{
	public Value Evaluate(Scope location, ValueNode node)
	{
		object value = node.Value ?? Value.Empty;
		return new Value<object>(node.Tag, value);
	}

	public Value Evaluate(Scope location, IdentifierNode node)
	{
		if (location.TryRead(node.Name, out Variable? variable)) return variable.Value;
		throw new NotExistIssue($"Variable '{node.Name}' in {location}", node.RangePosition);
	}

	public Value Evaluate(Scope location, GenericNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		if (!location.TryRead(nodeTarget.Name, out Template? template)) throw new NotExistIssue($"Template '{nodeTarget.Name}' in {location}", nodeTarget.RangePosition);
		IEnumerable<Class> generics = node.Generics.Select((nodeGeneric) =>
		{
			Value value = nodeGeneric.Accept(this, location);
			if (value.Tag != TypeConstants.Type || value.Content is not Class type) throw new TypeMismatchIssue(TypeConstants.Type, value.Tag, nodeGeneric.RangePosition);
			return type;
		});
		if (location.TryRead(node.Name, out Variable? existing))
		{
			if (existing.Value is Value<Class> type) return type;
			throw new UnexpectedIssue($"Identifier '{node.Name}' is taken by something that is not a Class", node.RangePosition);
		}
		Class type2 = template.Assemble(node.Name, generics, node.RangePosition);
		if (!location.TryRegister(node.Name, new Value<Class>(TypeConstants.Type, type2))) throw new AlreadyExistsIssue($"Class '{node.Name}' in {location}", node.RangePosition);
		return new Value<Class>(TypeConstants.Type, type2);
	}

	public Value Evaluate(Scope location, DeclarationNode node)
	{
		IdentifierNode nodeType = node.Type;
		Value typeValue = nodeType.Accept(this, location);
		if (typeValue.Content is not Class typeClass) throw new TypeMismatchIssue(TypeConstants.Type, typeValue.Tag, nodeType.RangePosition);
		string typeName = typeClass.Name;
		IdentifierNode nodeIdentifier = node.Identifier;
		if (node.Value == null && !TypeHelper.IsOptional(typeName)) throw new InitializationRequiredIssue(nodeIdentifier.Name, typeName, nodeIdentifier.RangePosition);
		Value value = node.Value?.Accept(this, location) ?? Value.Null;
		if (!TypeHelper.IsCompatible(typeName, value.Tag, location)) throw new TypeMismatchIssue(typeName, value.Tag, node.Value?.RangePosition ?? nodeIdentifier.RangePosition);
		if (!location.TryRegister(nodeIdentifier.Name, typeName, value, true)) throw new AlreadyExistsIssue($"Variable '{nodeIdentifier.Name}' in {location}", nodeIdentifier.RangePosition);
		return Value.Null;
	}

	public Value Evaluate(Scope location, AssignmentNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		Value value = node.Value.Accept(this, location);
		if (!location.TryRead(nodeIdentifier.Name, out Variable? variable)) throw new NotExistIssue($"Variable '{nodeIdentifier.Name}' in {location}", nodeIdentifier.RangePosition);
		variable.Assign(value, location, nodeIdentifier.RangePosition);
		return Value.Null;
	}

	public Value Evaluate(Scope location, InvokationNode node)
	{
		IEnumerable<Value> arguments = node.Arguments.Select(argument => TypeHelper.Unwrap(argument.Accept(this, location)));
		if (node.Target is FieldNode memberAccess)
		{
			Value target = TypeHelper.Unwrap(memberAccess.Target.Accept(this, location));
			return target.RunOperation(memberAccess.Member.Name, arguments, location, node.RangePosition);
		}
		if (node.Target is IdentifierNode nodeTarget)
		{
			if (!location.TryRead(nodeTarget.Name, out Operator? @operator)) throw new NotExistIssue($"Operator '{nodeTarget.Name}' in {location}", nodeTarget.RangePosition);
			if (!@operator.TryReadOperation(arguments.Select(result => result.Tag), out Operation? operation)) throw new NoOverloadIssue(nodeTarget.Name, Convert.ToByte(arguments.Count()), nodeTarget.RangePosition);
			Scope scope = location.GetSubscope("Call");
			return operation.Invoke(arguments, scope, node.RangePosition);
		}
		throw new UnexpectedIssue($"Call target '{node.Target}' is not callable", node.Target.RangePosition);
	}

	public Value Evaluate(Scope location, FieldNode node)
	{
		Value target = TypeHelper.Unwrap(node.Target.Accept(this, location));
		if (!location.TryRead(target.Tag, out Class? type)) throw new NotExistIssue($"Type '{target.Tag}' in {location}", node.Target.RangePosition);
		if (!type.TryReadProperty(node.Member.Name, out Variable? variable)) throw new NotExistIssue($"Variable '{node.Member.Name}' in {location}", node.Member.RangePosition);
		return variable.Value;
	}

	public Value Evaluate(Scope location, UnaryOperatorNode node)
	{
		Node nodeTarget = node.Target;
		IdentifierNode nodeOperator = node.Operator;
		Value target = TypeHelper.Unwrap(nodeTarget.Accept(this, location));
		if (!location.TryRead(target.Tag, out Class? type)) throw new NotExistIssue($"Type '{target.Tag}' in {location}", nodeTarget.RangePosition);
		if (!type.TryReadOperation(nodeOperator.Name, [target.Tag], out Operation? operation)) throw new NoOverloadIssue(nodeOperator.Name, 1, nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([target], scope, node.RangePosition);
	}

	public Value Evaluate(Scope location, BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Value left = TypeHelper.Unwrap(node.Left.Accept(this, location));
		Value right = TypeHelper.Unwrap(node.Right.Accept(this, location));
		if (!location.TryRead(left.Tag, out Class? type)) throw new NotExistIssue($"Type '{left.Tag}' in {location}", node.Left.RangePosition);
		if (!type.TryReadOperation(nodeOperator.Name, [left.Tag, right.Tag], out Operation? operation)) throw new NoOverloadIssue(nodeOperator.Name, 2, nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([left, right], scope, node.RangePosition);
	}

	public Value Evaluate(Scope location, BlockNode node)
	{
		Scope scope = location.GetSubscope("Block");
		foreach (Node statement in node.Statements) statement.Accept(this, scope);
		return Value.Null;
	}

	public Value Evaluate(Scope location, IfStatementNode node)
	{
		Value condition = node.Condition.Accept(this, location);
		if (condition.Tag != TypeConstants.Boolean) throw new TypeMismatchIssue(TypeConstants.Boolean, condition.Tag, node.Condition.RangePosition);
		Node? nodeBranch = condition.As<bool>().Content ? node.Then : node.Else;
		nodeBranch?.Accept(this, location);
		return Value.Null;
	}

	public Value Evaluate(Scope location, WhileStatementNode node)
	{
		while (true)
		{
			Value condition = node.Condition.Accept(this, location);
			if (condition.Tag != TypeConstants.Boolean) throw new TypeMismatchIssue(TypeConstants.Boolean, condition.Tag, node.Condition.RangePosition);
			if (!condition.As<bool>().Content) break;
			try { node.Body.Accept(this, location); }
			catch (ContinueSignal) { continue; }
			catch (BreakSignal) { break; }
		}
		return Value.Null;
	}

	public Value Evaluate(Scope location, BreakStatementNode node)
	{
		throw new BreakSignal();
	}

	public Value Evaluate(Scope location, ContinueStatementNode node)
	{
		throw new ContinueSignal();
	}
}
