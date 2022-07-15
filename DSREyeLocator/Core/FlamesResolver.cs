using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.Automation;
using ECommons.GameFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Core
{
    internal unsafe static class FlamesResolver
    {
        internal const uint EntangledFlames = 2759;
        internal const uint SpreadingFlames = 2758;
        internal static bool FlamesResolved = false;
        internal static TickScheduler ClearScheduler;
        internal static bool FlamesClearRequested = false;

        internal static void ResolveFlames()
        {
            if (!FlamesResolved && Svc.Party
                .Where(x => x.GameObject is PlayerCharacter)
                .Select(x => ((PlayerCharacter)x.GameObject).StatusList)
                .Count(x => x.Any(s => s.StatusId == EntangledFlames || s.StatusId == SpreadingFlames)) == 6)
            {
                FlamesResolved = true;
                List<string> commands = new();
                if (P.config.FlamesOnlySelf)
                {
                    if (Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == SpreadingFlames))
                    {
                        DuoLog.Debug("Flames: spread self");
                        commands.Add(P.config.FlamesSelfSpread);
                    }
                    else if (Svc.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == EntangledFlames))
                    {
                        DuoLog.Debug("Flames: stack self");
                        commands.Add(P.config.FlamesSelfStack);
                    }
                    else
                    {
                        DuoLog.Debug("Flames: none self");
                        commands.Add(P.config.FlamesSelfNone);
                    }
                }
                else
                {
                    Queue<string> EngangledCommands = new(new string[]
                    {
                    "/enemysign bind1",
                    "/enemysign bind2",
                    });
                    Queue<string> NoneCommands = new(new string[]
                    {
                    "/enemysign ignore1",
                    "/enemysign ignore2",
                    });
                    Queue<string> SpreadingCommands = new(new string[]
                    {
                    "/enemysign attack1",
                    "/enemysign attack2",
                    "/enemysign attack3",
                    "/enemysign attack4",
                    });
                    foreach (var s in Svc.Party)
                    {
                        if (s.GameObject is PlayerCharacter x)
                        {
                            DuoLog.Debug($"Player {x.Name}");
                            if (x.TryGetPlaceholder(out var num))
                            {
                                DuoLog.Debug($"Player {x.Name} placeholder {num} statuses {x.StatusList.Select(s => s.StatusId.ToString()).Join(", ")}");
                                if (x.StatusList.Any(s => s.StatusId == SpreadingFlames))
                                {
                                    commands.Add($"{SpreadingCommands.Dequeue()} <{num}>");
                                }
                                else if (x.StatusList.Any(s => s.StatusId == EntangledFlames))
                                {
                                    commands.Add($"{EngangledCommands.Dequeue()} <{num}>");
                                }
                                else
                                {
                                    commands.Add($"{NoneCommands.Dequeue()} <{num}>");
                                }
                            }
                            else
                            {
                                PluginLog.Error($"Failed to resolve placeholder for {x.Name}");
                            }
                        }
                        else
                        {
                            DuoLog.Warning($"Not a PC in party {s.GameObject}");
                        }
                    }
                }
                if (P.config.WrothFlamesOperational)
                {
                    MacroManager.Execute(commands);
                }
                else
                {
                    DuoLog.Information("=== Wroth flames ===");
                    foreach (var x in commands)
                    {
                        DuoLog.Information(x);
                    }
                    DuoLog.Information("====================");
                }
                ClearScheduler?.Dispose();
                ClearScheduler = new TickScheduler(ClearMarkers, 30000);
            }
        }
        internal static void ClearMarkers()
        {
            List<string> l = new();
            if (P.config.FlamesOnlySelf)
            {
                l.Add($"/enemysign off <me>");
            }
            else
            {
                for (var i = 1; i <= 8; i++)
                {
                    l.Add($"/enemysign off <{i}>");
                }
            }
            if (P.config.WrothFlamesOperational)
            {
                MacroManager.Execute(l);
            }
            else
            {
                DuoLog.Information("=== Wroth flames ===");
                foreach (var x in l)
                {
                    DuoLog.Information(x);
                }
                DuoLog.Information("====================");
            }
            ClearScheduler?.Dispose();
        }

        internal static void FlamesTick()
        {
            if (FlamesClearRequested)
            {
                if (!Svc.Condition[ConditionFlag.Unconscious] &&
                    Svc.Party.All(x => x.GameObject != null
                    && x.GameObject.Struct()->GetIsTargetable()
                    && !x.GameObject.Struct()->IsDead()))
                {
                    FlamesClearRequested = false;
                    ClearMarkers();
                }
            }
            ResolveFlames();
        }
    }
}
