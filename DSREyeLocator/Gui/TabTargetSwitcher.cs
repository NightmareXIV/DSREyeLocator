using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Gui
{
    internal  static class TabTargetSwitcher
    {
        internal static void Draw()
        {
            ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "This module is untested and not completed yet. It may not work or work unexpectedly. Additionally, it is only intended for ranged dps. Proceed with caution.");
            ImGui.Checkbox("Enable module", ref P.config.TargetEnabled);
            if (!P.config.TargetEnabled) return;
            ImGuiEx.EnumCombo("My target", ref P.config.MyDragon);
            ImGui.DragFloat("Switch target if percent is higher than", ref P.config.SwitchTreshold);
            ImGui.Checkbox("Untarget if akh afah is being cast", ref P.config.NoDamageAkhAfah);
        }
    }
}
