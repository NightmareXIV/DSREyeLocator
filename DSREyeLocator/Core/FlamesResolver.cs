using Dalamud;
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
        internal static Queue<string> ChatCommands = new();
        internal static long NextChatCommandAt = 0;

        internal static void ResolveFlames()
        {
            if (!FlamesResolved && Svc.Party
                .Where(x => x.GameObject is PlayerCharacter)
                .Select(x => ((PlayerCharacter)x.GameObject).StatusList)
                .Count(x => x.Any(s => s.StatusId == EntangledFlames || s.StatusId == SpreadingFlames)) == 6)
            {
                FlamesResolved = true;
                NextChatCommandAt = Environment.TickCount64 + 1000;
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
                    $"/marking {GetLocalizedBind()}1",
                    $"/marking {GetLocalizedBind()}2",
                    });
                    Queue<string> NoneCommands = new(new string[]
                    {
                    $"/marking {GetLocalizedIgnore()}1",
                    $"/marking {GetLocalizedIgnore()}2",
                    });
                    Queue<string> SpreadingCommands = new(new string[]
                    {
                    $"/marking {GetLocalizedAttack()}1",
                    $"/marking {GetLocalizedAttack()}2",
                    $"/marking {GetLocalizedAttack()}3",
                    $"/marking {GetLocalizedAttack()}4",
                    });
                    foreach (var s in Svc.Party)
                    {
                        if (s.GameObject is PlayerCharacter x)
                        {
                            PluginLog.Information($"Player {x.Name}");
                            if (x.TryGetPlaceholder(out var num))
                            {
                                PluginLog.Information($"-- Player {x.Name} placeholder {num} statuses {x.StatusList.Select(s => s.StatusId.ToString()).Join(", ")}");
                                if (x.StatusList.Any(s => s.StatusId == SpreadingFlames))
                                {
                                    var cmd = SpreadingCommands.Dequeue();
                                    commands.Add($"{cmd} <{num}>");
                                    PluginLog.Information($"-- Player {x.Name} command {cmd} <{num}>");
                                }
                                else if (x.StatusList.Any(s => s.StatusId == EntangledFlames))
                                {
                                    var cmd = EngangledCommands.Dequeue();
                                    commands.Add($"{cmd} <{num}>");
                                    PluginLog.Information($"-- Player {x.Name} command {cmd} <{num}>");
                                }
                                else
                                {
                                    var cmd = NoneCommands.Dequeue();
                                    commands.Add($"{cmd} <{num}>");
                                    PluginLog.Information($"-- Player {x.Name} command {cmd} <{num}>");
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
                    if (P.config.FlamesEmulateDelay)
                    {
                        foreach(var x in commands)
                        {
                            ChatCommands.Enqueue(x);
                        }
                    }
                    else
                    {
                        MacroManager.Execute(commands);
                    }
                }
                PluginLog.Information("=== Wroth flames ===");
                foreach (var x in commands)
                {
                    PluginLog.Information(x);
                }
                PluginLog.Information("====================");
                ClearScheduler?.Dispose();
                ClearScheduler = new TickScheduler(ClearMarkers, 30000);
            }
        }
        internal static void ClearMarkers()
        {
            List<string> l = new();
            if (P.config.FlamesOnlySelf)
            {
                l.Add($"/marking off <me>");
            }
            else
            {
                for (var i = 1; i <= 8; i++)
                {
                    l.Add($"/marking off <{i}>");
                }
            }
            if (P.config.WrothFlamesOperational)
            {
                MacroManager.Execute(l);
            }
            PluginLog.Information("=== Wroth flames ===");
            foreach (var x in l)
            {
                PluginLog.Information(x);
            }
            PluginLog.Information("====================");
            ClearScheduler?.Dispose();
        }

        internal static string GetLocalizedAttack()
        {
            if (Svc.Data.Language == ClientLanguage.French)
            {
                return "attaque";
            }
            else if (Svc.Data.Language == ClientLanguage.German)
            {
                return "att";
            }
            else if (Svc.Data.Language == ClientLanguage.Japanese)
            {
                return "attack";
            }
            else
            {
                return "attack";
            }
        }

        internal static string GetLocalizedBind()
        {
            if (Svc.Data.Language == ClientLanguage.French)
            {
                return "entrave";
            }
            else if (Svc.Data.Language == ClientLanguage.German)
            {
                return "bind";
            }
            else if (Svc.Data.Language == ClientLanguage.Japanese)
            {
                return "bind";
            }
            else
            {
                return "bind";
            }
        }

        internal static string GetLocalizedIgnore()
        {
            if (Svc.Data.Language == ClientLanguage.French)
            {
                return "interdit";
            }
            else if (Svc.Data.Language == ClientLanguage.German)
            {
                return "ignor";
            }
            else if (Svc.Data.Language == ClientLanguage.Japanese)
            {
                return "stop";
            }
            else
            {
                return "ignore";
            }
        }

        internal static void FlamesTick()
        {
            if (!Svc.Condition[ConditionFlag.InCombat])
            {
                ChatCommands.Clear();
            }
            if(Environment.TickCount64 > NextChatCommandAt && ChatCommands.TryDequeue(out var command))
            {
                NextChatCommandAt = Environment.TickCount64 + new Random().Next(250, 500);
                if (P.config.WrothFlamesOperational)
                {
                    PluginLog.Information($"Sending chat command: {command}");
                    P.chat.SendMessage(command);
                }
                else
                {
                    PluginLog.Information($"Sending fake chat command: {command}");
                    P.chat.SendMessage($"/echo {command}");
                }
            }
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
