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
                ("Eye locator", TabEyeConfig.Draw, null, true),
                ("Flames automarker", TabFlames.Draw, null, true),
                ("TargetSwitcher", TabTargetSwitcher.Draw, null, true),
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
