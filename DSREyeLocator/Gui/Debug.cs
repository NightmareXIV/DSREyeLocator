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
            
        }

    }
}
