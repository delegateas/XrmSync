using DG.XrmPluginSync.Model.Plugin;
using DG.XrmPluginSync.SyncService.Comparers;
using DG.XrmPluginSync.SyncService.Differences;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests;

public class DifferenceUtilityTests
{
    private readonly IServiceProvider _provider;

    public DifferenceUtilityTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEqualityComparer<PluginType>, PluginTypeComparer>();
        services.AddSingleton<IEqualityComparer<Step>, PluginStepComparer>();
        services.AddSingleton<IEqualityComparer<Image>, PluginImageComparer>();
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public void GetDifference_Generic_ReturnsCorrectCreatesUpdatesDeletes()
    {
        var list1 = new List<string> { "A", "B", "C" };
        var list2 = new List<string> { "B", "C", "D" };
        var comparer = StringComparer.Ordinal;

        var diff = DifferenceUtility.GetDifference(list1, list2, comparer);

        Assert.Equal(["A"], diff.Creates);
        Assert.Equal(["D"], diff.Deletes);
        Assert.Equal(["B", "C"], diff.Updates);
    }

    [Fact]
    public void GetDifference_PluginTypeEntity_ReturnsCorrectCreatesDeletesUpdates()
    {
        var type1 = new PluginDefinition { Name = "Type1", PluginSteps = [], Id = Guid.NewGuid() };
        var type2 = new PluginDefinition { Name = "Type2", PluginSteps = [], Id = Guid.NewGuid() };
        var type3 = new PluginDefinition { Name = "Type3", PluginSteps = [], Id = Guid.NewGuid() };
        var type3b = new PluginDefinition { Name = "Type3", PluginSteps = [], Id = Guid.NewGuid() };
        var list1 = new List<PluginDefinition> { type1, type3 };
        var list2 = new List<PluginDefinition> { type2, type3b };
        var comparer = _provider.GetRequiredService<IEqualityComparer<PluginDefinition>>();

        var diff = DifferenceUtility.GetDifference(list1, list2, comparer);

        Assert.Single(diff.Creates);
        Assert.Equal("Type1", diff.Creates[0].Name);
        Assert.Single(diff.Deletes);
        Assert.Equal("Type2", diff.Deletes[0].Name);
        Assert.Empty(diff.Updates); // No updates because type3 and type3b are equal by comparer
    }

    [Fact]
    public void GetDifference_PluginStepEntity_ReturnsCorrectCreatesDeletesUpdates()
    {
        var step1 = new Step {
            Name = "Step1", PluginTypeName = "Type1", ExecutionStage = 1, EventOperation = "Create", LogicalName = "entity", Deployment = 0, ExecutionMode = 0, ExecutionOrder = 1, FilteredAttributes = "", UserContext = Guid.NewGuid(), PluginImages = []
        };
        var step2 = new Step {
            Name = "Step2", PluginTypeName = "Type1", ExecutionStage = 1, EventOperation = "Create", LogicalName = "entity", Deployment = 0, ExecutionMode = 0, ExecutionOrder = 1, FilteredAttributes = "", UserContext = Guid.NewGuid(), PluginImages = []
        };
        var step3 = new Step {
            Name = "Step3", PluginTypeName = "Type1", ExecutionStage = 1, EventOperation = "Create", LogicalName = "entity", Deployment = 0, ExecutionMode = 0, ExecutionOrder = 1, FilteredAttributes = "", UserContext = Guid.NewGuid(), PluginImages = []
        };
        var step3b = new Step {
            Name = "Step3", PluginTypeName = "Type1", ExecutionStage = 2, EventOperation = "Update", LogicalName = "entity", Deployment = 0, ExecutionMode = 0, ExecutionOrder = 1, FilteredAttributes = "", UserContext = Guid.NewGuid(), PluginImages = []
        };
        var list1 = new List<Step> { step1, step3 };
        var list2 = new List<Step> { step2, step3b };
        var comparer = _provider.GetRequiredService<IEqualityComparer<Step>>();

        var diff = DifferenceUtility.GetDifference(list1, list2, comparer);

        Assert.Single(diff.Creates);
        Assert.Equal("Step1", diff.Creates[0].Name);
        Assert.Single(diff.Deletes);
        Assert.Equal("Step2", diff.Deletes[0].Name);
        Assert.Single(diff.Updates); // step3 and step3b have same name but differ by comparer
        Assert.Equal("Step3", diff.Updates[0].Name);
    }

    [Fact]
    public void GetDifference_PluginImageEntity_ReturnsCorrectCreatesDeletesUpdates()
    {
        var img1 = new Image { Name = "Img1", PluginStepName = "Step1", EntityAlias = "alias", ImageType = 0, Attributes = "attr1" };
        var img2 = new Image { Name = "Img2", PluginStepName = "Step1", EntityAlias = "alias", ImageType = 0, Attributes = "attr2" };
        var img3 = new Image { Name = "Img3", PluginStepName = "Step1", EntityAlias = "alias", ImageType = 0, Attributes = "attr3" };
        var img3b = new Image { Name = "Img3", PluginStepName = "Step1", EntityAlias = "alias2", ImageType = 1, Attributes = "attr4" };
        var list1 = new List<Image> { img1, img3 };
        var list2 = new List<Image> { img2, img3b };
        var comparer = _provider.GetRequiredService<IEqualityComparer<Image>>();

        var diff = DifferenceUtility.GetDifference(list1, list2, comparer);

        Assert.Single(diff.Creates);
        Assert.Equal("Img1", diff.Creates[0].Name);
        Assert.Single(diff.Deletes);
        Assert.Equal("Img2", diff.Deletes[0].Name);
        Assert.Single(diff.Updates); // img3 and img3b have same name+step but differ by comparer
        Assert.Equal("Img3", diff.Updates[0].Name);
    }

    [Fact]
    public void GetDifference_EmptyLists_ReturnsEmptyDifference()
    {
        var comparer = StringComparer.Ordinal;
        var diff = DifferenceUtility.GetDifference([], [], comparer);
        Assert.Empty(diff.Creates);
        Assert.Empty(diff.Deletes);
        Assert.Empty(diff.Updates);
    }
}
