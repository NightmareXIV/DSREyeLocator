using Dalamud.Memory;
using ECommons.EzSharedDataManager;
using ECommons.Logging;
using ECommons.Reflection;
using ExposedObject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Gui
{
    internal unsafe static class Debug
    {
        internal static string header = "";
        internal static FFXIVIpcMapEffect sts = new();
        internal static void Draw()
        {

            if (ImGui.Button("Refresh color"))
            {
                DalamudReflector.GetService("Dalamud.Plugin.Ipc.Internal.DataShare").GetFoP<System.Collections.IDictionary>("caches").Remove("ECommonsPatreonBannerRandomColor");
                ((System.Collections.IDictionary)typeof(EzSharedData).GetFieldPropertyUnion("Cache", ReflectionHelper.AllFlags).GetValue(null)).Remove("ECommonsPatreonBannerRandomColor");
            }
            if (ImGui.Button("Tamper"))
            {
                var randomInt = new Random().NextInt64(0, uint.MaxValue);
                EzSharedData.GetOrCreate<uint[]>("ECommonsPatreonBannerRandomColor", [(uint)randomInt])[0] = (uint)randomInt;
            }
            ImGui.Checkbox("Enable MapEffect logging and manipulating", ref P.config.MapEffectDbg);
            if (P.config.MapEffectDbg)
            {
                ImGui.SetNextItemWidth(300f);
                ImGui.InputText("Header", ref header, 32, ImGuiInputTextFlags.CharsHexadecimal);
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.InputHex("InstanceContentID |", ref sts.InstanceContentID);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.InputHex("+4 uint", ref sts.unk_4);
                ImGui.SetNextItemWidth(50f);
                ImGuiEx.InputHex("+8 byte |", ref sts.unk_8);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50f);
                ImGuiEx.InputHex("+10 byte |", ref sts.unk_10);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50f);
                ImGuiEx.InputHex("+12 byte |", ref sts.unk_12);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(50f);
                ImGuiEx.InputHex("+14 byte |", ref sts.unk_14);
                ImGui.SameLine();
                if (ImGui.Button("Invoke"))
                {
                    Safe(delegate
                    {
                        var h = new List<byte>();
                        foreach (var x in header.Split(2))
                        {
                            h.Add(byte.Parse(x, NumberStyles.HexNumber));
                        }
                        ReceiveMapEffect(h.ToArray(), sts);
                    });
                }
                for (var i = P.config.MapEffectLog.Count - 1; i >= 0; i--)
                {
                    var el = P.config.MapEffectLog[i];
                    ImGuiEx.Text(el.TerritoryType.GetTerritoryName());
                    ImGui.SameLine();
                    if (ImGui.SmallButton($"Delete (hold ctrl)##{i}") && ImGui.GetIO().KeyCtrl)
                    {
                        P.config.MapEffectLog.RemoveAt(i);
                        break;
                    }
                    for (var z = el.structs.Count - 1; z >= 0; z--)
                    {
                        var s = el.structs[z];
                        ImGuiEx.TextCopy($"{s.header.Select(x => $"{x:X2}").Join("")}");
                        ImGui.SameLine();
                        ImGuiEx.TextCopy($"{s.st.InstanceContentID:X8}");
                        ImGui.SameLine();
                        ImGuiEx.TextCopy($"{s.st.unk_4:X8}");
                        ImGui.SameLine();
                        ImGuiEx.TextCopy($"{s.st.unk_8:X2}");
                        ImGui.SameLine();
                        ImGuiEx.TextCopy($"{s.st.unk_10:X2}");
                        ImGui.SameLine();
                        ImGuiEx.TextCopy($"{s.st.unk_12:X2}");
                        ImGui.SameLine();
                        ImGuiEx.TextCopy($"{s.st.unk_14:X2}");
                        ImGui.SameLine();
                        if (Svc.ClientState.TerritoryType == el.TerritoryType && ImGui.SmallButton($"Replay##{i}/{z}"))
                        {
                            //replay
                            ReceiveMapEffect(s.header, s.st);
                        }
                        ImGui.SameLine();
                        if (ImGui.SmallButton($"Delete##{i}/{z}"))
                        {
                            el.structs.RemoveAt(z);
                            break;
                        }
                    }
                    ImGui.Separator();
                }
            }
        }

        static void ReceiveMapEffect(byte[] header, FFXIVIpcMapEffect st)
        {
            var mem = Marshal.AllocHGlobal(16 + sizeof(FFXIVIpcMapEffect));
            GenericHelpers.Safe(delegate
            {
                MemoryHelper.WriteRaw(mem, new byte[16 + sizeof(FFXIVIpcMapEffect)]);
                var svc = Exposed.From(DalamudReflector.GetService("Dalamud.Game.Network.GameNetwork"));
                MemoryHelper.WriteRaw(mem, header);
                *(FFXIVIpcMapEffect*)(mem + 16) = st;
                *(ushort*)(mem + 2) = (ushort)P.config.MapEventOpcode;
                PluginLog.Debug($"Data: {MemoryHelper.ReadRaw(mem, 16+sizeof(FFXIVIpcMapEffect)).Select(x => $"{x:X2}").Join(" ")}");
                var hook = Exposed.From(svc.processZonePacketDownHook);
                var original = Exposed.From(hook.Original);
                MethodInfo del = original.GetType().GetMethod("Invoke");
                del.Invoke(hook.Original, new object[] { (IntPtr)svc.baseAddress, (uint)0, mem });
            });
            Marshal.FreeHGlobal(mem);
        }
    }
}
