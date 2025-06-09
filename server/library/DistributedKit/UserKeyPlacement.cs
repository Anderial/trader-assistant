using Orleans.Placement;
using Orleans.Runtime.Placement;

namespace DistributedKit;

[Serializable]
[GenerateSerializer]
[Alias("Rant.DistributedKit.UserKeyPlacementStrategy")]
public sealed class UserKeyPlacementStrategy : PlacementStrategy;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class UserKeyPlacementAttribute() : PlacementAttribute(new UserKeyPlacementStrategy());

public class UserKeyPlacementDirector : IPlacementDirector
{
    public Task<SiloAddress> OnAddActivation(
        PlacementStrategy strategy,
        PlacementTarget target,
        IPlacementContext context)
    {
        var silos = context.GetCompatibleSilos(target);

        if (silos.Length == 0)
            return Task.FromResult<SiloAddress>(null);

        var primaryKey = target.GrainIdentity.GetGuidKey();
        var hashCode = primaryKey.GetHashCode();
        var index = Math.Abs(hashCode) % silos.Length;

        return Task.FromResult(silos[index]);
    }
}