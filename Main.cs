using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;

namespace AdvancedSlowMo
{
    public class Main : Script
    {
        int time;
        int newTime;
        int delay;
        int i;
        float mod;
        bool onExp = true;
        bool onCollision = false;
        bool onPedCollision = true;
        bool onPedRagdoll = false;
        public Main()
        {
            Tick += onTick;
        }
        void slowMo()
        {
            i = 1;
            while (i <= 10)
            {
                mod = 1.0f / i;
                Game.TimeScale = mod;
                //GTA.UI.Screen.ShowSubtitle("mod " + mod, 2000);
                i++;
                Wait(10);
            }
            time = Game.GameTime;
            wait(time);
        }
        void wait(int time)
        {
            delay = 0;
            while(delay <= 100)
            {
                newTime = Game.GameTime;
                delay = newTime - time;
                
                if (delay >= 100)
                {
                    Game.TimeScale = 1.0f;
                    delay = 100;
                }
                Wait(1);
                while (delay < 2000 && delay >= 100)
                {
                    newTime = Game.GameTime;
                    delay = newTime - time;
                   // GTA.UI.Screen.ShowSubtitle("Cooldown " + delay, 2000);
                    Wait(1);
                }
            }
            
        }
       
        void onTick(object sender, EventArgs e)
        {
            Ped player = Game.Player.Character;
            if (Game.TimeScale == 1.0f)
            {
                if (onExp)
                {
                    if (Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, -1, player.Position.X, player.Position.Y, player.Position.Z, 50f))
                        if(!Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 19 , player.Position.X, player.Position.Y, player.Position.Z, 50f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 20, player.Position.X, player.Position.Y, player.Position.Z, 50f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 21, player.Position.X, player.Position.Y, player.Position.Z, 50f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 22, player.Position.X, player.Position.Y, player.Position.Z, 50f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 35, player.Position.X, player.Position.Y, player.Position.Z, 50f))
                        {
                        slowMo();
                        }
                }
                if (onCollision)
                {
                    if (player.IsInVehicle())
                    {
                        Vehicle car = player.CurrentVehicle;
                        GTA.UI.Screen.ShowHelpText("CAR SPEED" + car.Speed, 1000, false, false);
                        if (car.Speed > 25)
                        {
                            if (car.HasCollided)
                            {
                                slowMo();
                            }
                        }
                    }
                }
                if (onPedCollision)
                {
                    var pedVehs = World.GetNearbyVehicles(player, 25f).Where(v => v.Speed > 10).ToList();
                    foreach(Vehicle v in pedVehs)
                    {
                        if(!player.IsInVehicle() && v.HasCollided)
                        {
                            slowMo();
                        }
                        else
                        {
                            if (v.HasCollided && player.IsInVehicle() && v.IsTouching(player.CurrentVehicle))
                            {

                                slowMo();
                            }
                        }
                    }
                }
                if (onPedRagdoll)
                {
                    var peds = World.GetNearbyPeds(player, 15f).Where(p => p.IsRagdoll).ToList();
                    
                    {
                        slowMo();
                    }
                }
            
            }
        }
    }
}
