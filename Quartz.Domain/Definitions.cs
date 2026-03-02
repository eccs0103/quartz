namespace Quartz.Domain;

public static class Definitions
{
	public static class Keywords
	{
		public const string True = "true";
		public const string False = "false";
		public const string Null = "null";
		public const string If = "if";
		public const string Else = "else";
		public const string While = "while";
		public const string For = "for";
		public const string In = "in";
		public const string Continue = "continue";
		public const string Break = "break";

		public static readonly string[] All = [True, False, Null, If, Else, While, For, In, Continue, Break];
	}

	public static class Types
	{
		public const string Any = "Any";
		public const string Null = "Null";
		public const string Nullable = "Nullable";
		public const string Boolean = "Boolean";
		public const string Character = "Character";
		public const string Number = "Number";
		public const string String = "String";
		public const string Type = "Type";
		public const string Template = "Template";
		public const string Sequence = "Sequence";
		public const string Array = "Array";
		public const string Function = "Function";
		public const string Workspace = "@Workspace";
	}

	public static class Operators
	{
		public const string Equal = "=";
		public const string NotEqual = "!=";
		public const string Greater = ">";
		public const string Less = "<";
		public const string GreaterOrEqual = ">=";
		public const string LessOrEqual = "<=";
		public const string Plus = "+";
		public const string Minus = "-";
		public const string Multiply = "*";
		public const string Divide = "/";
		public const string Colon = ":";
		public const string Question = "?";
		public const string And = "&";
		public const string Or = "|";
		public const string Not = "!";
		public const string Dot = ".";
	}

	public static class Brackets
	{
		public const string OpenParen = "(";
		public const string CloseParen = ")";
		public const string OpenBrace = "{";
		public const string CloseBrace = "}";
		public const string OpenAngle = "<";
		public const string CloseAngle = ">";
		public const string OpenBracket = "[";
		public const string CloseBracket = "]";
	}

	public static class Separators
	{
		public const string Semicolon = ";";
		public const string Comma = ",";
	}
}
