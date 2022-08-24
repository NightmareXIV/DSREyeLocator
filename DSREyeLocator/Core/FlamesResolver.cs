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
                List<string> commands = new();
                if (P.config.FlamesOnlySelf)
                {
                    NextChatCommandAt = Environment.TickCount64 + 100;
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
                    Queue<string> EngangledCommands;
                    Queue<string> NoneCommands;
                    Queue<string> SpreadingCommands;
                    NextChatCommandAt = Environment.TickCount64 + 500;
                    if (P.config.UseCustomCommands)
                    {
                        EngangledCommands = new(P.config.CustomCommandsStack.Split("\n"));
                        NoneCommands = new(P.config.CustomCommandsNone.Split("\n"));
                        SpreadingCommands = new(P.config.CustomCommandsSpread.Split("\n"));
                    }
                    else
                    {
                        EngangledCommands = new(new string[]
                        {
                            $"/marking {GetLocalizedBind()}1",
                            $"/marking {GetLocalizedBind()}2",
                        });
                        NoneCommands = new(new string[]
                        {
                            $"/marking {GetLocalizedIgnore()}1",
                            $"/marking {GetLocalizedIgnore()}2",
                        });
                        SpreadingCommands = new(new string[]
                        {
                            $"/marking {GetLocalizedAttack()}1",
                            $"/marking {GetLocalizedAttack()}2",
                            $"/marking {GetLocalizedAttack()}3",
                            $"/marking {GetLocalizedAttack()}4",
                        });
                    }
                    foreach (var s in Svc.Party)
                    {
                        if (s.GameObject is PlayerCharacter x)
                        {
                            PluginLog.Debug($"Player {x.Name}");
                            if (x.TryGetPlaceholder(out var num))
                            {
                                PluginLog.Debug($"-- Player {x.Name} placeholder {num} statuses {x.StatusList.Select(s => s.StatusId.ToString()).Join(", ")}");
                                if (x.StatusList.Any(s => s.StatusId == SpreadingFlames))
                                {
                                    var cmd = SpreadingCommands.Dequeue();
                                    if(P.config.MarkSpreads) commands.Add($"{cmd} <{num}>");
                                    PluginLog.Debug($"-- Player {x.Name} command {cmd} <{num}> {P.config.MarkSpreads}");
                                }
                                else if (x.StatusList.Any(s => s.StatusId == EntangledFlames))
                                {
                                    var cmd = EngangledCommands.Dequeue();
                                    if (P.config.MarkStacks) commands.Add($"{cmd} <{num}>");
                                    PluginLog.Debug($"-- Player {x.Name} command {cmd} <{num}> {P.config.MarkStacks}");
                                }
                                else
                                {
                                    var cmd = NoneCommands.Dequeue();
                                    if (P.config.MarkNones) commands.Add($"{cmd} <{num}>");
                                    PluginLog.Debug($"-- Player {x.Name} command {cmd} <{num}> {P.config.MarkNones}");
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
                PluginLog.Debug("=== Wroth flames ===");
                foreach (var x in commands)
                {
                    PluginLog.Debug(x);
                }
                PluginLog.Debug("====================");
                ClearScheduler?.Dispose();
                ClearScheduler = new TickScheduler(ClearMarkers, 30000);
            }
        }
        internal static void ClearMarkers()
        {
            List<string> l = new();
            if (P.config.FlamesOnlySelf)
            {
                //l.Add($"/marking off <me>");
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
            PluginLog.Debug("=== Wroth flames ===");
            foreach (var x in l)
            {
                PluginLog.Debug(x);
            }
            PluginLog.Debug("====================");
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
                    PluginLog.Debug($"Sending chat command: {command}");
                    if (command.StartsWith("/"))
                    {
                        P.chat.SendMessage(command);
                    }
                    else
                    {
                        DuoLog.Error($"Command must start with slash. Received: {command}");
                    }
                }
                else
                {
                    PluginLog.Debug($"Sending fake chat command: {command}");
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
