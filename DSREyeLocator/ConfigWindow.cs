using ECommons.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator
{
    internal class ConfigWindow : Window
    {
        public ConfigWindow() : base($"{P.Name} configuration")
        {
        }

        public override void Draw()
        {
            ImGuiEx.Text("0x");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(50f);
            ImGuiEx.InputUint("MapEvent opcode", ref P.config.MapEventOpcode);
            if(ImGui.Checkbox("Enable tether to eye and Thordan (requires Splatoon)", ref P.config.EnableTether))
            {
                if(P.config.EnableTether && !DalamudReflector.TryGetDalamudPlugin("Splatoon", out _))
                {
                    Notify.Error("You do not have Splatoon installed");
                    P.config.EnableTether = false;
                }
            }
            if (P.config.EnableTether)
            {
                ImGui.SetNextItemWidth(50f);
                ImGui.InputFloat("Tether thickness", ref P.config.Thickness);
                P.config.Thickness.ValidateRange(0.1f, 50f);
                var col = P.config.Color.
                ImGui.ColorEdit4("Tether color", )
            }
        }
    }
}
