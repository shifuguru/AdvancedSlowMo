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
        bool onExp;
        bool onCollision;
        bool onPedCollision;
        bool onPedRagdoll;
        public Main()
        {
            loadSettings();
            Tick += onTick;
        }
        void loadSettings()
        {
            onExp = Settings.GetValue<bool>("SETTINGS", "onExp", false);
            onCollision = Settings.GetValue<bool>("SETTINGS", "onCollision", false);
            onPedCollision = Settings.GetValue<bool>("SETTINGS", "onPedCollision", true);
            onPedRagdoll = Settings.GetValue<bool>("SETTINGS", "onPedRagdoll", true);
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
                    if (Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, -1, player.Position.X, player.Position.Y, player.Position.Z, 10f))
                        if(!Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 19 , player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 20, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 21, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 22, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 35, player.Position.X, player.Position.Y, player.Position.Z, 10f))
                        {
                            {
                                //GTA.UI.Screen.ShowHelpText("EXPLOSION");
                                slowMo();
                            }
                        }
                }
                if (onCollision)
                {
                    if (player.IsInVehicle())
                    {
                        Vehicle car = player.CurrentVehicle;
                        if (car.Speed > 25)
                        {
                            if (car.HasCollided)
                            {
                                //GTA.UI.Screen.ShowHelpText("COLLISION PLAYER");
                                slowMo();
                            }
                        }
                    }
                }
                if (onPedCollision)
                {
                    var pedVehs = World.GetNearbyVehicles(player, 25f).Where(v => v.Speed > 10 && v.HasCollided).ToList();
                    foreach(Vehicle v in pedVehs)
                    {
                        if (Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, v))
                        {
                            if (!player.IsInVehicle())
                            {
                                //GTA.UI.Screen.ShowHelpText("PED COLLISION");
                                slowMo();
                            }
                            else
                            {
                                if (player.IsInVehicle() && !v.IsTouching(player.CurrentVehicle))
                                {
                                    //GTA.UI.Screen.ShowHelpText("PED COLLISION");
                                    slowMo();
                                }
                            }
                        }
                       
                    }
                }
                if (onPedRagdoll)
                {
                    var peds = World.GetNearbyPeds(player, 100f).Where(p => p.IsJumping || p.IsInAir || p.IsRagdoll && !p.IsOnFire).ToList();
                    foreach(Ped ped in peds)
                    {
                        if (Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, ped) && !ped.IsDead)
                        {
                           // GTA.UI.Screen.ShowHelpText("PED RAGDOLL");
                            slowMo();
                        }
                    }
                }
            
            }
        }
    }
}
