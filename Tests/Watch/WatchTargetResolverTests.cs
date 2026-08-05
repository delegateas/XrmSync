using XrmSync.Model;
using XrmSync.Watch;

namespace Tests.Watch;

public class WatchTargetResolverTests
{
	private static readonly string PluginDirectory = Path.GetFullPath(Path.Combine("bin", "Debug", "net462"));
	private static readonly string AssemblyPath = Path.Combine(PluginDirectory, "MyPlugin.dll");

	private static bool Accepts(WatchTarget target, string path) =>
		target.Accept(new FileChange(path, FileChangeKind.Changed));

	[Fact]
	public void PluginItemWatchesOnlyItsOwnAssemblyInItsOwnDirectory()
	{
		// Arrange
		var profile = new ProfileConfiguration("dev", "MySolution", [], AssemblyPath);
		var item = new PluginSyncItem { Watch = true };

		// Act
		var target = WatchTargetResolver.TryCreate(item, profile);

		// Assert
		Assert.NotNull(target);
		Assert.Equal(PluginDirectory, target.Scope.Directory);
		Assert.Equal("MyPlugin.dll", target.Scope.Filter);
		Assert.False(target.Scope.IncludeSubdirectories);
		Assert.Equal(AssemblyPath, target.ReadinessPath);

		Assert.True(Accepts(target, AssemblyPath));
		Assert.True(Accepts(target, Path.Combine(PluginDirectory, "MYPLUGIN.DLL")));
		Assert.False(Accepts(target, Path.Combine(PluginDirectory, "MyPlugin.pdb")));
		Assert.False(Accepts(target, Path.Combine(PluginDirectory, "OtherPlugin.dll")));
	}

	[Fact]
	public void WebresourceItemWatchesTheFolderRecursivelyForSupportedFileTypes()
	{
		// Arrange
		var profile = new ProfileConfiguration("dev", "MySolution", []);
		var item = new WebresourceSyncItem("wwwroot") { Watch = true };

		// Act
		var target = WatchTargetResolver.TryCreate(item, profile);

		// Assert
		Assert.NotNull(target);
		Assert.Equal(Path.GetFullPath("wwwroot"), target.Scope.Directory);
		Assert.Equal("*.*", target.Scope.Filter);
		Assert.True(target.Scope.IncludeSubdirectories);
		Assert.Null(target.ReadinessPath);

		Assert.True(Accepts(target, Path.Combine("wwwroot", "js", "app.js")));
		Assert.True(Accepts(target, Path.Combine("wwwroot", "img", "logo.png")));
		Assert.False(Accepts(target, Path.Combine("wwwroot", "notes.txt")));
	}

	[Fact]
	public void WebresourceItemWithAnExtensionFilterIgnoresOtherSupportedTypes()
	{
		// Arrange
		var item = new WebresourceSyncItem("wwwroot", ["js"]) { Watch = true };

		// Act
		var target = WatchTargetResolver.ForFolder(item.FolderPath, item.FileExtensions, item);

		// Assert
		Assert.NotNull(target);
		Assert.True(Accepts(target, Path.Combine("wwwroot", "app.js")));
		Assert.False(Accepts(target, Path.Combine("wwwroot", "site.css")));
	}

	[Theory]
	[MemberData(nameof(NonWatchableItems))]
	public void NonWatchableItemTypesYieldNoTarget(SyncItem item)
	{
		// Arrange
		var profile = new ProfileConfiguration("dev", "MySolution", [], AssemblyPath);

		// Act & Assert
		Assert.Null(WatchTargetResolver.TryCreate(item, profile));
	}

	public static TheoryData<SyncItem> NonWatchableItems => new()
	{
		new PluginAnalysisSyncItem(AssemblyPath, "new", false) { Watch = true },
		new IdentitySyncItem(IdentityOperation.Ensure, AssemblyPath) { Watch = true }
	};

	[Fact]
	public void MissingPathsYieldNoTarget()
	{
		// Arrange
		var profileWithoutAssembly = new ProfileConfiguration("dev", "MySolution", []);

		// Act & Assert
		Assert.Null(WatchTargetResolver.TryCreate(new PluginSyncItem { Watch = true }, profileWithoutAssembly));
		Assert.Null(WatchTargetResolver.TryCreate(new WebresourceSyncItem("  ") { Watch = true }, profileWithoutAssembly));
		Assert.Null(WatchTargetResolver.ForAssembly(null, PluginSyncItem.Empty));
		Assert.Null(WatchTargetResolver.ForFolder(null, null, WebresourceSyncItem.Empty));
	}
}
