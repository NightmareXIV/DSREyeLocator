using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Network;
using Dalamud.Memory;
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

        internal static void OnNetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            Safe(delegate
            {
            if (direction == NetworkMessageDirection.ZoneDown)
            {
                if (P.config.MapEffectDbg && opCode == P.config.MapEventOpcode)
                {
                    var list = P.config.MapEffectLog.Last();
                    var eff = *(FFXIVIpcMapEffect*)dataPtr;
                        if (list.TerritoryType == Svc.ClientState.TerritoryType)
                        {
                            var header = MemoryHelper.ReadRaw(dataPtr - 16, 16);
                            list.structs.Add((header, eff));
                        }
                        Svc.Chat.Print(new() { Message = $"{eff.unk_4:X4} {eff.unk_8:X2} {eff.unk_10:X2} {eff.unk_12:X2} {eff.unk_14:X2}", Type = Dalamud.Game.Text.XivChatType.Ls8 });
                        Svc.PluginInterface.SavePluginConfig(P.config);
                    }
                    if (Svc.ClientState.TerritoryType == 838 && P.configWindow.IsOpen && !TabMainConfig.OpcodeFound)
                    {
                        var data = (FFXIVIpcMapEffect*)dataPtr;
                        //80030043, 00080004, 0003, 0000
                        if (data->InstanceContentID == 0x80030043
                            && data->unk_4 == 0x00080004
                            && data->unk_8 == 0x03
                            && data->unk_12 == 0x00)
                        {
                            TabMainConfig.OpcodeFound = true;
                            P.config.MapEventOpcode = opCode;
                            Svc.PluginInterface.SavePluginConfig(P.config);
                        }
                    }
                    if (P.config.EyeEnabled && (Svc.ClientState.TerritoryType == DSRTerritory || P.config.Test) && opCode == P.config.MapEventOpcode)
                    {
                        var data = (FFXIVIpcMapEffect*)dataPtr;
                        PluginLog.Debug($"MapEvent: {data->InstanceContentID:X8}, {data->unk_4:X8}, {data->unk_8:X2}, {data->unk_10:X2}, {data->unk_12:X4}");
                        if (IsSanctity() || IsDeath())
                        {
                            if (data->unk_4 == 0x00020001)
                            {
                                EyePos = data->unk_8;
                                if (!EyesPositions.ContainsKey(EyePos))
                                {
                                    Svc.Chat.PrintError("No data for this eye position was present");
                                    Svc.Chat.PrintError($"MapEvent: {data->InstanceContentID:X8}, {data->unk_4:X8}, {data->unk_8:X2}, {data->unk_10:X2}, {data->unk_12:X4}");
                                }
                            }
                            else if (data->unk_4 == 0x00400020)
                            {
                                EyePos = -1;
                            }
                        }
                    }
                }
            });
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
