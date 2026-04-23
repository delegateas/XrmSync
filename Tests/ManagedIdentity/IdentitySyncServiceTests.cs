using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.SyncService;

namespace Tests.ManagedIdentity;

public class IdentitySyncServiceTests
{
	private readonly ISolutionReader solutionReader = Substitute.For<ISolutionReader>();
	private readonly IManagedIdentityReader managedIdentityReader = Substitute.For<IManagedIdentityReader>();
	private readonly IManagedIdentityWriter managedIdentityWriter = Substitute.For<IManagedIdentityWriter>();
	private readonly ILogger<IdentitySyncService> logger = Substitute.For<ILogger<IdentitySyncService>>();

	private readonly Guid solutionId = Guid.NewGuid();
	private const string SolutionName = "TestSolution";
	private const string AssemblyPath = "path/to/MyPlugin.dll";

	private IdentitySyncService CreateService(
		IdentityOperation operation = IdentityOperation.Remove,
		string? assemblyPath = null, string? solutionName = null,
		string? clientId = null, string? tenantId = null)
	{
		var options = new IdentityCommandOptions(
			operation,
			assemblyPath ?? AssemblyPath,
			solutionName ?? SolutionName,
			clientId ?? string.Empty,
			tenantId ?? string.Empty);

		return new IdentitySyncService(
			solutionReader,
			managedIdentityReader,
			managedIdentityWriter,
			Options.Create(options),
			logger);
	}

	// --- Remove operation tests ---

	[Fact]
	public async Task RemoveDeletesManagedIdentityWhenLinkedToAssembly()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var miId = Guid.NewGuid();
		var miRef = new EntityReference("managedidentity", miId);

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, miRef));

		var service = CreateService(IdentityOperation.Remove);

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityWriter.Received(1).Remove(miId);
	}

	[Fact]
	public async Task RemoveDoesNotDeleteWhenNoManagedIdentityLinked()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, (EntityReference?)null));

		var service = CreateService(IdentityOperation.Remove);

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityWriter.DidNotReceive().Remove(Arg.Any<Guid>());
	}

	[Fact]
	public async Task RemoveThrowsWhenAssemblyNotFound()
	{
		// Arrange
		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns(((Guid, EntityReference?)?)null);

		var service = CreateService(IdentityOperation.Remove);

		// Act & Assert
		var exception = await Assert.ThrowsAsync<XrmSyncException>(
			() => service.Sync(CancellationToken.None));
		Assert.Contains("MyPlugin", exception.Message);
		Assert.Contains("not found", exception.Message);
	}

	[Fact]
	public async Task RemoveDerivesAssemblyNameFromPath()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "Custom.Plugin.Assembly")
			.Returns((assemblyId, (EntityReference?)null));

		var service = CreateService(IdentityOperation.Remove, assemblyPath: "some/nested/path/Custom.Plugin.Assembly.dll");

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityReader.Received(1).GetPluginAssemblyManagedIdentity(solutionId, "Custom.Plugin.Assembly");
	}

	// --- Ensure operation tests ---

	[Fact]
	public async Task EnsureCreatesManagedIdentityAndLinksToAssembly()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var createdMiId = Guid.NewGuid();
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, (EntityReference?)null));
		managedIdentityWriter.Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
			.Returns(createdMiId);

		var service = CreateService(IdentityOperation.Ensure, clientId: clientId.ToString(), tenantId: tenantId.ToString());

		// Act
		await service.Sync(CancellationToken.None);

		// Assert - MI was created with correct values
		managedIdentityWriter.Received(1).Create("TestSolution Managed Identity", clientId, tenantId);

		// Assert - Assembly was linked to the new MI
		managedIdentityWriter.Received(1).LinkToAssembly(assemblyId, createdMiId);
	}

	[Fact]
	public async Task EnsureNoOpsWhenManagedIdentityAlreadyLinked()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var existingMiRef = new EntityReference("managedidentity", Guid.NewGuid());

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, existingMiRef));

		var service = CreateService(IdentityOperation.Ensure,
			clientId: Guid.NewGuid().ToString(), tenantId: Guid.NewGuid().ToString());

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityWriter.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>());
		managedIdentityWriter.DidNotReceive().LinkToAssembly(Arg.Any<Guid>(), Arg.Any<Guid>());
	}

	[Fact]
	public async Task EnsureThrowsWhenClientIdIsNull()
	{
		// Arrange
		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		var service = CreateService(IdentityOperation.Ensure, clientId: null, tenantId: Guid.NewGuid().ToString());

		// Act & Assert
		var exception = await Assert.ThrowsAsync<XrmSyncException>(() => service.Sync(CancellationToken.None));
		Assert.Equal("Client ID is required and cannot be empty.", exception.Message);
	}

	[Fact]
	public async Task EnsureThrowsWhenClientIdIsNotAValidGuid()
	{
		// Arrange
		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		var service = CreateService(IdentityOperation.Ensure, clientId: "not-a-guid", tenantId: Guid.NewGuid().ToString());

		// Act & Assert
		var exception = await Assert.ThrowsAsync<XrmSyncException>(() => service.Sync(CancellationToken.None));
		Assert.Equal("Client ID must be a valid GUID.", exception.Message);
	}

	[Fact]
	public async Task EnsureThrowsWhenTenantIdIsNull()
	{
		// Arrange
		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		var service = CreateService(IdentityOperation.Ensure, clientId: Guid.NewGuid().ToString(), tenantId: null);

		// Act & Assert
		var exception = await Assert.ThrowsAsync<XrmSyncException>(() => service.Sync(CancellationToken.None));
		Assert.Equal("Tenant ID is required and cannot be empty.", exception.Message);
	}

	[Fact]
	public async Task EnsureThrowsWhenTenantIdIsNotAValidGuid()
	{
		// Arrange
		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		var service = CreateService(IdentityOperation.Ensure, clientId: Guid.NewGuid().ToString(), tenantId: "not-a-guid");

		// Act & Assert
		var exception = await Assert.ThrowsAsync<XrmSyncException>(() => service.Sync(CancellationToken.None));
		Assert.Equal("Tenant ID must be a valid GUID.", exception.Message);
	}

	[Fact]
	public async Task EnsureThrowsWhenAssemblyNotFound()
	{
		// Arrange
		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns(((Guid, EntityReference?)?)null);

		var service = CreateService(IdentityOperation.Ensure,
			clientId: Guid.NewGuid().ToString(), tenantId: Guid.NewGuid().ToString());

		// Act & Assert
		var exception = await Assert.ThrowsAsync<XrmSyncException>(
			() => service.Sync(CancellationToken.None));
		Assert.Contains("MyPlugin", exception.Message);
		Assert.Contains("not found", exception.Message);
	}

	[Fact]
	public async Task EnsureUsesSolutionNameForManagedIdentityName()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();

		solutionReader.RetrieveSolution("CustomSolution").Returns((solutionId, "custom"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, (EntityReference?)null));
		managedIdentityWriter.Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
			.Returns(Guid.NewGuid());

		var service = CreateService(IdentityOperation.Ensure, solutionName: "CustomSolution",
			clientId: Guid.NewGuid().ToString(), tenantId: Guid.NewGuid().ToString());

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityWriter.Received(1).Create(
			"CustomSolution Managed Identity", Arg.Any<Guid>(), Arg.Any<Guid>());
	}

	[Fact]
	public async Task EnsureDerivesAssemblyNameFromPath()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "Custom.Plugin.Assembly")
			.Returns((assemblyId, (EntityReference?)null));
		managedIdentityWriter.Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
			.Returns(Guid.NewGuid());

		var service = CreateService(IdentityOperation.Ensure, assemblyPath: "some/nested/path/Custom.Plugin.Assembly.dll",
			clientId: Guid.NewGuid().ToString(), tenantId: Guid.NewGuid().ToString());

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityReader.Received(1).GetPluginAssemblyManagedIdentity(solutionId, "Custom.Plugin.Assembly");
	}
}
