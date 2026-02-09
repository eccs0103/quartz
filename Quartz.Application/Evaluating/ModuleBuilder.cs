using Quartz.Shared.Helpers;
using Quartz.Domain.Evaluating;

namespace Quartz.Application.Evaluating;

internal class ModuleBuilder(Module module, Scope location)
{
	public ModuleBuilder DeclareClass(string name, string? @base, Action<ClassBuilder> configure)
	{
		Scope scope = name.Equals(RuntimeBuilder.NameWorkspace)
			? RuntimeBuilder.Workspace
			: location.GetSubscope(name);
		
		Class? typeBase = null;
		if (@base != null) module.TryReadClass(@base, out typeBase);

		Class type = new(name, scope, typeBase);
		location.Register(name, type, ~Position.Zero);

		configure.Invoke(new ClassBuilder(type, scope));
		return this;
	}
}
