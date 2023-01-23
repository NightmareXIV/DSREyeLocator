using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Components;
using ECommons.MathHelpers;
using ECommons.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Gui
{
    internal unsafe class ConfigWindow : Window
    {

        public ConfigWindow() : base($"{P.Name} configuration")
        {
            this.SizeConstraints = new()
            {
                MaximumSize = new(9999, 9999),
                MinimumSize = new(400, 200)
            };
        }

        public override void Draw()
        {
            KoFiButton.DrawRight();
            ImGuiEx.EzTabBar("DSREyeMainTabBar",
                ("Modules", delegate
                {
                    ImGuiEx.EzTabBar("Func",
                        ("[P2/P5] Eye Locator", TabEyeConfig.Draw, P.config.EyeEnabled ? ImGuiColors.ParsedGreen : null, true),
                        ("[P1/P5] Chain Tether", TabChains.Draw, P.config.ChainEnabled ? ImGuiColors.ParsedGreen : null, true),
                        ("[P6] Flames Automarker", TabFlames.Draw, P.config.WrothFlames ? ImGuiColors.ParsedGreen : null, true)
                        );
                }, ImGuiColors.DalamudOrange, true
                ),
                ("Options", TabMainConfig.Draw, null, true),
                ("Debug", Debug.Draw, ImGuiColors.DalamudGrey3, true)
                );
        }

        public override void OnClose()
        {
            P.config.Test = false;
            Svc.PluginInterface.SavePluginConfig(P.config);
            Notify.Success("Configuration saved");
        }
    }
}
