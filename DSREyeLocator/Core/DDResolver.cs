using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Core
{
    internal unsafe static class DDResolver
    {
        const uint AkhAfah = 27971;
        static long NextTarget = 0;

        internal static void Tick()
        {
            if(TryGetDragons(out var nid, out var hrae))
            {
                var nidPercent = ((float)nid.CurrentHp / (float)nid.MaxHp) * 100f;
                var hraePercent = ((float)hrae.CurrentHp / (float)hrae.MaxHp) * 100f;
                if (P.config.NoDamageAkhAfah && nid.IsCasting && nid.CastActionId == AkhAfah && nid.CurrentCastTime > 3)
                {
                    if(Svc.Targets.Target != null)
                    {
                        SetTarget(null);
                    }
                }
                else
                {
                    var th = P.config.SwitchTreshold;
                    if(nid.IsCasting && nid.CastActionId == AkhAfah)
                    {
                        th = 2;
                    }
                    //attack nidhogg if nid is higher
                    if(nidPercent - hraePercent > th && Svc.Targets.Target?.Address != nid.Address && nid.H2HRange() < 25f)
                    {
                        SetTarget(nid, 5000);
                    }
                    else if(hraePercent - nidPercent > th && Svc.Targets.Target?.Address != hrae.Address && hrae.H2HRange() < 25f)
                    {
                        SetTarget(hrae, 5000);
                    }
                    else if(P.config.MyDragon == Dragon.Nidhogg && Svc.Targets.Target?.Address != nid.Address && nid.H2HRange() < 25f)
                    {
                        SetTarget(nid);
                    }
                    else if (P.config.MyDragon == Dragon.Hraesvelgr && Svc.Targets.Target?.Address != hrae.Address && hrae.H2HRange() < 25f)
                    {
                        SetTarget(hrae);
                    }
                }
            }
        }

        static float H2HRange(this GameObject target)
        {
            return Vector3.Distance(target.Position, Svc.ClientState.LocalPlayer.Position) - Svc.ClientState.LocalPlayer.HitboxRadius
                - target.HitboxRadius;
        }



        static void SetTarget(BattleNpc bnpc, long addTime = 0)
        {
            if(Environment.TickCount64 > NextTarget && !Svc.Condition[ConditionFlag.Unconscious] && !Svc.Condition[ConditionFlag.BetweenAreas])
            {
                NextTarget = Environment.TickCount64 + new Random().Next(400, 1000) + addTime;
                if (bnpc == null)
                {
                    Svc.Targets.ClearTarget();
                }
                else
                {
                    Svc.Targets.SetTarget(bnpc);
                }
                
            }
        }

        internal static bool TryGetDragons(out BattleNpc nid, out BattleNpc hrae)
        {
            nid = hrae = null;
            foreach (var x in Svc.Objects)
            {
                if(x is BattleNpc bnpc && x.Struct()->GetIsTargetable() && !x.Struct()->IsDead())
                {
                    if (bnpc.Name.ToString() == "Nidhogg")
                    {
                        nid = bnpc;
                    }
                    else if (bnpc.Name.ToString() == "Hraesvelgr")
                    {
                        hrae = bnpc;
                    }
                }
                if (nid != null && hrae != null) return true;
            }
            return false;
        }
    }
}
