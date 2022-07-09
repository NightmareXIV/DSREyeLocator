using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Core
{
    internal static class ChainsResolver
    {
        static string[] ChainHeadmarkers = new string[]
        {
            "vfx/lockon/eff/r1fz_firechain_01x.avfx",
            "vfx/lockon/eff/r1fz_firechain_02x.avfx",
            "vfx/lockon/eff/r1fz_firechain_03x.avfx",
            "vfx/lockon/eff/r1fz_firechain_04x.avfx"
        };

        internal static void ChainsTick()
        {
            if(TryFindSelfMarker(out var marker) && ChainHeadmarkers.Contains(marker) && TryFindPartner(marker, out var partner))
            {
                Safe(delegate
                {
                    SplatoonManager.DrawLine(SplatoonManager.Get(), Svc.ClientState.LocalPlayer.Position,
                        partner.Position, P.config.ChainColor, P.config.ChainThickness);
                });
            }
        }

        internal static bool TryFindSelfMarker(out string selfPath)
        {
            foreach(var x in Headmarker.HeadmarkerInfos)
            {
                if(x.ObjectID == Svc.ClientState.LocalPlayer.ObjectId && x.AppearedAt + 10000 > Environment.TickCount64)
                {
                    selfPath = x.Path;
                    return true;
                }
            }
            selfPath = default;
            return false;
        }

        internal static bool TryFindPartner(string path, out GameObject pc)
        {
            foreach (var x in Headmarker.HeadmarkerInfos)
            {
                if (x.ObjectID != Svc.ClientState.LocalPlayer.ObjectId && x.AppearedAt + 10000 > Environment.TickCount64 && x.Path == path
                    && Svc.Party.TryGetFirst(z => z.GameObject?.ObjectId == x.ObjectID, out var pm))
                {
                    pc = pm.GameObject;
                    return true;
                }
            }
            pc = default;
            return false;
        }
    }
}
