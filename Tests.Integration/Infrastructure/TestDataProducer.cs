using Microsoft.Xrm.Sdk;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Producer pattern for creating test data in XrmMockup.
/// Provides methods to create common entities with sensible defaults.
/// </summary>
public class TestDataProducer
{
	private readonly IOrganizationService service;

	public TestDataProducer(IOrganizationService service)
	{
		this.service = service;
	}

	/// <summary>
	/// Creates a test solution with publisher.
	/// </summary>
	public (Guid solutionId, string prefix) ProduceSolution(string uniqueName = "TestSolution", string prefix = "test")
	{
		var publisher = new Entity("publisher")
		{
			["uniquename"] = prefix,
			["friendlyname"] = "Test Publisher",
			["customizationprefix"] = prefix
		};
		publisher.Id = service.Create(publisher);

		var solution = new Entity("solution")
		{
			["uniquename"] = uniqueName,
			["friendlyname"] = "Test Solution",
			["publisherid"] = publisher.ToEntityReference(),
			["version"] = "1.0.0.0"
		};
		solution.Id = service.Create(solution);

		return (solution.Id, prefix);
	}

	/// <summary>
	/// Creates a plugin assembly.
	/// </summary>
	public Guid ProducePluginAssembly(string name, string version = "1.0.0.0", string? hash = null)
	{
		var assembly = new Entity("pluginassembly")
		{
			["name"] = name,
			["version"] = version,
			["sourcehash"] = hash ?? Guid.NewGuid().ToString(),
			["isolationmode"] = new OptionSetValue(2), // Sandbox
			["sourcetype"] = new OptionSetValue(0),    // Database
			["culture"] = "neutral",
			["publickeytoken"] = "null"
		};
		return service.Create(assembly);
	}

	/// <summary>
	/// Creates a plugin type (class) for an assembly.
	/// </summary>
	public Guid ProducePluginType(Guid assemblyId, string typeName)
	{
		var pluginType = new Entity("plugintype")
		{
			["pluginassemblyid"] = new EntityReference("pluginassembly", assemblyId),
			["typename"] = typeName,
			["friendlyname"] = typeName,
			["name"] = typeName
		};
		return service.Create(pluginType);
	}

	/// <summary>
	/// Creates an SDK message (e.g., Create, Update, Delete).
	/// </summary>
	public Guid ProduceSdkMessage(string name)
	{
		var message = new Entity("sdkmessage")
		{
			["name"] = name
		};
		return service.Create(message);
	}

	/// <summary>
	/// Creates an SDK message filter linking a message to an entity.
	/// </summary>
	public Guid ProduceSdkMessageFilter(Guid messageId, string primaryEntityName)
	{
		var filter = new Entity("sdkmessagefilter")
		{
			["sdkmessageid"] = new EntityReference("sdkmessage", messageId),
			["primaryobjecttypecode"] = primaryEntityName
		};
		return service.Create(filter);
	}

	/// <summary>
	/// Creates a plugin step.
	/// </summary>
	public Guid ProducePluginStep(
		Guid pluginTypeId,
		Guid messageId,
		Guid? messageFilterId,
		string name,
		int stage = 20,          // Pre-operation
		int mode = 0,            // Synchronous
		int rank = 1,
		string? filteringAttributes = null)
	{
		var step = new Entity("sdkmessageprocessingstep")
		{
			["plugintypeid"] = new EntityReference("plugintype", pluginTypeId),
			["sdkmessageid"] = new EntityReference("sdkmessage", messageId),
			["name"] = name,
			["stage"] = new OptionSetValue(stage),
			["mode"] = new OptionSetValue(mode),
			["rank"] = rank,
			["supporteddeployment"] = new OptionSetValue(0) // Server only
		};

		if (messageFilterId.HasValue)
		{
			step["sdkmessagefilterid"] = new EntityReference("sdkmessagefilter", messageFilterId.Value);
		}

		if (!string.IsNullOrEmpty(filteringAttributes))
		{
			step["filteringattributes"] = filteringAttributes;
		}

		return service.Create(step);
	}

	/// <summary>
	/// Creates a plugin step image (pre or post).
	/// </summary>
	public Guid ProducePluginStepImage(
		Guid stepId,
		string name,
		int imageType,           // 0 = PreImage, 1 = PostImage, 2 = Both
		string? attributes = null,
		string entityAlias = "Image")
	{
		var image = new Entity("sdkmessageprocessingstepimage")
		{
			["sdkmessageprocessingstepid"] = new EntityReference("sdkmessageprocessingstep", stepId),
			["name"] = name,
			["entityalias"] = entityAlias,
			["imagetype"] = new OptionSetValue(imageType),
			["messagepropertyname"] = "Target"
		};

		if (!string.IsNullOrEmpty(attributes))
		{
			image["attributes"] = attributes;
		}

		return service.Create(image);
	}

	/// <summary>
	/// Creates a webresource.
	/// </summary>
	public Guid ProduceWebresource(
		string name,
		string displayName,
		int webresourceType,     // 1=HTML, 2=CSS, 3=JS, etc.
		string? content = null)
	{
		var webresource = new Entity("webresource")
		{
			["name"] = name,
			["displayname"] = displayName,
			["webresourcetype"] = new OptionSetValue(webresourceType)
		};

		if (!string.IsNullOrEmpty(content))
		{
			webresource["content"] = content;
		}

		return service.Create(webresource);
	}

	/// <summary>
	/// Creates a custom API.
	/// </summary>
	public Guid ProduceCustomApi(
		string uniqueName,
		string name,
		string displayName,
		Guid? pluginTypeId = null,
		int bindingType = 0,     // 0 = Global, 1 = Entity, 2 = EntityCollection
		string? boundEntityLogicalName = null)
	{
		var customApi = new Entity("customapi")
		{
			["uniquename"] = uniqueName,
			["name"] = name,
			["displayname"] = displayName,
			["bindingtype"] = new OptionSetValue(bindingType),
			["isfunction"] = false,
			["isprivate"] = false,
			["allowedcustomprocessingsteptype"] = new OptionSetValue(0) // None
		};

		if (pluginTypeId.HasValue)
		{
			customApi["plugintypeid"] = new EntityReference("plugintype", pluginTypeId.Value);
		}

		if (!string.IsNullOrEmpty(boundEntityLogicalName))
		{
			customApi["boundentitylogicalname"] = boundEntityLogicalName;
		}

		return service.Create(customApi);
	}

	/// <summary>
	/// Creates a custom API request parameter.
	/// </summary>
	public Guid ProduceCustomApiRequestParameter(
		Guid customApiId,
		string uniqueName,
		string name,
		string displayName,
		int type,                // 0 = Boolean, 1 = DateTime, 2 = Decimal, etc.
		bool isOptional = false)
	{
		var param = new Entity("customapirequestparameter")
		{
			["customapiid"] = new EntityReference("customapi", customApiId),
			["uniquename"] = uniqueName,
			["name"] = name,
			["displayname"] = displayName,
			["type"] = new OptionSetValue(type),
			["isoptional"] = isOptional
		};
		return service.Create(param);
	}

	/// <summary>
	/// Creates a custom API response property.
	/// </summary>
	public Guid ProduceCustomApiResponseProperty(
		Guid customApiId,
		string uniqueName,
		string name,
		string displayName,
		int type)
	{
		var prop = new Entity("customapiresponseproperty")
		{
			["customapiid"] = new EntityReference("customapi", customApiId),
			["uniquename"] = uniqueName,
			["name"] = name,
			["displayname"] = displayName,
			["type"] = new OptionSetValue(type)
		};
		return service.Create(prop);
	}
}
