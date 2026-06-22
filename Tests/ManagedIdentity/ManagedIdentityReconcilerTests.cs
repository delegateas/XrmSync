using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Identity;
using XrmSync.SyncService;

namespace Tests.ManagedIdentity;

public class ManagedIdentityReconcilerTests
{
	private readonly IManagedIdentityReader reader = Substitute.For<IManagedIdentityReader>();
	private readonly IManagedIdentityWriter writer = Substitute.For<IManagedIdentityWriter>();
	private readonly ILogger<ManagedIdentityReconciler> logger = Substitute.For<ILogger<ManagedIdentityReconciler>>();

	private const string SolutionName = "TestSolution";
	private const string ExpectedName = "TestSolution Managed Identity";

	private ManagedIdentityReconciler CreateService() => new(reader, writer, logger);

	// --- Ensure: create + link ---

	[Fact]
	public void EnsureCreatesAndLinksWhenNoCurrentIdentity()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();
		var createdId = Guid.NewGuid();
		writer.Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(createdId);

		// Act
		CreateService().Ensure(assemblyId, null, SolutionName, clientId, tenantId);

		// Assert
		writer.Received(1).Create(ExpectedName, clientId, tenantId);
		writer.Received(1).LinkToAssembly(assemblyId, createdId);
		writer.DidNotReceive().Update(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>());
	}

	// --- Ensure: upsert in place ---

	[Fact]
	public void EnsureUpdatesInPlaceWhenApplicationIdDiffers()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var miId = Guid.NewGuid();
		var current = new EntityReference("managedidentity", miId);
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();

		reader.GetManagedIdentity(miId).Returns(new ManagedIdentityInfo(miId, ExpectedName, Guid.NewGuid(), tenantId));

		// Act
		CreateService().Ensure(assemblyId, current, SolutionName, clientId, tenantId);

		// Assert
		writer.Received(1).Update(miId, ExpectedName, clientId, tenantId);
		writer.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>());
	}

	[Fact]
	public void EnsureUpdatesInPlaceWhenTenantIdDiffers()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var miId = Guid.NewGuid();
		var current = new EntityReference("managedidentity", miId);
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();

		reader.GetManagedIdentity(miId).Returns(new ManagedIdentityInfo(miId, ExpectedName, clientId, Guid.NewGuid()));

		// Act
		CreateService().Ensure(assemblyId, current, SolutionName, clientId, tenantId);

		// Assert
		writer.Received(1).Update(miId, ExpectedName, clientId, tenantId);
	}

	[Fact]
	public void EnsureUpdatesInPlaceWhenNameDiffers()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var miId = Guid.NewGuid();
		var current = new EntityReference("managedidentity", miId);
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();

		reader.GetManagedIdentity(miId).Returns(new ManagedIdentityInfo(miId, "Old Name", clientId, tenantId));

		// Act
		CreateService().Ensure(assemblyId, current, SolutionName, clientId, tenantId);

		// Assert
		writer.Received(1).Update(miId, ExpectedName, clientId, tenantId);
	}

	[Fact]
	public void EnsureNoOpsWhenEverythingMatches()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var miId = Guid.NewGuid();
		var current = new EntityReference("managedidentity", miId);
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();

		reader.GetManagedIdentity(miId).Returns(new ManagedIdentityInfo(miId, ExpectedName, clientId, tenantId));

		// Act
		CreateService().Ensure(assemblyId, current, SolutionName, clientId, tenantId);

		// Assert — nothing changes
		writer.DidNotReceive().Update(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>());
		writer.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>());
		writer.DidNotReceive().LinkToAssembly(Arg.Any<Guid>(), Arg.Any<Guid>());
	}

	[Fact]
	public void EnsureRecreatesWhenLinkedRecordMissing()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var miId = Guid.NewGuid();
		var current = new EntityReference("managedidentity", miId);
		var clientId = Guid.NewGuid();
		var tenantId = Guid.NewGuid();
		var createdId = Guid.NewGuid();

		reader.GetManagedIdentity(miId).Returns((ManagedIdentityInfo?)null);
		writer.Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(createdId);

		// Act
		CreateService().Ensure(assemblyId, current, SolutionName, clientId, tenantId);

		// Assert
		writer.Received(1).Create(ExpectedName, clientId, tenantId);
		writer.Received(1).LinkToAssembly(assemblyId, createdId);
	}

	[Fact]
	public void EnsureUsesSolutionNameForIdentityName()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		writer.Create(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Guid.NewGuid());

		// Act
		CreateService().Ensure(assemblyId, null, "CustomSolution", Guid.NewGuid(), Guid.NewGuid());

		// Assert
		writer.Received(1).Create("CustomSolution Managed Identity", Arg.Any<Guid>(), Arg.Any<Guid>());
	}

	// --- Remove ---

	[Fact]
	public void RemoveDeletesWhenLinked()
	{
		// Arrange
		var miId = Guid.NewGuid();
		var current = new EntityReference("managedidentity", miId);

		// Act
		CreateService().Remove(current, "MyPlugin");

		// Assert
		writer.Received(1).Remove(miId);
	}

	[Fact]
	public void RemoveNoOpsWhenNotLinked()
	{
		// Act
		CreateService().Remove(null, "MyPlugin");

		// Assert
		writer.DidNotReceive().Remove(Arg.Any<Guid>());
	}
}
