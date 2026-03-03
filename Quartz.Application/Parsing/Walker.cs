using System.Diagnostics.CodeAnalysis;
using System.Text;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Exceptions.Parsing;
using Quartz.Domain.Lexing;
using Quartz.Shared.Helpers;
using static System.Math;
using static Quartz.Domain.Lexing.Token;

namespace Quartz.Application.Parsing;

internal class Walker(Token[] tokens, Range<uint> range)
{
	private Token[] Tokens { get; } = tokens;
	public Range<uint> RangeIndex { get; } = range;
	private Wrapper<uint> IndexWrapper = new(0);

	public uint Index
	{
		get => IndexWrapper.Value;
		set => IndexWrapper.Value = value;
	}
	public bool InRange => Max(RangeIndex.Begin, 0) <= Index && Index < Min(Tokens.Length, RangeIndex.End);
	public Range<Position> RangePosition => (Tokens.Length > RangeIndex.Begin ? Tokens[RangeIndex.Begin].RangePosition.Begin : Position.Zero) >> (Tokens.Length > 0 && RangeIndex.End > 0 && RangeIndex.End <= Tokens.Length ? Tokens[RangeIndex.End - 1].RangePosition.End : Position.Zero);

	public Walker(Token[] tokens) : this(tokens, new(0, (uint) tokens.Length))
	{
	}

	public override string ToString()
	{
		StringBuilder builder = new();
		for (uint index = RangeIndex.Begin; index < RangeIndex.End; index++)
		{
			if (index != RangeIndex.Begin) builder.Append(' ');
			builder.Append(Tokens[index].Value);
		}
		return builder.ToString();
	}

	public bool Peek([NotNullWhen(true)] out Token? token)
	{
		if (!InRange)
		{
			token = default!;
			return false;
		}
		token = Tokens[Index];
		return true;
	}

	public bool Peek([NotNullWhen(true)] out Token? token, int offset)
	{
		uint current = Index;
		Index = (uint) (Index + offset);
		bool result = Peek(out token);
		Index = current;
		return result;
	}

	public bool Attempt(Action action)
	{
		uint mark = Index;
		try
		{
			action.Invoke();
			return true;
		}
		catch (Issue)
		{
			Index = mark;
			return false;
		}
	}

	public bool Attempt<T>(Func<T> action, [NotNullWhen(true)] out T? result)
		where T : class
	{
		uint mark = Index;
		try
		{
			result = action.Invoke();
			return true;
		}
		catch (Issue)
		{
			Index = mark;
			result = null;
			return false;
		}
	}

	public Walker GetSubwalker(in uint begin, in uint end)
	{
		return new Walker(Tokens, new(begin, end))
		{
			IndexWrapper = IndexWrapper
		};
	}

	public Walker GetSubwalker(string bracket, string pair)
	{
		uint counter = 1;
		uint begin = ++Index;
		for (; Index < RangeIndex.End; Index++)
		{
			if (!Peek(out Token? token)) continue;
			if (token.Represents(Types.Bracket, bracket)) counter++;
			else if (token.Represents(Types.Bracket, pair)) counter--;
			if (counter != 0) continue;
			uint end = Index;
			Index = begin;
			return GetSubwalker(begin, end);
		}
		throw new ExpectedIssue(pair, ~RangePosition.End);
	}
}
