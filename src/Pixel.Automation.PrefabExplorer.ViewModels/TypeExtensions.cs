using System.Reflection;

namespace Pixel.Automation.PrefabExplorer.ViewModels;

public static class TypeExtensions
{
    private static PropertyInfo[] GetEligibleProperties(Type type)
    {
        return type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
                p.GetMethod?.IsPublic == true &&
                p.SetMethod?.IsPublic == true &&
                !p.GetMethod.IsStatic &&
                !p.SetMethod.IsStatic)
            .ToArray();
    }

    public static void EnsureCompatible(Type sourceType, Type targetType)
    {
        var sourceProperties = GetEligibleProperties(sourceType)
            .ToDictionary(p => p.Name);

        foreach (var targetProperty in GetEligibleProperties(targetType))
        {
            if (!sourceProperties.TryGetValue(
                    targetProperty.Name, out var sourceProperty))
            {
                throw new InvalidOperationException(
                    $"Source type '{sourceType.Name}' is missing property " +
                    $"'{targetProperty.Name}' required by '{targetType.Name}'.");
            }

            if (sourceProperty.PropertyType != targetProperty.PropertyType)
            {
                throw new InvalidOperationException(
                    $"Property '{targetProperty.Name}' has incompatible types. " +
                    $"Source: '{sourceProperty.PropertyType.Name}', " +
                    $"Target: '{targetProperty.PropertyType.Name}'.");
            }
        }
    }

    public static void CopyProperties<TA, TB>(TA source, TB target)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (target == null)
            throw new ArgumentNullException(nameof(target));

        Type sourceType = source.GetType();
        Type targetType = target.GetType();

        EnsureCompatible(sourceType, targetType);

        var sourceProperties = GetEligibleProperties(sourceType)
            .ToDictionary(p => p.Name);

        foreach (var targetProperty in GetEligibleProperties(targetType))
        {
            var value = sourceProperties[targetProperty.Name]
                .GetValue(source);

            targetProperty.SetValue(target, value);
        }
    }
}
