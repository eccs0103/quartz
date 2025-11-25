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

		// Переменная для значения (которое мы или вычислим, или создадим автоматически)
		ValueNode nodeValue;

		// Сценарий 1: Пользователь дал значение (name String = "Arman";)
		if (node.Value != null)
		{
			nodeValue = node.Value.Accept(this, location);
			// Проверяем совместимость типов через наш TypeHelper
			if (!TypeHelper.IsCompatible(nodeType.Name, nodeValue.Tag)) throw new TypeMismatchIssue(nodeType.Name, nodeValue.Tag, nodeValue.RangePosition);
		}
		// Сценарий 2: Пользователь НЕ дал значение (name String?; или name Null;)
		else
		{
			// Проверяем, разрешено ли этому типу быть пустым
			if (!TypeHelper.IsOptional(nodeType.Name)) throw new InitializationRequiredIssue(nodeIdentifier.Name, nodeType.Name, nodeIdentifier.RangePosition);
			// Если разрешено -> Автоматически создаем значение "Null"
			// Используем ~Position.Zero или позицию идентификатора, так как реального значения нет
			nodeValue = new ValueNode("Null", null, nodeIdentifier.RangePosition);
		}

		// Регистрируем переменную
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
		ValueNode nodeCondition = node.Condition.Accept(this, location);
		if (nodeCondition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", nodeCondition.Tag, nodeCondition.RangePosition);
		Node? nodeBranch = nodeCondition.ValueAs<bool>() ? node.Then : node.Else;
		nodeBranch?.Accept(this, location);
		return ValueNode.NullAt(node.RangePosition);
	}

	public ValueNode Visit(Scope location, WhileStatementNode node)
	{
		while (true)
		{
			ValueNode nodeCondition = node.Condition.Accept(this, location);
			if (nodeCondition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", nodeCondition.Tag, nodeCondition.RangePosition);
			if (!nodeCondition.ValueAs<bool>()) break;
			try { node.Body.Accept(this, location); }
			catch (ContinueSignal) { continue; }
			catch (BreakSignal) { break; }
		}
		return ValueNode.NullAt(node.RangePosition);
	}

	public ValueNode Visit(Scope location, RepeatStatementNode node)
	{
		ValueNode nodeCount = node.Count.Accept(this, location);
		if (nodeCount.Tag != "Number") throw new TypeMismatchIssue("Number", nodeCount.Tag, nodeCount.RangePosition);
		long count = Convert.ToInt64(nodeCount.ValueAs<double>());
		while (true)
		{
			if (count <= 0) break;
			try { node.Body.Accept(this, location); }
			catch (ContinueSignal) { continue; }
			catch (BreakSignal) { break; }
			count--;
		}
		return ValueNode.NullAt(node.RangePosition);
	}

	public ValueNode Visit(Scope location, BreakStatementNode node)
	{
		throw new BreakSignal();
	}

	public ValueNode Visit(Scope location, ContinueStatementNode node)
	{
		throw new ContinueSignal();
	}
}
