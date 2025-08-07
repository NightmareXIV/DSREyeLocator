using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator
{
    internal class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public bool EyeEnabled = true;
        public uint MapEventOpcode = 0x2E0;
        public int VerticalOffset = 100;
        public int HorizontalOffset = 0;
        public float Scale = 1f;
        public bool EnableTether = true;
        public float Thickness = 5f;
        public uint Color = 0xC8FF0E00;
        public bool EnableBanner = true;
        public bool BannerBlink = true;
        [NonSerialized] internal bool Test = false;
        public bool Delay = true;
        public int SanctityDelay = 11000;
        public int DeathDelay = 25000;

        public bool ChainEnabled = false;
        public float ChainThickness = 2f;
        public uint ChainColor = 0xC80000FF;

        public bool WrothFlamesOperational = false;
        public bool WrothFlames = false;
        public bool FlamesOnlySelf = false;
        public string FlamesSelfSpread = "/marking attack <me>";
        public string FlamesSelfStack = "/marking bind <me>";
        public string FlamesSelfNone = "/marking ignore <me>";
        public bool FlamesEmulateDelay = true;
        public bool MarkSpreads = true;
        public bool MarkStacks = true;
        public bool MarkNones = true;
        public bool UseCustomCommands = false;
        public string CustomCommandsSpread = "/marking attack1\n/marking attack2\n/marking attack3\n/marking attack4";
        public string CustomCommandsStack = "/marking bind1\n/marking bind2";
        public string CustomCommandsNone = "/marking ignore1\n/marking ignore2";

        public int ChangelogWindowVer = 0;

        public bool MapEffectDbg = false;
    }
}
