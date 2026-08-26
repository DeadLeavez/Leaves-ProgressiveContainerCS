# Leaves-ProgressiveContainer — Handoff

Last updated: 2026-07-15. Read `4.0 Mods\HANDOFF.md` first for project-wide rules.

---

## 1. What the mod does

Adds hideout crafts to build progressively larger secure containers (pouch → alpha → beta →
epsilon → gamma → kappa → theta → unheard). Each craft is opt-in per container via config, with
configurable requirements and craft time. Optionally removes the vanilla Alpha/Beta containers
from Peacekeeper so crafting is the only path.

Single class, `src/ProgressiveContainer.cs` (~130 lines) — the simplest gameplay mod here.

## 2. Lifecycle

`[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]`, `IOnLoad` only. One pass:
- Load config + `ids.jsonc`.
- For each container that has a config entry, build a `HideoutProduction` recipe
  (`CreateCraft`) and append it to `GetHideout().Production.Recipes`. The craft's `AreaType`
  is derived from whichever requirement carries an `areaType` (falls back to Workbench with an
  error log if none).
- Optionally strip Alpha/Beta from Peacekeeper's assort (iterate backwards while removing).
- Save `ids.jsonc` back.

## 3. `ids.jsonc` — runtime-owned, stable craft ids

Craft ids are minted once (keyed `LeavesCraft_ProgressiveContainer<name>`) and cached so they
stay stable across launches. Same pattern and same footgun family as ShinyAirdropGuns (handoff
§5): the id lookup key is the container name, so it's stable as long as you don't rename the
container keys.

The csproj **excludes** `config/ids.jsonc` from the build-copy glob (aligned with
ShinyAirdropGuns/Questrandomizer on 2026-07-15) so a rebuild never clobbers the live server's
populated file with the workspace placeholder. That required a matching code change: `OnLoad`
now loads ids.jsonc only `if File.Exists`, else starts from an empty dict — because a fresh
install no longer receives the file and the previous unconditional `GetJsonDataFromFile` would
have thrown on the missing file. Verified: the `dist/` build output (the fresh-install layout)
contains only `config.jsonc`, and the mod loads fine from empty.

`containers` dict quirk: "kappa" and "unheard" map to the same template id
(`5c093ca986f7740a1867ab12`) — intentional, that's the shared game template.

## 4. Files

| File | Role |
|---|---|
| `src/ProgressiveContainer.cs` | Everything. |
| `src/Config.cs` | `secureContainers` (per-container `{ requirements, areaType }`), `craftingTime`, `removeContainersFromPeacekeeper`. |
| `config/config.jsonc` | The recipes + toggles. |
| `config/ids.jsonc` | Runtime-owned craft-id cache (§3). |

(The `.nuget/` and `.vscode/` folders in this mod are local tooling leftovers, not part of the
mod.)
