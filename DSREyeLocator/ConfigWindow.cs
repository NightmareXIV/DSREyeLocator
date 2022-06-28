using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Components;
using ECommons.MathHelpers;
using ECommons.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator
{
    internal unsafe class ConfigWindow : Window
    {
        internal bool OpcodeFound = false;

        public ConfigWindow() : base($"{P.Name} configuration")
        {
        }

        public override void Draw()
        {
            ImGuiEx.Text("0x");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(50f);
            ImGuiEx.InputHex("MapEvent opcode", ref P.config.MapEventOpcode);
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
            ImGui.Separator();
            ImGui.Checkbox("Test mode", ref P.config.Test);
            ImGui.Separator();
            if(Svc.ClientState.TerritoryType == 838)
            {
                if (OpcodeFound)
                {
                    ImGuiEx.Text(Environment.TickCount % 400 > 200?ImGuiColors.ParsedGreen:Vector4.Zero, "Opcode found and recorded!");
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
            if(Svc.Targets.Target != null)
            {
                var angle = ConeHandler.GetAngleTo(Svc.Targets.Target.Position.ToVector2());
                ImGuiEx.Text(ConeHandler.IsInCone(Svc.Targets.Target.Position.ToVector2())?ImGuiColors.DalamudRed:ImGuiColors.DalamudWhite, $"{angle}");
                if(Svc.Targets.Target is Character c)
                {
                    ImGuiEx.Text($"{c.Address.As<FFXIVClientStructs.FFXIV.Client.Game.Character.Character>()->NameID}");
                }
            }
            if (DalamudReflector.TryGetDalamudStartInfo(out var info))
            {
                ImGuiEx.TextCopy($"{info.GameVersion.ToString()}");
            }
        }

        public override void OnClose()
        {
            P.config.Test = false;
            Svc.PluginInterface.SavePluginConfig(P.config);
            Notify.Success("Configuration saved");
        }
    }
}
