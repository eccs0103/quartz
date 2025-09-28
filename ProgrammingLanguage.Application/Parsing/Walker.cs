using System.Diagnostics.CodeAnalysis;
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

	public bool GetToken([NotNullWhen(true)] out Token? token)
	{
		if (!InRange)
		{
			token = default!;
			return false;
		}
		token = Tokens[Index];
		return true;
	}

	public Walker GetSubwalker(in uint begin, in uint end)
	{
		return new(Tokens, new(begin, end))
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
			if (!GetToken(out Token? token)) continue;
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
