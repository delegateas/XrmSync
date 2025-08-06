using System.Linq.Expressions;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

internal class CoreAnalyzer : Analyzer
{
    protected static int GetEnumIntValue<TReference, TEnum>(object obj, Expression<Func<TReference, TEnum>> propertyExpression) where TEnum : Enum
    {
        var propertyName = GetPropertyName(propertyExpression);
        return GetEnumIntValue(obj, propertyName);
    }

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

    private static int GetEnumIntValue(object obj, string propertyName)
    {
        return GetPropertyValue<object>(obj, propertyName) switch
        {
            null => default,
            Enum enumValue => Convert.ToInt32(enumValue),
            _ => throw new AnalysisException($"Property '{propertyName}' on type '{obj.GetType().FullName}' is not an enum")
        };
    }

    private static T? GetPropertyValue<T>(object obj, string propertyName)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyName)
            ?? throw new AnalysisException($"Property '{propertyName}' not found on type '{type.FullName}'");
        var value = property.GetValue(obj);

        return value is T typedValue ? typedValue : default;
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