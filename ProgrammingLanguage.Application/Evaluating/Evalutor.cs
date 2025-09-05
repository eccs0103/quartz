using static System.Math;

namespace ProgrammingLanguage.Application.Evaluating;

internal partial class Evalutor
{
	//private class MemberInformation(in string type, in object? value, in MemberInformation.Initializer initializer)
	//{
	//	public class Initializer(in bool mutable = true)
	//	{
	//		public readonly bool Mutable = mutable;
	//	}
	//	public readonly string Type = type;
	//	private object? _Value = value;
	//	public readonly bool Mutable = initializer.Mutable;
	//	public object? Value
	//	{
	//		get => this._Value;
	//		set
	//		{
	//			if (!this.Mutable) return;
	//			this._Value = value;
	//		}
	//	}
	//}
	//private class TypeInformation(in Dictionary<string, MemberInformation> value, in TypeInformation.Initializer initializer): MemberInformation("Type", value, initializer)
	//{
	//	public new class Initializer(): MemberInformation.Initializer(false)
	//	{

	//	}
	//	public new Dictionary<string, MemberInformation> Value
	//	{
	//		get => base.Value;
	//		set => base.Value = value;
	//	}
	//}
	//private readonly Dictionary<string, MemberInformation> Memory2 = new()
	//{
	//	{ "Type", new TypeInformation(null, new()) }
	//};

	private static string? GlobalFetch(in string address)
	{
		try
		{
			using HttpClient client = new();
			HttpResponseMessage response = client.GetAsync(address).Result;
			response.EnsureSuccessStatusCode();
			return response.Content.ReadAsStringAsync().Result;
		}
		catch (Exception)
		{
			return null;
		}
	}
	private static string? LocalFetch(in string address)
	{
		try
		{
			FileInfo file = new(address);
			if (!file.Extension.Equals(".APL", StringComparison.OrdinalIgnoreCase)) throw new FileNotFoundException($"Only files with the .APL extension can be imported");
			using StreamReader reader = file.OpenText();
			return reader.ReadToEnd();
		}
		catch (Exception)
		{
			return null;
		}
	}
	private static string? Fetch(in string address)
	{
		return LocalFetch(address) ?? GlobalFetch(address);
	}
	private readonly Dictionary<string, Datum> Memory = new()
	{
		{ "Pi", new Datum(PI, new(false)) },
		{ "E", new Datum(E, new(false)) },
	};
	private void Evaluate(in IEnumerable<Node> trees)
	{
		foreach (Node tree in trees)
		{
			tree.Evaluate<ValueNode>(this);
		}
	}
}
