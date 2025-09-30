using System.Diagnostics.CodeAnalysis;
using System.Text;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Lexing;
using ProgrammingLanguage.Shared.Helpers;
using static System.Math;
using static ProgrammingLanguage.Application.Lexing.Token;

namespace ProgrammingLanguage.Application.Parsing;

internal class Walker(Token[] tokens, Range<uint> range)
{
	private readonly Token[] Tokens = tokens;
	public readonly Range<uint> RangeIndex = range;
	private Wrapper<uint> IndexWrapper = new(0);

	public uint Index
	{
		get => IndexWrapper.Value;
		set => IndexWrapper.Value = value;
	}
	public bool InRange => Max(RangeIndex.Begin, 0) <= Index && Index < Min(Tokens.Length, RangeIndex.End);
	public readonly Range<Position> RangePosition = (tokens.FirstOrDefault()?.RangePosition.Begin ?? Position.Zero) >> (tokens.LastOrDefault()?.RangePosition.End ?? Position.Zero);

	public Walker(Token[] tokens) : this(tokens, new(0, Convert.ToUInt32(tokens.Length)))
	{
	}

	public override string ToString()
	{
		StringBuilder builder = new();
		for (uint index = RangeIndex.Begin; index < RangeIndex.End; index++)
		{
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
		uint begin = Index + 1;
		for (Index++; Index < RangeIndex.End; Index++)
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
