using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;

namespace SlowMoEvents
{
    internal class onKeyPress : Script
    {
        public onKeyPress()
        {
            KeyUp += onKeyDown;
        }
        void onKeyDown(object sender, KeyEventArgs e)
        {
            //Toggle script
            if (e.KeyCode == Main.tog)
            {
                Main._switch = !Main._switch;
                if (Main._switch)
                {
                    GTA.UI.Screen.ShowHelpText("SlowMo Events Detector ~g~enabled", 1000, true, false);
                }
                else if (!Main._switch)
                {
                    GTA.UI.Screen.ShowHelpText("SlowMo Events Detector ~r~disabled", 1000, true, false);
                }
            }
            //Instant slow mo toggle
            if (e.KeyCode == Main.tog1 && Main._switch && Game.TimeScale == 1.0f)
            {
                Main.SlowMo(true);
            }
        }
    }
}
