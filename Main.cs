using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using Screen = GTA.UI.Screen;
using GTA.Native;

namespace SlowMoEvents
{
    public class Main : Script
    {
        public static string LogFilePath = Path.Combine("scripts", "SlowMoEvents.log");
        public static string settingsFilePath = Path.Combine("scripts", "SlowMoEvents.ini");
        public static ScriptSettings settings = ScriptSettings.Load(settingsFilePath);
        public static bool modEnabled;
        public static bool debugEnabled;
        public static int currentTime;
        public static int newTime;
        public static int delay;
        public static float gameSpeed;
        public static float mod;
        public static bool onExp;
        public static bool onCollision;
        public static bool onPedCollision;
        public static bool onPedRagdoll;
        public static int coolDown;
        public static Keys tog;
        public static Keys tog1;
        public static int length;
        public static float transition;

        public Main()
        {
            LoadSettings();
            Tick += OnTick;
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        }
        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (!modEnabled) return;

                Ped player = Game.Player.Character;
                if (player == null) return;

                if (Game.TimeScale == 1.0f)
                {
                    if (onExp)
                    {
                        if (Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, -1, player.Position.X, player.Position.Y, player.Position.Z, 10f))
                        {
                            if (!Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 19, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 20, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 21, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 22, player.Position.X, player.Position.Y, player.Position.Z, 10f) && !Function.Call<bool>(Hash.IS_EXPLOSION_IN_SPHERE, 35, player.Position.X, player.Position.Y, player.Position.Z, 10f))
                            {
                                {
                                    if (debugEnabled)
                                    {
                                        Screen.ShowHelpText("EXPLOSION");
                                    }
                                    SlowMo(false);
                                }
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
                                    if (debugEnabled)
                                    {
                                        Screen.ShowHelpText("COLLISION PLAYER");
                                    }
                                    SlowMo(false);
                                }
                            }
                        }
                    }
                    if (onPedCollision)
                    {
                        var pedVehs = World.GetNearbyVehicles(player, 25f).Where(v => v.Speed > 10 && v.HasCollided).ToList();
                        if (pedVehs.Count <= 0) return;

                        foreach (Vehicle v in pedVehs)
                        {
                            if (Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, v))
                            {
                                if (!player.IsInVehicle())
                                {
                                    if (debugEnabled)
                                    {
                                        Screen.ShowHelpText("PED COLLISION");
                                    }
                                    SlowMo(false);
                                }
                                else
                                {
                                    if (player.IsInVehicle() && !v.IsTouching(player.CurrentVehicle))
                                    {
                                        if (debugEnabled)
                                        {
                                            Screen.ShowHelpText("PED COLLISION");
                                        }
                                        SlowMo(false);
                                    }
                                }
                            }
                        }
                    }
                    if (onPedRagdoll)
                    {
                        var peds = World.GetNearbyPeds(player, 100f).Where(p => p.IsJumping || p.IsInAir || p.IsRagdoll && !p.IsOnFire).ToList();
                        if (peds.Count <= 0) return;

                        foreach (Ped ped in peds)
                        {
                            if (Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, ped) && !ped.IsDead)
                            {
                                if (debugEnabled)
                                {
                                    Screen.ShowHelpText("PED RAGDOLL");
                                }
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

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                {
                    CreateIni(settingsFilePath);
                }

                settings = ScriptSettings.Load(settingsFilePath);

                if (settings == null)
                {
                    Screen.ShowSubtitle("~r~SlowMoEvents Settings Failed to Load!~s~", 1500);
                    return;
                }

                // SETTINGS
                modEnabled = settings.GetValue<bool>("SETTINGS", "Mod Enabled", true);
                debugEnabled = settings.GetValue<bool>("SETTINGS", "Debug Enabled", false);
                tog = settings.GetValue<Keys>("SETTINGS", "Toggle", Keys.End);
                tog1 = settings.GetValue<Keys>("SETTINGS", "Instant Toggle", Keys.NumPad0);
                coolDown = settings.GetValue<int>("SETTINGS", "Cool Down", 5000);
                length = settings.GetValue<int>("SETTINGS", "Length", 110);
                transition = settings.GetValue<float>("SETTINGS", "Transition Length", 0.02f);
                gameSpeed = settings.GetValue<float>("SETTINGS", "Game Speed", 0.1f);
                if (gameSpeed > 0.9f || gameSpeed < 0.1f)
                {
                    gameSpeed = 0.1f;
                }
                // TRIGGERS
                onExp = settings.GetValue<bool>("TRIGGERS", "On Explosion", false);
                onCollision = settings.GetValue<bool>("TRIGGERS", "On Collision", false);
                onPedCollision = settings.GetValue<bool>("TRIGGERS", "On Ped Collision", true);
                onPedRagdoll = settings.GetValue<bool>("TRIGGERS", "On Ped Ragdoll", true);

                SaveSettings();

                if (debugEnabled)
                {
                    Notification.Show("SlowMoEvents Settings Loaded.", true);
                }
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle("SlowMoEvents ~r~LoadSettings Error~s~: " + ex.Message, 1500);
                LogException("SlowMoEvents LoadSettings Error", ex);
            }
        }
        public static void SaveSettings()
        {
            try
            {
                if (settings != null)
                {
                    // SETTINGS
                    settings.SetValue<bool>("SETTINGS", "Mod Enabled", modEnabled);
                    settings.SetValue<bool>("SETTINGS", "Debug Enabled", debugEnabled);
                    settings.SetValue<Keys>("SETTINGS", "Toggle", Keys.End);
                    settings.SetValue<Keys>("SETTINGS", "Instant Toggle", Keys.NumPad0);
                    settings.SetValue<int>("SETTINGS", "Cool Down", 5000);
                    settings.SetValue<int>("SETTINGS", "Length", 110);
                    settings.SetValue<float>("SETTINGS", "Transition Length", 0.02f);
                    settings.SetValue<float>("SETTINGS", "Game Speed", 0.1f);
                    // TRIGGERS
                    settings.SetValue<bool>("TRIGGERS", "On Explosion", false);
                    settings.SetValue<bool>("TRIGGERS", "On Collision", false);
                    settings.SetValue<bool>("TRIGGERS", "On Ped Collision", true);
                    settings.SetValue<bool>("TRIGGERS", "On Ped Ragdoll", true);

                    settings.Save();

                    if (debugEnabled)
                        Notification.Show($"SlowMoEvents ~g~Settings Saved~s~.", true);
                }
                else
                {
                    // Saving Failed!
                    if (debugEnabled)
                        Screen.ShowSubtitle($"SlowMoEvents ~r~Warning!: SlowMoEvents Settings Failed to Save.~s~", 500);
                }
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle("SlowMoEvents ~r~SaveSettings Error~s~: " + ex.Message, 1500);
                LogException("SlowMoEvents SaveSettings Error: ", ex);
            }
        }

        public static void CreateIni(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("[SETTINGS]");
                    // 
                    writer.WriteLine($"Mod Enabled = {modEnabled}");
                    writer.WriteLine($"Debug Enabled = {debugEnabled}");
                    writer.WriteLine($"Toggle = " + Keys.End);
                    writer.WriteLine($"Instant Toggle = " + Keys.NumPad0);
                    writer.WriteLine($"Cool Down = " + 5000);
                    writer.WriteLine($"Length = " + 110);
                    writer.WriteLine($"Game Speed = " + 0.1f);
                    writer.WriteLine($"Transition Length = " + 0.02f);

                    writer.WriteLine("[TRIGGERS]");
                    //
                    writer.WriteLine($"On Explosion = " + false);
                    writer.WriteLine($"On Collision = " + false);
                    writer.WriteLine($"On Ped Collision = " + true);
                    writer.WriteLine($"On Ped Ragdoll = " + true);
                    // writer.WriteLine($"");
                }
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle("SlowMoEvents ~r~CreateIni Error~s~: " + ex.Message, 1500);
                LogException("SlowMoEvents CreateIni Error: ", ex);
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
                    mod -= transition;

                    if (mod < gameSpeed)
                    {
                        mod = gameSpeed;
                        Game.TimeScale = mod;
                        break;
                    }
                    Game.TimeScale = mod;
                    if (debugEnabled)
                    {
                        Screen.ShowSubtitle("mod " + mod, 2000);
                    }

                    Wait(10);
                }
                if (debugEnabled)
                {
                    Screen.ShowSubtitle("MAX SLOW");
                }
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
                Screen.ShowSubtitle("SlowMoEvents ~r~SlowMo Error~s~: " + ex.Message, 1500);
                LogException("SlowMoEvents SlowMo Error: ", ex);
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
                    if (debugEnabled)
                    {
                        Screen.ShowSubtitle("mod " + mod, 2000);
                    }
                    Wait(10);
                }
                if (debugEnabled)
                {
                    Screen.ShowSubtitle("REG SPEED");
                }
            }
            catch (Exception ex)
            {
                Screen.ShowSubtitle("SlowMoEvents ~r~RegularMo Error~s~: " + ex.Message, 1500);
                LogException("SlowMoEvents RegularMo Error: ", ex);
            }
        }
        private static void Wait(int time, bool instant)
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
                Screen.ShowSubtitle("SlowMoEvents ~r~Wait Error~s~: " + ex.Message, 1500);
                LogException("SlowMoEvents Wait Error: ", ex);
            }
        }
        private static void LogException(string message, Exception ex)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"[{DateTime.Now}] {message}");
                    writer.WriteLine(ex.ToString());
                    writer.WriteLine("----------------------------------------------");
                }
            }
            catch
            {
                // Fallback incase logging fails. Avoid throwing further exceptions here.
            }
        }
        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException("Unhandled exception caught", ex);
            }
        }
    }
}
