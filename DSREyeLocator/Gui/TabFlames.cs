using Dalamud.Interface.Components;
using DSREyeLocator.Core;
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
            //ImGuiEx.TextWrapped(ImGuiColors.DalamudOrange, "This module had very little testing. It may not work or have problems. ");
            ImGui.Checkbox("Enable module", ref P.config.WrothFlames);
            if (!P.config.WrothFlames) return;
            ImGui.Checkbox("Operational mode", ref P.config.WrothFlamesOperational);
            ImGuiEx.Text("     will print would-be executed macro commands into chat if unchecked");
            if (P.config.WrothFlamesOperational)
            {
                ImGuiEx.Text("Test:");
                ImGui.SameLine();
                if (ImGui.Button("attack marker"))
                {
                    P.chat.SendMessage($"/marking {FlamesResolver.GetLocalizedAttack()}1 <me>");
                }
                ImGui.SameLine();
                if (ImGui.Button("bind marker"))
                {
                    P.chat.SendMessage($"/marking {FlamesResolver.GetLocalizedBind()}1 <me>");
                }
                ImGui.SameLine();
                if (ImGui.Button("ignore marker"))
                {
                    P.chat.SendMessage($"/marking {FlamesResolver.GetLocalizedIgnore()}1 <me>");
                }
            }
            ImGui.Checkbox("Add random delay between markings (recommended)", ref P.config.FlamesEmulateDelay);
            /*if (P.config.FlamesEmulateDelay)
            {
                ImGui.SameLine();
                if (ImGui.Button("Test"))
                {
                    P.config.Test = true;
                    FlamesResolver.ChatCommands.Enqueue("/marking attack1 <1>");
                    FlamesResolver.ChatCommands.Enqueue("/marking attack2 <2>");
                    FlamesResolver.ChatCommands.Enqueue("/marking attack3 <3>");
                    FlamesResolver.ChatCommands.Enqueue("/marking attack4 <4>");
                    FlamesResolver.ChatCommands.Enqueue("/marking bind1 <5>");
                    FlamesResolver.ChatCommands.Enqueue("/marking bind2 <6>");
                    FlamesResolver.ChatCommands.Enqueue("/marking ignore1 <7>");
                    FlamesResolver.ChatCommands.Enqueue("/marking ignore2 <8>");
                }
                ImGuiComponents.HelpMarker("Will try to place markers on party members with delay. You need to be in combat.");
            }*/
            ImGui.Checkbox("Self only", ref P.config.FlamesOnlySelf);
            if (P.config.FlamesOnlySelf)
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "Marker will be applied ONLY TO YOU. Uncheck \"Self only\" if you want to use plugin as automarker for the whole party.");
                ImGui.InputText("Spread command", ref P.config.FlamesSelfSpread, 100);
                ImGui.InputText("Stack command", ref P.config.FlamesSelfStack, 100);
                ImGui.InputText("None command", ref P.config.FlamesSelfNone, 100);
            }
            else
            {
                ImGui.Checkbox("Mark people with stack debuff", ref P.config.MarkStacks);
                ImGui.Checkbox("Mark people with spread debuff", ref P.config.MarkSpreads);
                ImGui.Checkbox("Mark people without debuff", ref P.config.MarkNones);
                ImGui.Checkbox("Use custom commands", ref P.config.UseCustomCommands);
                if (P.config.UseCustomCommands)
                {
                    ImGuiEx.Text("Commands for spreading players:");
                    if (P.config.CustomCommandsSpread.Count(x => x == '\n') < 3)
                    {
                        ImGui.SameLine();
                        ImGuiEx.Text(Environment.TickCount % 1000 > 500 ? ImGuiColors.DalamudRed : ImGuiColors.DalamudYellow, "Must have at least 4 commands");
                    }
                    ImGui.InputTextMultiline("##spread", ref P.config.CustomCommandsSpread, 1000, new(ImGui.GetContentRegionAvail().X, 100));

                    ImGuiEx.Text("Commands for stacking players:");
                    if (P.config.CustomCommandsStack.Count(x => x == '\n') < 1)
                    {
                        ImGui.SameLine();
                        ImGuiEx.Text(Environment.TickCount % 1000 > 500 ? ImGuiColors.DalamudRed : ImGuiColors.DalamudYellow, "Must have at least 4 commands");
                    }
                    ImGui.InputTextMultiline("##stak", ref P.config.CustomCommandsStack, 1000, new(ImGui.GetContentRegionAvail().X, 100));

                    ImGuiEx.Text("Commands for no debuff players:");
                    if (P.config.CustomCommandsNone.Count(x => x == '\n') < 1)
                    {
                        ImGui.SameLine();
                        ImGuiEx.Text(Environment.TickCount % 1000 > 500 ? ImGuiColors.DalamudRed : ImGuiColors.DalamudYellow, "Must have at least 4 commands");
                    }
                    ImGui.InputTextMultiline("##none", ref P.config.CustomCommandsNone, 1000, new(ImGui.GetContentRegionAvail().X, 100));
                }
            }
            if(ImGui.Button("Clear markers"))
            {
                ClearMarkers();
            }
        }
    }
}
