using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Template(string name, string[] generics, Action<Class, Class[]> builder, Scope location) : Symbol(name)
{
	public Class Instantiate(string name, Class[] arguments)
	{
		Class type = new(name, location.GetSubscope(name));
		if (generics.Length != arguments.Length) throw new Exception("Invalid generic arguments count");

		for (int i = 0; i < generics.Length; i++)
		{
			type.Location.Register(generics[i], arguments[i], ~Position.Zero);
		}

		builder(type, arguments);
		return type;
	}

	public override void Assign(Instance value, Range<Position> range)
	{
		throw new NotImplementedException();
	}
}
