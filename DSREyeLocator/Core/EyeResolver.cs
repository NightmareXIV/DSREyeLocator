using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Network;
using Dalamud.Memory;
using Dalamud.Plugin.Ipc.Exceptions;
using DSREyeLocator.Gui;
using ECommons.GameFunctions;
using ECommons.Logging;
using ECommons.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Core
{
    internal unsafe static class EyeResolver
    {

        internal static long SanctityStartTime = 0;
        internal static long DeathStartTime = 0;
        internal static long SanctityStartTimeDelay = 0;
        internal static long DeathStartTimeDelay = 0;
        internal static int EyePos = -1;
        internal static Dictionary<int, Vector2> EyesPositions = new()
        {
            { 0, new(100.00f, 60.00f) },
            { 1, new(128.28f, 71.72f) },
            { 2, new(140.00f, 100.00f) },
            { 3, new(128.28f, 128.28f) },
            { 4, new(100.00f, 140.00f) },
            { 5, new(71.72f, 128.28f) },
            { 6, new(60.00f, 100.00f) },
            { 7, new(71.72f, 71.72f) },
        };
        internal const uint KingThordanNameID = 3632;

        internal static void OnMapEffect(long ptr, uint a2, ushort a3, ushort a4)
        {
                
            if (P.config.EyeEnabled && (Svc.ClientState.TerritoryType == DSRTerritory || P.config.Test))
            {
                PluginLog.Debug($"MapEffect: {a2}, {a3}, {a4}");
                if (IsSanctity() || IsDeath())
                {
                    if(EyesPositions.ContainsKey((int)a2))
                    {
                        if(a3 == 1)
                        {
                            EyePos = (int)a2;
                        }
                        else
                        {
                            EyePos = -1;
                        }
                    }
                }
            }
        }


        internal static bool IsSanctity()
        {
            if (SanctityStartTime == 0) return false;
            return Environment.TickCount64 < SanctityStartTime + 30000;
        }

        internal static bool IsDeath()
        {
            if (DeathStartTime == 0) return false;
            return Environment.TickCount64 < DeathStartTime + 40000;
        }

        internal static void EyeTick()
        {
            if (Svc.Condition[ConditionFlag.InCombat])
            {
                foreach (var x in Svc.Objects)
                {
                    if (x is IBattleNpc b && b.IsCasting && b.NameId == KingThordanNameID)
                    {
                        if (SanctityStartTime == 0 && b.CastActionId == 25569) //sanctity of the ward
                        {
                            SanctityStartTime = Environment.TickCount64;
                            if (P.config.Delay) SanctityStartTimeDelay = Environment.TickCount64 + P.config.SanctityDelay;
                        }
                        else if (DeathStartTime == 0 && b.CastActionId == 27538) //death of the heavens
                        {
                            DeathStartTime = Environment.TickCount64;
                            if (P.config.Delay) DeathStartTimeDelay = Environment.TickCount64 + P.config.DeathDelay;
                        }
                    }
                }
                if (((IsSanctity() && Environment.TickCount64 > SanctityStartTimeDelay) || (IsDeath() && Environment.TickCount64 > DeathStartTimeDelay)) && EyesPositions.TryGetValue(EyePos, out var eye)
                    && Svc.Objects.TryGetFirst(x => x is IBattleNpc b && b.NameId == KingThordanNameID
                    && b.IsCharacterVisible(), out var thordan))
                {
                    if (P.config.EnableTether)
                    {
                        Safe(delegate
                        {
                            SplatoonManager.DrawLine(SplatoonManager.Get(), Svc.ClientState.LocalPlayer.Position,
                                eye.ToVector3(), P.config.Color, P.config.Thickness);
                            SplatoonManager.DrawLine(SplatoonManager.Get(), Svc.ClientState.LocalPlayer.Position,
                                thordan.Position, P.config.Color, P.config.Thickness);
                        });
                    }
                    if (P.config.EnableBanner)
                    {
                        P.overlayWindow.Correct = !ConeHandler.IsInCone(eye) && !ConeHandler.IsInCone(thordan.Position.ToVector2());
                    }
                }
            }
            else
            {
                SanctityStartTime = 0;
                DeathStartTime = 0;
            }
        }
    }
}
