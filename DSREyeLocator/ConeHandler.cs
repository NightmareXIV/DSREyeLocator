using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator
{
    internal static class ConeHandler
    {
        internal static bool IsInCone(Vector2 pos)
        {
            var angle = GetAngleTo(pos);
            return angle > 180 - 46 && angle < 180 + 46;
        }

        internal static float GetAngleTo(Vector2 pos)
        {
            return (MathHelper.GetRelativeAngle(Svc.ClientState.LocalPlayer.Position.ToVector2(), pos) + Svc.ClientState.LocalPlayer.Rotation.RadToDeg()) % 360;
        }
    }
}
