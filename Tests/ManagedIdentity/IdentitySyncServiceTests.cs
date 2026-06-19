using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Identity;
using XrmSync.SyncService;

namespace Tests.ManagedIdentity;

public class IdentitySyncServiceTests
{
	private readonly ISolutionReader solutionReader = Substitute.For<ISolutionReader>();
	private readonly IManagedIdentityReader managedIdentityReader = Substitute.For<IManagedIdentityReader>();
	private readonly IManagedIdentityReconciler managedIdentityService = Substitute.For<IManagedIdentityReconciler>();
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
			managedIdentityService,
			Options.Create(options),
			logger);
	}

	// --- Remove operation tests ---

	[Fact]
	public async Task RemoveDelegatesToServiceWhenAssemblyFound()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var miRef = new EntityReference("managedidentity", Guid.NewGuid());

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, miRef));

		var service = CreateService(IdentityOperation.Remove);

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityService.Received(1).Remove(miRef, "MyPlugin");
	}

	[Fact]
	public async Task RemoveWarnsAndDoesNotThrowWhenAssemblyNotFound()
	{
		// Arrange
		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns(((Guid, EntityReference?)?)null);

		var service = CreateService(IdentityOperation.Remove);

		// Act — a missing assembly must not block removal
		await service.Sync(CancellationToken.None);

		// Assert — nothing was removed
		managedIdentityService.DidNotReceive().Remove(Arg.Any<EntityReference?>(), Arg.Any<string>());
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
	public async Task EnsureDelegatesToServiceWithResolvedAssemblyAndIdentity()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, (EntityReference?)null));

		var service = CreateService(IdentityOperation.Ensure, clientId: clientId.ToString(), tenantId: tenantId.ToString());

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityService.Received(1).Ensure(assemblyId, null, SolutionName, clientId, tenantId);
	}

	[Fact]
	public async Task EnsurePassesExistingIdentityToService()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var existingMiRef = new EntityReference("managedidentity", Guid.NewGuid());
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, existingMiRef));

		var service = CreateService(IdentityOperation.Ensure, clientId: clientId.ToString(), tenantId: tenantId.ToString());

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityService.Received(1).Ensure(assemblyId, existingMiRef, SolutionName, clientId, tenantId);
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

		// Act & Assert — Ensure needs the assembly to link the identity to
		var exception = await Assert.ThrowsAsync<XrmSyncException>(
			() => service.Sync(CancellationToken.None));
		Assert.Contains("MyPlugin", exception.Message);
		Assert.Contains("not found", exception.Message);
	}

	[Fact]
	public async Task EnsurePassesSolutionNameToService()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();

		solutionReader.RetrieveSolution("CustomSolution").Returns((solutionId, "custom"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "MyPlugin")
			.Returns((assemblyId, (EntityReference?)null));

		var service = CreateService(IdentityOperation.Ensure, solutionName: "CustomSolution",
			clientId: Guid.NewGuid().ToString(), tenantId: Guid.NewGuid().ToString());

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityService.Received(1).Ensure(
			assemblyId, null, "CustomSolution", Arg.Any<Guid>(), Arg.Any<Guid>());
	}

	[Fact]
	public async Task EnsureDerivesAssemblyNameFromPath()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();

		solutionReader.RetrieveSolution(SolutionName).Returns((solutionId, "test"));
		managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, "Custom.Plugin.Assembly")
			.Returns((assemblyId, (EntityReference?)null));

		var service = CreateService(IdentityOperation.Ensure, assemblyPath: "some/nested/path/Custom.Plugin.Assembly.dll",
			clientId: Guid.NewGuid().ToString(), tenantId: Guid.NewGuid().ToString());

		// Act
		await service.Sync(CancellationToken.None);

		// Assert
		managedIdentityReader.Received(1).GetPluginAssemblyManagedIdentity(solutionId, "Custom.Plugin.Assembly");
	}
}
