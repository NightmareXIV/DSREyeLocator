using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Gui
{
    internal static class TabFlames
    {
        internal static void Draw()
        {
            ImGuiEx.TextWrapped(ImGuiColors.DalamudOrange, "This module had very little testing. It may not work or have problems. ");
            ImGui.Checkbox("Enable module", ref P.config.WrothFlames);
            if (!P.config.WrothFlames) return;
            ImGui.Checkbox("Operational mode", ref P.config.WrothFlamesOperational);
            ImGuiEx.Text("     will print would-be executed macro commands into chat if unchecked");
            ImGui.Checkbox("Self only", ref P.config.FlamesOnlySelf);
            if (P.config.FlamesOnlySelf)
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "Marker will be applied ONLY TO YOU. Uncheck \"Self only\" if you want to use plugin as automarker for the whole party.");
                ImGui.InputText("Spread command", ref P.config.FlamesSelfSpread, 100);
                ImGui.InputText("Stack command", ref P.config.FlamesSelfStack, 100);
                ImGui.InputText("None command", ref P.config.FlamesSelfNone, 100);
            }
        }
    }
}
