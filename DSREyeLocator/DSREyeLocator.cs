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
using Newtonsoft.Json;
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
        internal Chat chat;

        public DSREyeLocator(DalamudPluginInterface pi)
        {
            P = this;
            ECommonsMain.Init(pi, this, Module.ObjectFunctions, Module.DalamudReflector);
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
                    OpcodeUpdater.DownloadOpcodes($"https://github.com/NightmareXIV/MyDalamudPlugins/raw/main/opcodes/{info.GameVersion}.txt",
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
                new ChangelogWindow(config, 1, delegate
                {
                    ImGuiEx.Text("DSR Eye Locator has been renamed into DSR Toolbox and contains few other functions \n" +
                        "to help with DSR. " +
                        "\n\nI may add some other in future as I progress though the fight/do reclears of it." +
                        "\nBy default only eye locator is enabled, matching previous behavior of the plugin.  ");
                });
                Svc.ClientState.TerritoryChanged += TerrChanged;
                Svc.Commands.AddHandler("/eye", new(delegate { configWindow.IsOpen = true; }) { HelpMessage = "Open configuration" });
                chat = new();
            });
        }

        public void Dispose()
        {
            Svc.Commands.RemoveHandler("/eye");
            Svc.GameNetwork.NetworkMessage -= OnNetworkMessage;
            Svc.Framework.Update -= Tick;
            Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
            Svc.Condition.ConditionChange -= Condition_ConditionChange;
            Safe(overlayWindow.Dispose);
            Safe(Headmarker.Dispose);
            Svc.ClientState.TerritoryChanged -= TerrChanged;
            ECommonsMain.Dispose();
        }

        private void TerrChanged(object sender, ushort e)
        {
            if (P.config.MapEffectDbg)
            {
                P.config.MapEffectLog.RemoveAll(x => x.structs.Count == 0);
                P.config.MapEffectLog.Add((e, new()));
            }
        }

        private void Tick(Framework framework)
        {
            if (Svc.ClientState.LocalPlayer == null || Svc.Condition[ConditionFlag.DutyRecorderPlayback]) return;
            if(Svc.ClientState.TerritoryType == DSRTerritory || P.config.Test)
            {
                Safe(delegate
                {
                    if(P.config.EyeEnabled) EyeTick();
                    if(P.config.ChainEnabled) ChainsResolver.ChainsTick();
                    if (P.config.WrothFlames) FlamesTick();
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
                    if (P.config.WrothFlames)
                    {
                        if (FlamesResolved && ClearScheduler != null)
                        {
                            FlamesClearRequested = true;
                        }
                    }
                }
                FlamesResolved = false;
            }
        }
    }
}
