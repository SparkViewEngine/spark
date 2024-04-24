using System;
using System.Linq;
using System.Reflection;
using Spark.FileSystem;

namespace Spark;

public static class ViewFolderSettingsExtensions
{
    public static IViewFolder ActivateViewFolder(this IViewFolderSettings viewFolderSettings)
    {
        var type = Type.GetType(viewFolderSettings.Type);
            
        ConstructorInfo bestConstructor = null;
        foreach (var constructor in type.GetConstructors())
        {
            if (bestConstructor == null || bestConstructor.GetParameters().Length < constructor.GetParameters().Length)
            {
                if (constructor.GetParameters().All(param => viewFolderSettings.Parameters.ContainsKey(param.Name)))
                {
                    bestConstructor = constructor;
                }
            }
        }

        if (bestConstructor == null)
        {
            throw new MissingMethodException($"No suitable constructor for {type.FullName} located");
        }

        var args = bestConstructor.GetParameters()
            .Select(viewFolderSettings.ChangeType)
            .ToArray();

        return (IViewFolder)Activator.CreateInstance(type, args);
    }

    private static object ChangeType(this IViewFolderSettings viewFolderSettings, ParameterInfo param)
    {
        if (param.ParameterType == typeof(Assembly))
        {
            return Assembly.Load(viewFolderSettings.Parameters[param.Name]);
        }

        return Convert.ChangeType(viewFolderSettings.Parameters[param.Name], param.ParameterType);
    }
}