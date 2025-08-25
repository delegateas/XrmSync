using System.Linq.Expressions;

namespace XrmSync.AssemblyAnalyzer.Analyzers.XrmPluginCore;

internal class CoreAnalyzer : Analyzer
{
    /// <summary>
    /// Type-safe property access using nameof() from a reference type to ensure property names match
    /// </summary>
    protected static T? GetPropertyValue<T, TReference>(object obj, Expression<Func<TReference, T>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);
        return GetPropertyValue<T>(obj, propertyName);
    }

    protected static Guid ParseGuid(string? guidString)
    {
        return string.IsNullOrEmpty(guidString) ? Guid.Empty : Guid.Parse(guidString);
    }

    private static T? GetPropertyValue<T>(object obj, string propertyName)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyName)
            ?? throw new AnalysisException($"Property '{propertyName}' not found on type '{type.FullName}'");
        var value = property.GetValue(obj);

        return value switch
        {
            null => default,
            T typedValue => typedValue,
            Enum enumValue => (T)(object)Convert.ToInt32(enumValue),
            _ => throw new AnalysisException($"Property '{propertyName}' on type '{type.FullName}' is not of type '{typeof(T).FullName}'")
        };
    }

    private static string GetPropertyName<TReference, T>(Expression<Func<TReference, T>> propertyExpression)
    {
        return propertyExpression.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression { Operand: MemberExpression unaryMemberExpression } => unaryMemberExpression.Member.Name,
            _ => throw new ArgumentException("Expression must be a property access", nameof(propertyExpression))
        };
    }
}