using ECommons.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator
{
    internal class SplatoonManager
    {
        static IDalamudPlugin Splatoon = null;
        static internal IDalamudPlugin Get()
        {
            try
            {
                if (Splatoon != null && (bool)Splatoon.GetType().GetField("Init").GetValue(Splatoon))
                {
                    return Splatoon;
                }
                DalamudReflector.TryGetDalamudPlugin("Splatoon", out var plugin);
                if ((bool)plugin.GetType().GetField("Init").GetValue(plugin))
                {
                    Splatoon = plugin;
                    return Splatoon;
                }
                else
                {
                    throw new Exception("Splatoon is not initialized");
                }
            }
            catch (Exception e)
            {
                PluginLog.Error("Can't find Splatoon plugin: " + e.Message);
                PluginLog.Error(e.StackTrace);
                Svc.Chat.PrintError("Splatoon was not found and tether feature was disabled");
                P.config.EnableTether = false;
                return null;
            }
        }

        static Vector3 addA1 = new(0.5f, 0, 0.5f);
        static Vector3 addB1 = new(-0.5f, 0, -0.5f);
        static Vector3 addA2 = new(-0.5f, 0, 0.5f);
        static Vector3 addB2 = new(0.5f, 0, -0.5f);
        static internal void DrawCrossMark(IDalamudPlugin spl, Vector3 position, bool tether)
        {
            var color = ImGui.ColorConvertFloat4ToU32(Environment.TickCount % 1000 > 500 ? ImGuiColors.DalamudRed : ImGuiColors.DalamudYellow);
            DrawLine(spl, position + addA1, position + addB1, color);
            DrawLine(spl, position + addA2, position + addB2, color);
            if (tether)
            {
                DrawLine(spl, position, Svc.ClientState.LocalPlayer.Position, color);
            }
        }

        static internal void DrawLine(IDalamudPlugin spl, Vector3 a, Vector3 b, uint color, float? thickness = null)
        {
            var element = spl.GetType().Assembly.CreateInstance("Splatoon.Element", false, BindingFlags.Default, null, new object[] { 2 }, null, null);
            element.GetType().GetField("refX").SetValue(element, a.X);
            element.GetType().GetField("refZ").SetValue(element, a.Y);
            element.GetType().GetField("refY").SetValue(element, a.Z);
            element.GetType().GetField("offX").SetValue(element, b.X);
            element.GetType().GetField("offZ").SetValue(element, b.Y);
            element.GetType().GetField("offY").SetValue(element, b.Z);
            element.GetType().GetField("radius").SetValue(element, 0f);
            element.GetType().GetField("color").SetValue(element, color);
            if (thickness != null) element.GetType().GetField("thicc").SetValue(element, thickness.Value);
            spl.GetType()
                .GetMethod("InjectElement")
                .Invoke(spl, new object[] { element });
        }

        static internal void DrawCircleWithText(IDalamudPlugin spl, Vector3 a, string text, uint color, float? thickness = null)
        {
            var element = spl.GetType().Assembly.CreateInstance("Splatoon.Element", false, BindingFlags.Default, null, new object[] { 0 }, null, null);
            element.GetType().GetField("refX").SetValue(element, a.X);
            element.GetType().GetField("refZ").SetValue(element, a.Y);
            element.GetType().GetField("refY").SetValue(element, a.Z);
            element.GetType().GetField("radius").SetValue(element, 2f);
            element.GetType().GetField("color").SetValue(element, color);
            element.GetType().GetField("overlayText").SetValue(element, "NORTH");
            element.GetType().GetField("overlayFScale").SetValue(element, 3f);
            element.GetType().GetField("overlayBGColor").SetValue(element, 0x9B000070);
            if (thickness != null) element.GetType().GetField("thicc").SetValue(element, thickness.Value);
            spl.GetType()
                .GetMethod("InjectElement")
                .Invoke(spl, new object[] { element });
        }
    }
}
