﻿using Dalamud.Configuration;
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
        public bool Delay = false;
    }
}
