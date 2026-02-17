using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Application.Evaluating;

internal class ModuleBuilder(Module module, Scope location)
{
	public void DeclareClass(string name, string? @base, IEnumerable<string> generics, Action<ClassBuilder, Class[]> configure)
	{
		if (generics.Any())
		{
			Template template = new(name, generics, (type, args, scope) =>
			{
				configure.Invoke(new ClassBuilder(type, scope), [.. args]);
			}, location);
			if (!location.TryRegister(name, new Value<Template>(TypeConstants.Template, template))) throw new AlreadyExistsIssue($"Template '{name}' in {location}", ~Position.Zero);
			return;
		}

		Scope scope = name.Equals(RuntimeBuilder.NameWorkspace)
			? RuntimeBuilder.Workspace
			: location.GetSubscope(name);
		Class? typeBase = null;
		if (@base != null) module.TryReadClass(@base, out typeBase);
		Class type = new(name, scope, typeBase);
		if (!location.TryRegister(name, new Value<Class>(TypeConstants.Type, type))) throw new AlreadyExistsIssue($"Class '{name}' in {location}", ~Position.Zero);
		configure.Invoke(new ClassBuilder(type, scope), []);
	}
}
