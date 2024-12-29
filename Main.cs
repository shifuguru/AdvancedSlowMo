using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using Screen = GTA.UI.Screen;
using GTA.Native;

namespace SlowMoEvents
{
    public class Main : Script
    {
        static int currentTime;
        static int newTime;
        static int delay;
        static float gameSpeed;
        static float mod;
        bool onExp;
        bool onCollision;
        bool onPedCollision;
        bool onPedRagdoll;
        public static bool _switch;
        static int coolDown;
        public static Keys tog;
        public static Keys tog1;
        static int length;
        static float transition;
        public Main()
        {
            LoadSettings();
            Tick += OnTick;
        }
        void LoadSettings()
        {
            try
            {
                coolDown = Settings.GetValue<int>("SETTINGS", "coolDown", 5000);
                length = Settings.GetValue<int>("SETTINGS", "length", 110);
                gameSpeed = Settings.GetValue<float>("SETTINGS", "gameSpeed", 0.1f);
                if (gameSpeed > 0.9f || gameSpeed < 0.1f)
                {
                    gameSpeed = 0.1f;
                }
                _switch = Settings.GetValue<bool>("SETTINGS", "activeByDefault", true);
                tog = Settings.GetValue<Keys>("SETTINGS", "toggle", Keys.Delete);
                tog1 = Settings.GetValue<Keys>("SETTINGS", "instantToggle", Keys.NumPad0);
                onExp = Settings.GetValue<bool>("TRIGGERS", "onExp", false);
                onCollision = Settings.GetValue<bool>("TRIGGERS", "onCollision", false);
                onPedCollision = Settings.GetValue<bool>("TRIGGERS", "onPedCollision", true);
                onPedRagdoll = Settings.GetValue<bool>("TRIGGERS", "onPedRagdoll", true);
                transition = Settings.GetValue<float>("SETTINGS", "transition", 0.02f);
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle($"~r~LoadSettings Error: {ex.Message}", 1500);
            }
        }
        public static void SlowMo(bool instant)
        {
            try
            {
                mod = 1.0f;
                int loopLimit = 0;
                while (mod >= gameSpeed && loopLimit < 100)
                {
                    // mod += -transition;
                    mod -= transition;

                    if (mod < gameSpeed)
                    {
                        mod = gameSpeed;
                        Game.TimeScale = mod;
                        break;
                    }
                    Game.TimeScale = mod;
                    // GTA.UI.Screen.ShowSubtitle("mod " + mod, 2000);
                    Wait(10);
                }
                // GTA.UI.Screen.ShowSubtitle("MAX SLOW");
                if (instant)
                {
                    currentTime = Game.GameTime;
                    Wait(currentTime, true);
                }
                else
                {
                    currentTime = Game.GameTime;
                    Wait(currentTime, false);
                }
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle($"~r~SlowMo Error: {ex.Message}", 1500);
            }
        }
        public static void RegularMo()
        {
            try
            {
                mod = gameSpeed;
                while (mod < 1.0f)
                {
                    mod += transition;
                    if (mod > 1.0f)
                    {
                        mod = 1.0f;
                        Game.TimeScale = mod;
                        break;
                    }
                    Game.TimeScale = mod;
                    // GTA.UI.Screen.ShowSubtitle("mod " + mod, 2000);
                    Wait(10);
                }
                // GTA.UI.Screen.ShowSubtitle("REG SPEED");
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle($"~r~RegularMo Error: {ex.Message}", 1500);
            }
        }
        static void Wait(int time, bool instant)
        {
            try
            {
                delay = 0;
                while (delay <= length)
                {
                    newTime = Game.GameTime;
                    delay = newTime - time;
                    if (delay >= length)
                    {
                        RegularMo();
                    }
                    Wait(1);
                    if (!instant)
                    {
                        while (delay < coolDown && delay >= length)
                        {
                            newTime = Game.GameTime;
                            delay = newTime - time;
                            Wait(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle($"~r~Wait Error: {ex.Message}", 1500);
            }
        }

        void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (!_switch) return;

                Ped player = Game.Player.Character;
                if (player == null) return;

                if (Game.TimeScale == 1.0f)
                {
                    if (onExp)
                    {
                        if (Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, -1, player.Position.X, player.Position.Y, player.Position.Z, 10f))
                            if (!Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 19, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 20, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 21, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 22, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 35, player.Position.X, player.Position.Y, player.Position.Z, 10f))
                            {
                                {
                                    //GTA.UI.Screen.ShowHelpText("EXPLOSION");
                                    SlowMo(false);
                                }
                            }
                    }
                    if (onCollision)
                    {
                        if (player.IsInVehicle())
                        {
                            Vehicle car = player.CurrentVehicle;
                            if (car == null) return;

                            if (car.Speed > 25)
                            {
                                if (car.HasCollided)
                                {
                                    //GTA.UI.Screen.ShowHelpText("COLLISION PLAYER");
                                    SlowMo(false);
                                }
                            }
                        }
                    }
                    if (onPedCollision)
                    {
                        var pedVehs = World.GetNearbyVehicles(player, 25f).Where(v => v.Speed > 10 && v.HasCollided).ToList();
                        // null check here? 

                        foreach (Vehicle v in pedVehs)
                        {
                            if (Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, v))
                            {
                                if (!player.IsInVehicle())
                                {
                                    //GTA.UI.Screen.ShowHelpText("PED COLLISION");
                                    SlowMo(false);
                                }
                                else
                                {
                                    if (player.IsInVehicle() && !v.IsTouching(player.CurrentVehicle))
                                    {
                                        // GTA.UI.Screen.ShowHelpText("PED COLLISION");
                                        SlowMo(false);
                                    }
                                }
                            }
                        }
                    }
                    if (onPedRagdoll)
                    {
                        var peds = World.GetNearbyPeds(player, 100f).Where(p => p.IsJumping || p.IsInAir || p.IsRagdoll && !p.IsOnFire).ToList();
                        foreach (Ped ped in peds)
                        {
                            if (Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, ped) && !ped.IsDead)
                            {
                                // GTA.UI.Screen.ShowHelpText("PED RAGDOLL");
                                SlowMo(false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle($"~r~OnTick Error: {ex.Message}", 1500);
            }
        }
    }
}
