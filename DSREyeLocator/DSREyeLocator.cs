using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Network;
using Dalamud.Plugin;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using ECommons.Opcodes;
using ECommons.Reflection;
using System;

namespace DSREyeLocator
{
    public unsafe class DSREyeLocator : IDalamudPlugin
    {
        public string Name => "DSR Eye Locator";
        internal static DSREyeLocator P { get; private set; }
        internal WindowSystem ws;
        internal ConfigWindow configWindow;
        internal OverlayWindow overlayWindow;
        internal Config config;
        internal long SanctityStartTime = 0;
        internal long DeathStartTime = 0;
        internal long SanctityStartTimeDelay = 0;
        internal long DeathStartTimeDelay = 0;
        internal int EyePos = -1;
        internal Dictionary<int, Vector2> EyesPositions = new()
        {
            { 0, new(100.00f, 60.00f) },
            { 1, new(128.28f, 71.72f) },
            { 2, new(140.00f, 100.00f) },
            { 3, new(128.28f, 128.28f) },
            { 4, new(100.00f, 140.00f) },
            { 5, new(71.72f, 128.28f) },
            { 6, new(60.00f, 100.00f) },
            { 7, new(71.72f, 71.72f) },
        };
        internal const uint KingThordanNameID = 3632;

        public DSREyeLocator(DalamudPluginInterface pi)
        {
            P = this;
            ECommons.ECommons.Init(pi, Module.ObjectFunctions, Module.DalamudReflector);
            new TickScheduler(delegate
            {
                config = Svc.PluginInterface.GetPluginConfig() as Config ?? new();
                ws = new();
                configWindow = new();
                ws.AddWindow(configWindow);
                overlayWindow = new();
                ws.AddWindow(overlayWindow);
                Svc.GameNetwork.NetworkMessage += OnNetworkMessage;
                Svc.Framework.Update += Tick;
                Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
                Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configWindow.IsOpen = true; };

                if (DalamudReflector.TryGetDalamudStartInfo(out var info))
                {
                    OpcodeUpdater.DownloadOpcodes($"https://github.com/Eternita-S/MyDalamudPlugins/raw/main/opcodes/{info.GameVersion}.txt",
                        (dic) =>
                        {
                            if (dic.TryGetValue("MapEffect", out var code))
                            {
                                config.MapEventOpcode = code;
                                PluginLog.Information($"Downloaded MapEffect opcode 0x{code:X}");
                            }
                        });
                }
            });
        }

        public void Dispose()
        {
            ECommons.ECommons.Dispose();
            Svc.GameNetwork.NetworkMessage -= OnNetworkMessage;
            Svc.Framework.Update -= Tick;
            Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
            Safe(overlayWindow.Dispose);
        }

        private void Tick(Framework framework)
        {
            if (Svc.ClientState.LocalPlayer == null) return;
            if(Svc.ClientState.TerritoryType == 968 || P.config.Test)
            {
                Safe(delegate
                {
                    if (Svc.Condition[ConditionFlag.InCombat])
                    {
                        foreach (var x in Svc.Objects)
                        {
                            if (x is BattleNpc b && b.IsCasting && b.NameId == KingThordanNameID)
                            {
                                if (SanctityStartTime == 0 && b.CastActionId == 25569) //sanctity of the ward
                                {
                                    SanctityStartTime = Environment.TickCount64;
                                    if(P.config.Delay) SanctityStartTimeDelay = Environment.TickCount64 + P.config.SanctityDelay;
                                }
                                else if (DeathStartTime == 0 && b.CastActionId == 27538) //death of the heavens
                                {
                                    DeathStartTime = Environment.TickCount64;
                                    if (P.config.Delay) DeathStartTimeDelay = Environment.TickCount64 + P.config.DeathDelay;
                                }
                            }
                        }
                        if (((IsSanctity() && Environment.TickCount64 > SanctityStartTimeDelay) || (IsDeath() && Environment.TickCount64 > DeathStartTimeDelay)) && EyesPositions.TryGetValue(EyePos, out var eye)
                            && Svc.Objects.TryGetFirst(x => x is BattleNpc b && b.NameId == KingThordanNameID
                            && b.IsCharacterVisible(), out var thordan))
                        {
                            if (P.config.EnableTether)
                            {
                                Safe(delegate
                                {
                                    SplatoonManager.DrawLine(SplatoonManager.Get(), Svc.ClientState.LocalPlayer.Position,
                                        eye.ToVector3(), P.config.Color, P.config.Thickness);
                                    SplatoonManager.DrawLine(SplatoonManager.Get(), Svc.ClientState.LocalPlayer.Position,
                                        thordan.Position, P.config.Color, P.config.Thickness);
                                });
                            }
                            if (P.config.EnableBanner)
                            {
                                P.overlayWindow.Correct = !ConeHandler.IsInCone(eye) && !ConeHandler.IsInCone(thordan.Position.ToVector2());
                            }
                        }
                    }
                    else
                    {
                        SanctityStartTime = 0;
                        DeathStartTime = 0;
                    }
                });
            }
        }

        internal bool IsSanctity()
        {
            if (SanctityStartTime == 0) return false;
            return Environment.TickCount64 < SanctityStartTime + 30000;
        }

        internal bool IsDeath()
        {
            if (DeathStartTime == 0) return false;
            return Environment.TickCount64 < DeathStartTime + 40000;
        }

        private void OnNetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            Safe(delegate
            {
                if (direction == NetworkMessageDirection.ZoneDown)
                {
                    if (Svc.ClientState.TerritoryType == 838 && configWindow.IsOpen && !configWindow.OpcodeFound)
                    {
                        var data = (FFXIVIpcMapEffect*)dataPtr;
                        //80030043, 00080004, 0003, 0000
                        if (data->InstanceContentID == 0x80030043
                            && data->unk_4 == 0x00080004
                            && data->unk_8 == 0x03
                            && data->unk_12 == 0x0000)
                        {
                            configWindow.OpcodeFound = true;
                            P.config.MapEventOpcode = opCode;
                            Svc.PluginInterface.SavePluginConfig(config);
                        }
                    }
                    if (Svc.ClientState.TerritoryType == 968 || P.config.Test)
                    {
                        var data = (FFXIVIpcMapEffect*)dataPtr;
                        if (opCode == P.config.MapEventOpcode)
                        {
                            PluginLog.Debug($"MapEvent: {data->InstanceContentID:X8}, {data->unk_4:X8}, {data->unk_8:X2}, {data->unk_10:X2}, {data->unk_12:X4}");
                        }
                        if(IsSanctity() || IsDeath())
                        {
                            if(data->unk_4 == 0x00020001)
                            {
                                EyePos = data->unk_8;
                                if (!EyesPositions.ContainsKey(EyePos))
                                {
                                    Svc.Chat.PrintError("No data for this eye position was present");
                                    Svc.Chat.PrintError($"MapEvent: {data->InstanceContentID:X8}, {data->unk_4:X8}, {data->unk_8:X2}, {data->unk_10:X2}, {data->unk_12:X4}");
                                }
                            }
                            else if(data->unk_4 == 0x00400020)
                            {
                                EyePos = -1;
                            }
                        }
                    }
                }
            });
        }
    }
}
