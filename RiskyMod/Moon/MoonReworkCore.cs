﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RiskyMod.Moon
{
    public class MoonReworkCore
    {
        public static bool enabled = true;
        public MoonReworkCore()
        {
            if (!enabled) return;
            new LessPillars();
            new PillarsDropItems();
        }
    }
}
