using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;

namespace Quartz.Application.Evaluating;

internal class Evaluator : IAstVisitor<Instance>
{
	public Instance Visit(Scope location, ValueNode node)
	{
		object value = node.Value ?? Instance.Empty;
		return new Instance<object>(node.Tag, value);
	}

	public Instance Visit(Scope location, IdentifierNode node)
	{
		Symbol symbol = Resolve(location, node);
		if (symbol is Datum datum) return datum.Value;
		if (symbol is Class type) return new Instance<Class>("Type", type);
		throw new NotExistIssue($"Identifier '{node.Name}' in {location}", node.RangePosition);
	}

	private static Symbol Resolve(Scope location, IdentifierNode node)
	{
		if (node is GenericNode generic)
		{
			Symbol symbol = location.Read(generic.Target.Name, generic.Target.RangePosition);
			if (symbol is not Generic genericType) throw new TypeMismatchIssue("Generic", symbol.Name, generic.Target.RangePosition);

			List<Class> arguments = [];
			foreach (IdentifierNode argumentNode in generic.Generics)
			{
				Symbol argumentSymbol = Resolve(location, argumentNode);
				if (argumentSymbol is not Class argumentClass) throw new TypeMismatchIssue("Class", argumentSymbol.Name, argumentNode.RangePosition);
				arguments.Add(argumentClass);
			}

			string name = generic.ToString();
			if (location.TryRead(name, out Symbol? existing)) return existing;

			Symbol instance = genericType.Instantiate(name, arguments);
			location.Register(name, instance, generic.RangePosition);
			return instance;
		}

		return location.Read(node.Name, node.RangePosition);
	}

	public Instance Visit(Scope location, DeclarationNode node)
	{
		IdentifierNode nodeType = node.Type;
		IdentifierNode nodeIdentifier = node.Identifier;

		if (node.Value == null && !TypeHelper.IsOptional(nodeType.Name)) throw new InitializationRequiredIssue(nodeIdentifier.Name, nodeType.Name, nodeIdentifier.RangePosition);

		Instance instance = node.Value?.Accept(this, location) ?? Instance.Null;

		if (node.Value != null && !TypeHelper.IsCompatible(nodeType.Name, instance.Tag)) throw new TypeMismatchIssue(nodeType.Name, instance.Tag, node.Value.RangePosition);

		Datum variable = new(nodeIdentifier.Name, nodeType.Name, instance, true);
		location.Register(nodeIdentifier.Name, variable, nodeIdentifier.RangePosition);

		return Instance.Null;
	}

	public Instance Visit(Scope location, AssignmentNode node)
	{
		IdentifierNode nodeIdentifier = node.Identifier;
		Instance nodeValue = node.Value.Accept(this, location);
		Symbol symbol = location.Read(nodeIdentifier.Name, nodeIdentifier.RangePosition);
		symbol.Assign(nodeValue, nodeIdentifier.RangePosition);
		return Instance.Null;
	}

	public Instance Visit(Scope location, InvokationNode node)
	{
		IdentifierNode nodeTarget = node.Target;
		IEnumerable<Instance> arguments = node.Arguments.Select(argument => TypeHelper.Unwrap(argument.Accept(this, location)));
		Symbol symbol = location.Read(nodeTarget.Name, nodeTarget.RangePosition);
		if (symbol is not Operator @operator) throw new NotExistIssue($"Operator '{nodeTarget.Name}' in {location}", nodeTarget.RangePosition);
		Operation operation = @operator.TryReadOperation(arguments.Select(result => result.Tag)) ?? throw new NotExistIssue($"Operation '{nodeTarget.Name}'", nodeTarget.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke(arguments, scope, node.RangePosition);
	}

	public Instance Visit(Scope location, UnaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Instance nodeTarget = TypeHelper.Unwrap(node.Target.Accept(this, location));
		Symbol symbol = location.Read(nodeTarget.Tag, node.Target.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{nodeTarget.Tag}' in {location}", node.Target.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [nodeTarget.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([nodeTarget], scope, node.RangePosition);
	}

	public Instance Visit(Scope location, BinaryOperatorNode node)
	{
		IdentifierNode nodeOperator = node.Operator;
		Instance nodeLeft = TypeHelper.Unwrap(node.Left.Accept(this, location));
		Instance nodeRight = TypeHelper.Unwrap(node.Right.Accept(this, location));
		Symbol symbol = location.Read(nodeLeft.Tag, node.Left.RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{nodeLeft.Tag}' in {location}", node.Left.RangePosition);
		Operation operation = type.ReadOperation(nodeOperator.Name, [nodeLeft.Tag, nodeRight.Tag], nodeOperator.RangePosition);
		Scope scope = location.GetSubscope("Call");
		return operation.Invoke([nodeLeft, nodeRight], scope, node.RangePosition);
	}

	public Instance Visit(Scope location, BlockNode node)
	{
		Scope scope = location.GetSubscope("Block");
		foreach (Node statement in node.Statements) statement.Accept(this, scope);
		return Instance.Null;
	}

	public Instance Visit(Scope location, IfStatementNode node)
	{
		Instance nodeCondition = node.Condition.Accept(this, location);
		if (nodeCondition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", nodeCondition.Tag, node.Condition.RangePosition);
		Node? nodeBranch = nodeCondition.As<bool>().Value ? node.Then : node.Else;
		nodeBranch?.Accept(this, location);
		return Instance.Null;
	}

	public Instance Visit(Scope location, WhileStatementNode node)
	{
		while (true)
		{
			Instance nodeCondition = node.Condition.Accept(this, location);
			if (nodeCondition.Tag != "Boolean") throw new TypeMismatchIssue("Boolean", nodeCondition.Tag, node.Condition.RangePosition);
			if (!nodeCondition.As<bool>().Value) break;
			try { node.Body.Accept(this, location); }
			catch (ContinueSignal) { continue; }
			catch (BreakSignal) { break; }
		}
		return Instance.Null;
	}

	public Instance Visit(Scope location, BreakStatementNode node)
	{
		throw new BreakSignal();
	}

	public Instance Visit(Scope location, ContinueStatementNode node)
	{
		throw new ContinueSignal();
	}
}
