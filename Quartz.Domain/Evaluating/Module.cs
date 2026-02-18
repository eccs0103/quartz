using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Module(string name, Scope location) : Container(name, location)
{
	public bool TryRegisterTemplate(Template template)
	{
		return Location.TryRegister(template.Name, TypeConstants.Template, new Value<Template>(TypeConstants.Template, template));
	}

	public bool TryReadTemplate(string name, [NotNullWhen(true)] out Template? template)
	{
		return Location.TryRead(name, out template);
	}

	public bool TryRegisterClass(Class type)
	{
		return Location.TryRegister(type.Name, TypeConstants.Type, new Value<Class>(TypeConstants.Type, type));
	}

	public bool TryReadClass(string name, [NotNullWhen(true)] out Class? type)
	{
		return Location.TryRead(name, out type);
	}
}
