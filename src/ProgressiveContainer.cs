using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace Leaves_ProgressiveContainer;

// We want to load after PostDBModLoader is complete, so we set our type priority to that, plus 1.
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class ProgressiveContainer(
    DatabaseService databaseService,
    ModHelper modHelper)
    : IOnLoad // Implement the IOnLoad interface so that this mod can do something on server load
{
    private const string CraftPrefix = "LeavesCraft_ProgressiveContainer";
    private const string PeacekeeperId = "5935c25fb3acc3127c3d8cd9";
    private const string AlphaContainerTpl = "544a11ac4bdc2d470e8b456a";
    private const string BetaContainerTpl = "5857a8b324597729ab0a0e7d";

    public Task OnLoad()
    {
        string modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        var config = modHelper.GetJsonDataFromFile<ModConfig>(modPath, "config/config.jsonc");

        // ids.jsonc is runtime-owned and excluded from the build copy (see the .csproj), so a
        // fresh install won't have it yet - IdStore starts empty and GetOrCreate mints the craft
        // ids, which get saved back below. Same handling as Leaves-ShinyAirdropGuns.
        var ids = new Leaves.IdStore(modPath, "config/ids.jsonc");

        var containers = new Dictionary<string, MongoId>
        {
            { "pouch", "5732ee6a24597719ae0c0281" },
            { "alpha", "544a11ac4bdc2d470e8b456a" },
            { "beta", "5857a8b324597729ab0a0e7d" },
            { "epsilon", "59db794186f77448bc595262" },
            { "gamma", "5857a8bc2459772bad15db29" },
            { "kappa", "5c093ca986f7740a1867ab12" },
            { "theta", "664a55d84a90fc2c8a6305c9" },
            { "unheard", "5c093ca986f7740a1867ab12" }
        };

        Leaves.Utils.Instance.printColor("[ProgressiveContainer] ProgressiveContainer Starting:");

        foreach (var (containerName, containerTpl) in containers)
        {
            CreateCraft(containerName, containerTpl, config, ids);
        }

        if (config.RemoveContainersFromPeacekeeper)
        {
            RemoveContainerFromPeacekeeper();
        }

        ids.Save();

        return Task.CompletedTask;
    }

    private void CreateCraft(string containerName, MongoId containerTpl, ModConfig config, Leaves.IdStore ids)
    {
        if (!config.SecureContainers.TryGetValue(containerName, out var containerConfig))
        {
            return;
        }

        Leaves.Utils.Instance.printColor($"[ProgressiveContainer] \tAdding craft for: {containerName}", LogTextColor.Cyan);

        Leaves.Utils.Instance.addCraft(
            ids.GetOrCreate(CraftPrefix + containerName),
            GetAreaType(containerName, containerConfig),
            containerConfig.Requirements,
            config.CraftingTime,
            containerTpl);
    }

    private HideoutAreas GetAreaType(string containerName, ContainerConfig containerConfig)
    {
        foreach (var requirement in containerConfig.Requirements)
        {
            if (requirement.AreaType != null)
            {
                return (HideoutAreas)requirement.AreaType.Value;
            }
        }

        Leaves.Utils.Instance.printColor($"[ProgressiveContainer] ERROR: Could not find areaType for: {containerName}", LogTextColor.Red);
        return HideoutAreas.Workbench;
    }

    private void RemoveContainerFromPeacekeeper()
    {
        var assort = databaseService.GetTraders()[PeacekeeperId].Assort.Items;
        int sizeBefore = assort.Count;

        for (int i = assort.Count - 1; i >= 0; i--)
        {
            if (assort[i].Template == AlphaContainerTpl || assort[i].Template == BetaContainerTpl)
            {
                assort.RemoveAt(i);
            }
        }

        Leaves.Utils.Instance.printColor($"[ProgressiveContainer] Size before: {sizeBefore}");
        Leaves.Utils.Instance.printColor($"[ProgressiveContainer] Size after: {assort.Count}");
    }

}
