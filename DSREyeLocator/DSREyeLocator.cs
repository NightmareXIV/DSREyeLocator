using Dalamud.Game;
using Dalamud.Game.Network;
using Dalamud.Plugin;
using System;

namespace DSREyeLocator
{
    public class DSREyeLocator : IDalamudPlugin
    {
        public string Name => "DSR Eye Locator";
        internal static DSREyeLocator P { get; private set; }
        internal WindowSystem ws;
        internal ConfigWindow configWindow;
        internal OverlayWindow overlayWindow;
        internal Config config;

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
            
        }

        private void OnNetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            
        }
    }
}
