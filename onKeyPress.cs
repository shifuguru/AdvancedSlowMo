using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SlowMoEvents
{
    internal class onKeyPress : Script
    {
        public onKeyPress()
        {
            KeyUp += onKeyUp;
        }
        void onKeyUp(object sender, KeyEventArgs e)
        {
            //Toggle detector
            if (e.KeyCode == Main.tog)
            {
                Main._switch = !Main._switch;
                if (Main._switch)
                {
                    GTA.UI.Screen.ShowHelpText("SlowMo events detector ~g~enabled", 1000, true, false);
                }
                else if (!Main._switch)
                {
                    GTA.UI.Screen.ShowHelpText("SlowMo events detector ~r~disabled", 1000, true, false);
                }
            }
            //Instant slow mo toggle
            if (e.KeyCode == Main.tog1 && Main._switch)
            {
                Main.slowMo();
            }
        }
    }
}
