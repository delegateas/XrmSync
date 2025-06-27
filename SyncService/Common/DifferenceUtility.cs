using DG.XrmPluginSync.Model;

namespace DG.XrmPluginSync.SyncService.Common;

public class Difference<T>
{
    public List<T> Creates { get; set; }
    public List<T> Updates { get; set; }
    public List<T> Deletes { get; set; }
}

public static class DifferenceUtility
{
    public static Difference<T> GetDifference<T>(List<T> list1, List<T> list2, IEqualityComparer<T> comparer)
    {
        var creates = list1
            .Except(list2, comparer)
            .ToList();
        var deletes = list2
            .Except(list1, comparer)
            .ToList();
        var updates = list1
            .Intersect(list2, comparer)
            .ToList();

        return new Difference<T>
        {
            Creates = creates,
            Deletes = deletes,
            Updates = updates
        };
    }

    public static Difference<PluginTypeEntity> GetDifference(List<PluginTypeEntity> list1, List<PluginTypeEntity> list2, IEqualityComparer<PluginTypeEntity> comparer)
    {
        var creates = list1
            .Where(x => !list2.Any(y => x.Name.Equals(y.Name)))
            .ToList();

        var deletes = list2
            .Where(x => !list1.Any(y => x.Name.Equals(y.Name)))
            .ToList();

        var list1Intersection = list1
            .Where(x =>
            {
                var element = list2.FirstOrDefault(y => x.Name.Equals(y.Name));
                return element != null;
            });

        var list2Intersection = list2
            .Where(x => list1.Any(y => x.Name.Equals(y.Name)));
        var updates = list1Intersection
            .Except(list2Intersection, comparer)
            .ToList();

        return new Difference<PluginTypeEntity>
        {
            Creates = creates,
            Deletes = deletes,
            Updates = updates
        };
    }

    public static Difference<PluginStepEntity> GetDifference(List<PluginStepEntity> list1, List<PluginStepEntity> list2, IEqualityComparer<PluginStepEntity> comparer)
    {
        var creates = list1
            .Where(x => !list2.Any(y => x.Name.Equals(y.Name)))
            .ToList();

        var deletes = list2
            .Where(x => !list1.Any(y => x.Name.Equals(y.Name)))
            .ToList();

        var list1Intersection = list1
            .Where(x =>
            {
                var element = list2.FirstOrDefault(y => x.Name.Equals(y.Name));
                return element != null;
            });

        var list2Intersection = list2
            .Where(x => list1.Any(y => x.Name.Equals(y.Name)));
        var updates = list1Intersection
            .Except(list2Intersection, comparer)
            .ToList();

        return new Difference<PluginStepEntity>
        {
            Creates = creates,
            Deletes = deletes,
            Updates = updates
        };
    }

    public static Difference<PluginImageEntity> GetDifference(List<PluginImageEntity> list1, List<PluginImageEntity> list2, IEqualityComparer<PluginImageEntity> comparer)
    {
        var creates = list1
            .Where(x => !list2.Any(y => x.Name == y.Name && x.PluginStepName == y.PluginStepName))
            .ToList();

        var deletes = list2
            .Where(x => !list1.Any(y => x.Name == y.Name && x.PluginStepName == y.PluginStepName))
            .ToList();

        var list1Intersection = list1
            .Where(x =>
            {
                var element = list2.FirstOrDefault(y => y.Name == x.Name && x.PluginStepName == y.PluginStepName);
                return element != null;
            });

        var list2Intersection = list2
            .Where(x => list1.Any(y => x.Name == y.Name && x.PluginStepName == y.PluginStepName));
        var updates = list1Intersection
            .Except(list2Intersection, comparer)
            .ToList();

        return new Difference<PluginImageEntity>
        {
            Creates = creates,
            Deletes = deletes,
            Updates = updates
        };
    }
}
