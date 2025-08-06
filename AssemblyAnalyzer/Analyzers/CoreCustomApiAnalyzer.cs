using DG.XrmPluginCore;
using DG.XrmPluginCore.Interfaces.CustomApi;
using System.Collections;
using System.Linq.Expressions;
using XrmSync.Model.CustomApi;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

internal class CoreCustomApiAnalyzer : CoreAnalyzer, ICustomApiAnalyzer
{
    public List<CustomApiDefinition> GetCustomApis(IEnumerable<Type> types)
    {
        var customApiBaseType = types.FirstOrDefault(t => t.FullName == typeof(ICustomApiDefinition).FullName);

        var validTypes = types
            .Where(t => t.IsAssignableTo(customApiBaseType) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

        return [.. validTypes.Select(GetCustomApiDefinition)];
    }

    private static CustomApiDefinition GetCustomApiDefinition(Type customApiType)
    {
        var customApi = Activator.CreateInstance(customApiType) ?? throw new AnalysisException($"Failed to create instance of type {customApiType.FullName}");

        // Use reflection to call GetRegistration() method safely
        var getRegistrationMethod = customApiType.GetMethod("GetRegistration")
            ?? throw new AnalysisException($"Type {customApiType.FullName} does not have GetRegistration method");

        var registration = getRegistrationMethod.Invoke(customApi, null)
            ?? throw new AnalysisException($"GetRegistration() returned null for type {customApiType.FullName}");

        return ConvertRegistrationToCustomApi(registration, customApiType);
    }

    private static CustomApiDefinition ConvertRegistrationToCustomApi(object registration, Type customApiType)
    {
        return new CustomApiDefinition
        {
            PluginTypeName = customApiType.FullName ?? string.Empty,
            Name = GetConfigValue(registration, x => x.Name) ?? string.Empty,
            DisplayName = GetConfigValue(registration, x => x.DisplayName) ?? string.Empty,
            UniqueName = GetConfigValue(registration, x => x.UniqueName) ?? string.Empty,
            BoundEntityLogicalName = GetConfigValue(registration, x => x.BoundEntityLogicalName) ?? string.Empty,
            Description = GetConfigValue(registration, x => x.Description) ?? string.Empty,
            IsFunction = GetConfigValue(registration, x => x.IsFunction),
            EnabledForWorkflow = GetConfigValue(registration, x => x.EnabledForWorkflow),
            BindingType = GetConfigEnum(registration, x => x.BindingType),
            ExecutePrivilegeName = GetConfigValue(registration, x => x.ExecutePrivilegeName) ?? string.Empty,
            AllowedCustomProcessingStepType = GetConfigEnum(registration, x => x.AllowedCustomProcessingStepType),
            OwnerId = ParseGuid(GetConfigValue(registration, x => x.OwnerId)),
            IsCustomizable = GetConfigValue(registration, x => x.IsCustomizable),
            IsPrivate = GetConfigValue(registration, x => x.IsPrivate),
            RequestParameters = [.. ConvertRequestParameters(GetConfigValue<IEnumerable>(registration, x => x.RequestParameters), GetConfigValue(registration, x => x.Name) ?? string.Empty)],
            ResponseProperties = [..ConvertResponseProperties(GetConfigValue<IEnumerable>(registration, x => x.ResponseProperties), GetConfigValue(registration, x => x.Name) ?? string.Empty)]
        };
    }

    private static IEnumerable<RequestParameter> ConvertRequestParameters(IEnumerable? requestParameters, string customApiName)
    {
        if (requestParameters == null)
        {
            return [];
        }
        return requestParameters
            .Cast<object>()
            .Select(param => new RequestParameter
            {
                Name = GetRequestValue(param, x => x.Name) ?? string.Empty,
                DisplayName = GetRequestValue(param, x => x.DisplayName) ?? string.Empty,
                Type = GetRequestEnum(param, x => x.Type),
                LogicalEntityName = GetRequestValue(param, x => x.LogicalEntityName) ?? string.Empty,
                UniqueName = GetRequestValue(param, x => x.UniqueName) ?? string.Empty,
                IsOptional = GetRequestValue(param, x => x.IsOptional),
                IsCustomizable = GetRequestValue(param, x => x.IsCustomizable),
                CustomApiName = customApiName
            });
    }

    private static IEnumerable<ResponseProperty> ConvertResponseProperties(IEnumerable? responseProperties, string customApiName)
    {
        if (responseProperties == null)
        {
            return [];
        }
        return responseProperties
            .Cast<object>()
            .Select(prop => new ResponseProperty
        {
            Name = GetResponseValue(prop, x => x.Name) ?? string.Empty,
            DisplayName = GetResponseValue(prop, x => x.DisplayName) ?? string.Empty,
            Type = GetEnumIntValue<IResponseProperty, DG.XrmPluginCore.Enums.CustomApiParameterType>(prop, x => x.Type),
            LogicalEntityName = GetResponseValue(prop, x => x.LogicalEntityName) ?? string.Empty,
            UniqueName = GetResponseValue(prop, x => x.UniqueName) ?? string.Empty,
            IsCustomizable = GetResponseValue(prop, x => x.IsCustomizable),
            CustomApiName = customApiName
        });
    }

    private static T? GetConfigValue<T>(object obj, Expression<Func<ICustomApiConfig, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);

    private static int GetConfigEnum<TEnum>(object obj, Expression<Func<ICustomApiConfig, TEnum>> propertyExpression) where TEnum : Enum =>
        GetEnumIntValue(obj, propertyExpression);

    private static int GetRequestEnum<TEnum>(object obj, Expression<Func<IRequestParameter, TEnum>> propertyExpression) where TEnum : Enum =>
        GetEnumIntValue(obj, propertyExpression);

    private static T? GetRequestValue<T>(object obj, Expression<Func<IRequestParameter, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);

    private static T? GetResponseValue<T>(object obj, Expression<Func<IResponseProperty, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);
}
