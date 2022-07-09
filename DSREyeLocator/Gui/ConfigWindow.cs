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
        }

        public override void Draw()
        {
            ImGuiEx.EzTabBar("DSREyeMainTabBar",
                ("General", TabMainConfig.Draw, null, true),
                ("[P2/P5] Eye Locator", TabEyeConfig.Draw, P.config.EyeEnabled ? ImGuiColors.ParsedGreen : null, true),
                ("[P1/P5] Chain Tether", TabChains.Draw, P.config.ChainEnabled ? ImGuiColors.ParsedGreen : null, true),
                ("[P6] Flames Automarker", TabFlames.Draw, P.config.WrothFlames ? ImGuiColors.ParsedGreen : null, true),
                ("[P6] TargetSwitcher", TabTargetSwitcher.Draw, P.config.TargetEnabled ? ImGuiColors.ParsedGreen : null, true),
                ("Contribute", TabContribute.Draw, ImGuiColors.DalamudYellow, true)
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
