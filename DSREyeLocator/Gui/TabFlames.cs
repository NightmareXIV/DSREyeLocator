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
            ImGui.Checkbox("Enable Wroth Flames automarker", ref P.config.WrothFlames);
            ImGui.Checkbox("Operational mode", ref P.config.WrothFlamesOperational);
            ImGuiEx.Text("     will print would-be executed macro commands into chat if unchecked");
        }
    }
}
