using Dalamud.Game.ClientState.Objects.Types;
using ECommons.MathHelpers;
using ECommons.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Gui
{
    internal unsafe static class TabMainConfig
    {
        internal static bool OpcodeFound = false;
        internal static void Draw()
        {
            ImGuiEx.Text("0x");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(50f);
            ImGuiEx.InputHex("MapEvent opcode", ref P.config.MapEventOpcode);

            ImGui.Separator();

            ImGui.Checkbox("Test mode", ref P.config.Test); 
            ImGui.Separator();
            if (Svc.ClientState.TerritoryType == 838)
            {
                if (OpcodeFound)
                {
                    ImGuiEx.Text(Environment.TickCount % 400 > 200 ? ImGuiColors.ParsedGreen : Vector4.Zero, "Opcode found and recorded!");
                }
                else
                {
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, "Go forward until meteor hits the groung");
                }
            }
            else
            {
                ImGuiEx.Text(ImGuiColors.DalamudYellow, "Enter Amaurot to enable opcode finder");
            }
            ImGui.Separator();
            ImGuiEx.Text("Debug:");
            ImGuiEx.Text($"Sanctity: {P.SanctityStartTime}/{P.IsSanctity()}");
            ImGuiEx.Text($"Death: {P.DeathStartTime}/{P.IsDeath()}");
            if (Svc.Targets.Target != null)
            {
                var angle = ConeHandler.GetAngleTo(Svc.Targets.Target.Position.ToVector2());
                ImGuiEx.Text(ConeHandler.IsInCone(Svc.Targets.Target.Position.ToVector2()) ? ImGuiColors.DalamudRed : ImGuiColors.DalamudWhite, $"{angle}");
                if (Svc.Targets.Target is Character c)
                {
                    ImGuiEx.Text($"{c.NameId}");
                }
            }
            if (DalamudReflector.TryGetDalamudStartInfo(out var info))
            {
                ImGuiEx.TextCopy($"{info.GameVersion.ToString()}");
            }
        }
    }
}
