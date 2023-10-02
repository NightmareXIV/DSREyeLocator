using Dalamud.Hooking;
using Reloaded.Hooks.Definitions.X64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CallingConventions = Reloaded.Hooks.Definitions.X64.CallingConventions;

namespace DSREyeLocator
{
    internal unsafe static class Headmarker
    {
        [Function(CallingConventions.Microsoft)]
        delegate IntPtr ActorVfxCreateDelegate2(char* a1, IntPtr a2, IntPtr a3, float a4, char a5, ushort a6, char a7);
        static Hook<ActorVfxCreateDelegate2> ActorVfxCreateHook;
        internal static List<HeadmarkerInfo> HeadmarkerInfos = new();

        internal static void Init()
        {
            var actorVfxCreateAddress = Svc.SigScanner.ScanText("40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8");
            ActorVfxCreateHook = Svc.Hook.HookFromAddress<ActorVfxCreateDelegate2>(actorVfxCreateAddress, ActorVfxNewHandler);
            ActorVfxCreateHook.Enable();
        }

        static unsafe IntPtr ActorVfxNewHandler(char* a1, IntPtr a2, IntPtr a3, float a4, char a5, ushort a6, char a7)
        {
            var vfxPath = Dalamud.Memory.MemoryHelper.ReadString(new IntPtr(a1), Encoding.ASCII, 256);
            var vfx = ActorVfxCreateHook.Original(a1, a2, a3, a4, a5, a6, a7);
            if (Svc.ClientState.TerritoryType == DSRTerritory || P.config.Test)
            {
                if (vfxPath.Contains("lockon"))
                {
                    foreach (var p in Svc.Party)
                    {
                        if (a2 == p.GameObject?.Address)
                        {
                            PluginLog.Debug($"Headmarker {vfxPath} spawned on {p.Name}");
                            HeadmarkerInfos.Add(new()
                            {
                                AppearedAt = Environment.TickCount64,
                                Path = vfxPath,
                                ObjectID = p.GameObject.ObjectId
                            });
                        }
                    }
                }
            }
            return vfx;
        }

        public static void Dispose()
        {
            ActorVfxCreateHook.Disable();
            ActorVfxCreateHook.Dispose();
        }
    }
}
