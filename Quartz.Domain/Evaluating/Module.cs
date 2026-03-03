using System.Diagnostics.CodeAnalysis;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Evaluating;

public class Module(string name, Scope location) : Container(name, location)
{
	public bool TryRegisterTemplate(Template template)
	{
		return Location.TryRegister(template.Name, Types.Template, new Value<Template>(Types.Template, template));
	}

	public bool TryReadTemplate(string name, [NotNullWhen(true)] out Template? template)
	{
		return Location.TryRead(name, out template);
	}

	public bool TryRegisterClass(Class type)
	{
		return Location.TryRegister(type.Name, Types.Type, new Value<Class>(Types.Type, type));
	}

	public bool TryReadClass(string name, [NotNullWhen(true)] out Class? type)
	{
		return Location.TryRead(name, out type);
	}
}
