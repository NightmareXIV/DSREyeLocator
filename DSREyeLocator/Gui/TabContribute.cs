using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator.Gui
{
    internal static class TabContribute
    {
        internal static void Draw()
        {
            ImGuiEx.TextWrapped("If you have found this project useful and wish to contribute, you may send some coins to any of these crypto wallets:");
            Donation.PrintDonationInfo();
        }
    }
}
