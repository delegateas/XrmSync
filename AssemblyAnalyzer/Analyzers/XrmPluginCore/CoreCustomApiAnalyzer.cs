using XrmPluginCore;
using XrmPluginCore.Interfaces.CustomApi;
using System.Collections;
using System.Linq.Expressions;
using XrmSync.Model.CustomApi;

namespace XrmSync.Analyzer.Analyzers.XrmPluginCore;

internal class CoreCustomApiAnalyzer : CoreAnalyzer, IAnalyzer<CustomApiDefinition>
{
    public List<CustomApiDefinition> AnalyzeTypes(IEnumerable<Type> types, string prefix)
    {
        var customApiBaseType = types.FirstOrDefault(t => t.FullName == typeof(ICustomApiDefinition).FullName);

        var validTypes = types
            .Where(t => t.IsAssignableTo(customApiBaseType) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

        return [.. AnalyzeTypesInner(validTypes, prefix)];
    }

    private static IEnumerable<CustomApiDefinition> AnalyzeTypesInner(IEnumerable<Type> types, string prefix)
    {
        foreach (var customApiType in types)
        {
            var registration = GetRegistrationFromType<object>(nameof(ICustomApiDefinition.GetRegistration), customApiType);
            if (registration is null)
            {
                continue;
            }

            yield return new CustomApiDefinition(GetConfigValue(registration, x => x.Name) ?? string.Empty)
            {
                PluginType = new PluginType(customApiType.FullName ?? string.Empty),
                DisplayName = GetConfigValue(registration, x => x.DisplayName) ?? string.Empty,
                UniqueName = prefix + "_" + (GetConfigValue(registration, x => x.UniqueName) ?? string.Empty),

                BoundEntityLogicalName = GetConfigValue(registration, x => x.BoundEntityLogicalName) ?? string.Empty,
                Description = GetConfigValue(registration, x => x.Description) ?? string.Empty,
                IsFunction = GetConfigValue(registration, x => x.IsFunction),
                EnabledForWorkflow = GetConfigValue(registration, x => x.EnabledForWorkflow),
                BindingType = GetConfigValue(registration, x => x.BindingType),
                ExecutePrivilegeName = GetConfigValue(registration, x => x.ExecutePrivilegeName) ?? string.Empty,
                AllowedCustomProcessingStepType = GetConfigValue(registration, x => x.AllowedCustomProcessingStepType),
                OwnerId = GetConfigValue(registration, x => x.OwnerId) ?? Guid.Empty,
                IsCustomizable = GetConfigValue(registration, x => x.IsCustomizable),
                IsPrivate = GetConfigValue(registration, x => x.IsPrivate),

                RequestParameters = [.. ConvertRequestParameters(GetConfigValue<IEnumerable>(registration, x => x.RequestParameters))],
                ResponseProperties = [.. ConvertResponseProperties(GetConfigValue<IEnumerable>(registration, x => x.ResponseProperties))]
            };
        }
    }

    private static IEnumerable<RequestParameter> ConvertRequestParameters(IEnumerable? requestParameters)
    {
        if (requestParameters == null)
        {
            return [];
        }
        return requestParameters
            .Cast<object>()
            .Select(param => new RequestParameter(GetRequestValue(param, x => x.Name) ?? string.Empty)
            {
                DisplayName = GetRequestValue(param, x => x.DisplayName) ?? string.Empty,
                UniqueName = GetRequestValue(param, x => x.UniqueName) ?? string.Empty,
                Type = GetRequestValue(param, x => x.Type),
                LogicalEntityName = GetRequestValue(param, x => x.LogicalEntityName) ?? string.Empty,
                IsOptional = GetRequestValue(param, x => x.IsOptional),
                IsCustomizable = GetRequestValue(param, x => x.IsCustomizable)
            });
    }

    private static IEnumerable<ResponseProperty> ConvertResponseProperties(IEnumerable? responseProperties)
    {
        if (responseProperties == null)
        {
            return [];
        }
        return responseProperties
            .Cast<object>()
            .Select(prop => new ResponseProperty(GetResponseValue(prop, x => x.Name) ?? string.Empty)
            {
                DisplayName = GetResponseValue(prop, x => x.DisplayName) ?? string.Empty,
                UniqueName = GetResponseValue(prop, x => x.UniqueName) ?? string.Empty,
                Type = GetResponseValue(prop, x => x.Type),
                LogicalEntityName = GetResponseValue(prop, x => x.LogicalEntityName) ?? string.Empty,
                IsCustomizable = GetResponseValue(prop, x => x.IsCustomizable)
            });
    }

    private static T? GetConfigValue<T>(object obj, Expression<Func<ICustomApiConfig, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);

    private static T? GetRequestValue<T>(object obj, Expression<Func<IRequestParameter, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);

    private static T? GetResponseValue<T>(object obj, Expression<Func<IResponseProperty, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);
}
