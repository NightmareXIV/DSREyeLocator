using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator
{
    internal static class Extension
    {
        internal static Dragon Other(this Dragon d)
        {
            return d == Dragon.Nidhogg ? Dragon.Hraesvelgr : Dragon.Nidhogg;
        }
    }
}
