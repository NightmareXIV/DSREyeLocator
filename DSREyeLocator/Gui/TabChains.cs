using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Gui
{
    internal static class TabChains
    {
        internal static void Draw()
        {
            ImGui.Checkbox("Enable module", ref P.config.ChainEnabled);
            if (!P.config.ChainEnabled) return;

            ImGui.SetNextItemWidth(50f);
            ImGui.InputFloat("Tether thickness", ref P.config.ChainThickness);
            P.config.Thickness.ValidateRange(0.1f, 50f);
            var col = P.config.ChainColor.ToVector4();
            ImGui.ColorEdit4("Tether color", ref col);
            P.config.ChainColor = col.ToUint();
        }
    }
}
