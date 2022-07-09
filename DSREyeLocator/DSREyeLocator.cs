using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Network;
using Dalamud.Logging;
using Dalamud.Plugin;
using DSREyeLocator.Core;
using DSREyeLocator.Gui;
using ECommons.Automation;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using ECommons.Opcodes;
using ECommons.Reflection;
using System;

namespace DSREyeLocator
{
    public unsafe class DSREyeLocator : IDalamudPlugin
    {
        public string Name => "DSR Toolbox";
        internal const uint DSRTerritory = 968;
        internal static DSREyeLocator P { get; private set; }
        internal WindowSystem ws;
        internal ConfigWindow configWindow;
        internal OverlayWindow overlayWindow;
        internal Config config;

        public DSREyeLocator(DalamudPluginInterface pi)
        {
            P = this;
            ECommons.ECommons.Init(pi, Module.ObjectFunctions, Module.DalamudReflector);
            /*DuoLog.Verbose("Verbose");
            DuoLog.Debug("Debug");
            DuoLog.Information("Information");
            DuoLog.Warning("Warning");
            DuoLog.Error("Error");
            DuoLog.Fatal("Fatal");*/
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
                Svc.Condition.ConditionChange += Condition_ConditionChange;

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

                Headmarker.Init();
            });
        }

        public void Dispose()
        {
            Svc.GameNetwork.NetworkMessage -= OnNetworkMessage;
            Svc.Framework.Update -= Tick;
            Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
            Svc.Condition.ConditionChange -= Condition_ConditionChange;
            Safe(overlayWindow.Dispose);
            Safe(Headmarker.Dispose);
            ECommons.ECommons.Dispose();
        }

        private void Tick(Framework framework)
        {
            if (Svc.ClientState.LocalPlayer == null) return;
            if(Svc.ClientState.TerritoryType == DSRTerritory || P.config.Test)
            {
                Safe(delegate
                {
                    if(P.config.EyeEnabled) EyeTick();
                    if(P.config.ChainEnabled) ChainsResolver.ChainsTick();
                    if (P.config.WrothFlames) FlamesTick();
                    if (P.config.TargetEnabled) DDResolver.Tick();
                });
            }
        }

        internal void Condition_ConditionChange(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.InCombat && Svc.ClientState.TerritoryType == DSRTerritory)
            {
                if (value)
                {
                    PluginLog.Debug("Combat started");
                }
                else
                {
                    PluginLog.Debug("Combat finished");
                    Headmarker.HeadmarkerInfos.Clear();
                    if (P.config.WrothFlames && FlamesResolved && ClearScheduler != null)
                    {
                        FlamesClearRequested = true;
                    }
                }
                FlamesResolved = false;
            }
        }
    }
}
