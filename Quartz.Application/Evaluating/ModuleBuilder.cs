using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Exceptions.Semantic;
using Quartz.Shared.Helpers;
using static Quartz.Shared.Constants;

namespace Quartz.Application.Evaluating;

internal delegate void ClassConfigurator(ClassBuilder type, Class[] generics);

internal class ModuleBuilder(Module module, Scope location)
{
	public void DeclareClass(string name, string? @base, IEnumerable<string> generics, ClassConfigurator configurator)
	{
		if (generics.Any())
		{
			void TemplateConstructor(Class type, IEnumerable<Class> parameters, Scope scope)
			{
				configurator.Invoke(new ClassBuilder(type, scope), [.. parameters]);
			}
			Template template = new(name, generics, TemplateConstructor, location);
			if (!module.TryRegisterTemplate(template)) throw new SymbolAlreadyDeclaredIssue(name, ~Position.Zero);
			return;
		}

		Scope scope = name.Equals(Types.Workspace)
			? RuntimeBuilder.Workspace
			: location.GetSubscope(name);
		Class? typeBase = null;
		if (@base != null) module.TryReadClass(@base, out typeBase);
		Class type = new(name, scope, typeBase);
		if (!module.TryRegisterClass(type)) throw new SymbolAlreadyDeclaredIssue(name, ~Position.Zero);
		configurator.Invoke(new ClassBuilder(type, scope), []);
	}
}
