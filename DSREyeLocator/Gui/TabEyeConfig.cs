using Dalamud.Interface.Components;
using ECommons.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Gui
{
    internal static class TabEyeConfig
    {
        internal static void Draw()
        {
            if (ImGui.Checkbox("Enable tether to eye and Thordan (requires Splatoon)", ref P.config.EnableTether))
            {
                if (P.config.EnableTether && !DalamudReflector.TryGetDalamudPlugin("Splatoon", out _))
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
                var col = P.config.Color.ToVector4();
                ImGui.ColorEdit4("Tether color", ref col);
                P.config.Color = col.ToUint();
                Safe(delegate
                {
                    if (Svc.Targets.Target != null && P.config.Test) SplatoonManager.DrawLine(SplatoonManager.Get(), Svc.ClientState.LocalPlayer.Position,
                        Svc.Targets.Target.Position, P.config.Color, P.config.Thickness);
                });
            }
            ImGui.Checkbox("Enable banner", ref P.config.EnableBanner);
            if (P.config.EnableBanner)
            {
                ImGui.SetNextItemWidth(50f);
                ImGui.DragInt("Vertical offset", ref P.config.VerticalOffset);
                ImGui.SetNextItemWidth(50f);
                ImGui.DragInt("Horizontal offset", ref P.config.HorizontalOffset);
                ImGui.SetNextItemWidth(50f);
                ImGui.DragFloat("Scale", ref P.config.Scale, 0.002f, 0.1f, 10f);
                P.config.Scale.ValidateRange(0.1f, 10f);
            }
            ImGui.Checkbox("Blinking", ref P.config.BannerBlink);
            ImGuiEx.WithTextColor(ImGuiColors.DalamudOrange, delegate
            {
                ImGui.Checkbox("Delay displaying information (recommended)", ref P.config.Delay);
            });
            ImGuiComponents.HelpMarker("Delay displaying tethers and banner until it actually matters (when going out in sanctity/returning to the middle in death)");
            if (P.config.Delay)
            {
                ImGuiEx.TextWrapped("You can configure delays dependin on how much time you need for each mechanic:");
                if (ImGui.SmallButton("Reset delays to defaults"))
                {
                    var c = new Config();
                    P.config.SanctityDelay = c.SanctityDelay;
                    P.config.DeathDelay = c.DeathDelay;
                }
                ImGui.SetNextItemWidth(50f);
                ImGui.DragInt("Delay since start of Sanctity of the Ward cast, ms", ref P.config.SanctityDelay, 10, 0, 15000);
                ImGuiEx.Text("   - Sanctity of the Ward's Gaze resolves at 17731 ms");
                ImGui.SetNextItemWidth(50f);
                ImGui.DragInt("Delay since start of Death of the Heavens cast, ms", ref P.config.DeathDelay, 10, 0, 30000);
                ImGuiEx.Text("   - Death of the Heavens's Gaze resolves at 34255 ms");
            }
        }
    }
}
