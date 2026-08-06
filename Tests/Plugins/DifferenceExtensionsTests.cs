using XrmPluginCore.Enums;
using XrmSync.Model;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Difference;

namespace Tests.Plugins;

public class DifferenceExtensionsTests
{
	[Fact]
	public void WithoutOrphanDeletesKeepsRecreatesAndSuppressesOrphans()
	{
		// Arrange — one delete is paired with a create (a recreate), the other is a plain orphan
		var recreated = CreatePlugin("Recreated");
		var orphan = CreatePlugin("Orphan");

		var diff = new Difference<PluginDefinition>(
			Creates: [
				new(recreated, recreated with { }, []), // recreate — carries the remote it replaces
				new(CreatePlugin("BrandNew"), null, []) // plain create
			],
			Updates: [],
			Deletes: [recreated, orphan]);

		// Act
		var (result, suppressed) = diff.WithoutOrphanDeletes();

		// Assert
		Assert.Equal([recreated], result.Deletes);
		Assert.Equal([orphan], suppressed);
		Assert.Equal(diff.Creates, result.Creates);
	}

	[Fact]
	public void WithoutOrphanDeletesReturnsUnchangedWhenNothingToDelete()
	{
		// Arrange
		var diff = new Difference<PluginDefinition>([], [], []);

		// Act
		var (result, suppressed) = diff.WithoutOrphanDeletes();

		// Assert
		Assert.Same(diff, result);
		Assert.Empty(suppressed);
	}

	[Fact]
	public void WithoutOrphanDeletesKeepsChildrenOfRecreatedParents()
	{
		// Arrange — three child deletes: one is its own recreate, one belongs to a parent being
		// recreated, and one is a plain orphan
		var parent = CreatePlugin("Parent");
		var recreatedParent = CreatePlugin("RecreatedParent");

		var ownRecreate = Reference(CreateStep("OwnRecreate"), parent);
		var childOfRecreatedParent = Reference(CreateStep("ChildOfRecreatedParent"), recreatedParent);
		var orphan = Reference(CreateStep("Orphan"), parent);

		var diff = new Difference<Step, PluginDefinition>(
			Creates: [new(ownRecreate, ownRecreate with { }, [])],
			Updates: [],
			Deletes: [ownRecreate, childOfRecreatedParent, orphan]);

		// Act
		var (result, suppressed) = diff.WithoutOrphanDeletes(new HashSet<Guid> { recreatedParent.Id });

		// Assert
		Assert.Equal([ownRecreate, childOfRecreatedParent], result.Deletes);
		Assert.Equal([orphan], suppressed);
	}

	[Fact]
	public void WithoutOrphanDeletesSuppressesAllChildrenWhenNoParentIsRecreated()
	{
		// Arrange
		var parent = CreatePlugin("Parent");
		var orphan = Reference(CreateStep("Orphan"), parent);

		var diff = new Difference<Step, PluginDefinition>([], [], [orphan]);

		// Act
		var (result, suppressed) = diff.WithoutOrphanDeletes();

		// Assert
		Assert.Empty(result.Deletes);
		Assert.Equal([orphan], suppressed);
	}

	private static ParentReference<Step, PluginDefinition> Reference(Step step, PluginDefinition parent) => new(step, parent);

	private static PluginDefinition CreatePlugin(string name) =>
		new(name) { Id = Guid.NewGuid(), PluginSteps = [] };

	private static Step CreateStep(string name) =>
		new(name)
		{
			Id = Guid.NewGuid(),
			ExecutionStage = ExecutionStage.PreValidation,
			EventOperation = "Create",
			LogicalName = "account",
			Deployment = 0,
			ExecutionMode = 0,
			ExecutionOrder = 1,
			FilteredAttributes = string.Empty,
			UserContext = Guid.Empty,
			AsyncAutoDelete = false,
			PluginImages = []
		};
}
