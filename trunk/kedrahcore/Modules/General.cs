using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kedrah.Modules
{
    public class General : Module
    {
        #region Variables/Objects

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        private bool talking = false;
        private bool holdingBoostKey = true;
        private int floorOfSpy = 0;
        private Keys spyPlusKey;
        private Keys spyMinusKey;
        private Keys spyCenterKey;
        private string sign = "";

        public bool showNames = false;
        public bool fullLight = false;
        public bool reusing = false;

        #endregion

        #region Constructor/Destructor

        public General(ref Core core)
            : base(ref core)
        {
            #region Timers

            Timers.Add("eatFood", new Tibia.Util.Timer(5000, false));
            Timers["eatFood"].Execute += new Tibia.Util.Timer.TimerExecution(EatFood_OnExecute);

            Timers.Add("makeLight", new Tibia.Util.Timer(1000, false));
            Timers["makeLight"].Execute += new Tibia.Util.Timer.TimerExecution(MakeLight_OnExecute);

            Timers.Add("revealFishSpots", new Tibia.Util.Timer(1000, false));
            Timers["revealFishSpots"].Execute += new Tibia.Util.Timer.TimerExecution(RevealFishSpots_OnExecute);

            Timers.Add("replaceTrees", new Tibia.Util.Timer(1000, false));
            Timers["replaceTrees"].Execute += new Tibia.Util.Timer.TimerExecution(ReplaceTrees_OnExecute);

            Timers.Add("framerateControl", new Tibia.Util.Timer(500, false));
            Timers["framerateControl"].Execute += new Tibia.Util.Timer.TimerExecution(FramerateControl_OnExecute);

            Timers.Add("stackItems", new Tibia.Util.Timer(300, false));
            Timers["stackItems"].Execute += new Tibia.Util.Timer.TimerExecution(StackItems_OnExecute);

            Timers.Add("clickReuse", new Tibia.Util.Timer(100, false));
            Timers["clickReuse"].Execute += new Tibia.Util.Timer.TimerExecution(ClickReuse_OnExecute);

            Timers.Add("worldOnlyView", new Tibia.Util.Timer(300, false));
            Timers["worldOnlyView"].Execute += new Tibia.Util.Timer.TimerExecution(WorldOnlyView_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool AvoidPitfalls
        {
            get
            {
                Tibia.Objects.Item f;
                f = new Tibia.Objects.Item(Kedrah.Client, 0);
                f.Id = 1066;
                return !f.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath);
            }
            set
            {
                List<uint> Falls = new List<uint>();
                Tibia.Objects.Item f;
                f = new Tibia.Objects.Item(Kedrah.Client, 0);

                Falls.Add(293);
                Falls.Add(475);
                Falls.Add(476);
                Falls.Add(1066);
                if (value)
                {
                    foreach (uint fi in Falls)
                    {
                        f.Id = fi;
                        f.SetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath, true);
                        f.SetFlag(Tibia.Addresses.DatItem.Flag.Floorchange, true);
                        f.AutomapColor = 210;
                    }
                }
                else
                {
                    foreach (uint fi in Falls)
                    {
                        f.Id = fi;
                        f.SetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath, false);
                        f.SetFlag(Tibia.Addresses.DatItem.Flag.Floorchange, false);
                        f.AutomapColor = 24;
                    }
                }
            }
        }

        public bool ClickReuse
        {
            get
            {
                if (Timers["clickReuse"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("clickReuse");
                }
                else
                {
                    PauseTimer("clickReuse");
                }
            }
        }

        public bool EatFood
        {
            get
            {
                if (Timers["eatFood"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("eatFood");
                }
                else
                {
                    PauseTimer("eatFood");
                }
            }
        }

        public bool FramerateControl
        {
            get
            {
                if (Timers["framerateControl"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("framerateControl");
                }
                else
                {
                    PauseTimer("framerateControl");
                }
            }
        }

        public int Light
        {
            get
            {
                return Kedrah.Player.Light;
            }
            set
            {
                Light = value;
            }
        }

        public int LightColor
        {
            get
            {
                return Kedrah.Player.LightColor;
            }
            set
            {
                LightColor = value;
            }
        }

        public bool LightHack
        {
            get
            {
                if (Timers["makeLight"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("makeLight");
                }
                else
                {
                    PauseTimer("makeLight");
                }
            }
        }

        public bool ReplaceTrees
        {
            get
            {
                if (Timers["replaceTrees"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("replaceTrees");
                }
                else
                {
                    PauseTimer("replaceTrees");
                }
            }
        }

        public bool RevealFishSpots
        {
            get
            {
                if (Timers["revealFishSpots"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("revealFishSpots");
                }
                else
                {
                    PauseTimer("revealFishSpots");
                }
            }
        }

        public bool ShowNames
        {
            get
            {
                return ShowNames;
            }
            set
            {
                ShowNames = value;

                if (value)
                {
                    Kedrah.Map.NameSpyOn();
                }
                else
                {
                    Kedrah.Map.NameSpyOff();
                }
            }
        }

        public bool SpeedBoost
        {
            get
            {
                return SpeedBoost;
            }
            set
            {
                if (value)
                {
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Up, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                        {
                            if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                            {
                                Kedrah.Player.Walk(Tibia.Constants.Direction.Up);
                            }
                        }
                        holdingBoostKey = true;
                        return true;
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Left, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                        {
                            if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                            {
                                Kedrah.Player.Walk(Tibia.Constants.Direction.Left);
                            }
                        }
                        holdingBoostKey = true;
                        return true;
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Down, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                        {
                            if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                            {
                                Kedrah.Player.Walk(Tibia.Constants.Direction.Down);
                            }
                        }
                        holdingBoostKey = true;
                        return true;
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Right, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                        {
                            if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                            {
                                Kedrah.Player.Walk(Tibia.Constants.Direction.Right);
                            }
                        }
                        holdingBoostKey = true;
                        return true;
                    }));

                    Tibia.KeyboardHook.AddKeyUp(System.Windows.Forms.Keys.Up, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    Tibia.KeyboardHook.AddKeyUp(System.Windows.Forms.Keys.Down, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    Tibia.KeyboardHook.AddKeyUp(System.Windows.Forms.Keys.Left, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    Tibia.KeyboardHook.AddKeyUp(System.Windows.Forms.Keys.Right, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));

                    SpeedBoost = true;
                }
                else
                {
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Up);
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Down);
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Left);
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Right);
                    SpeedBoost = false;
                }
            }
        }

        public bool StackItems
        {
            get
            {
                if (Timers["stackItems"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("stackItems");
                }
                else
                {
                    PauseTimer("stackItems");
                }
            }
        }

        public bool WalkOverFields
        {
            get
            {
                Tibia.Objects.Item f;
                f = new Tibia.Objects.Item(Kedrah.Client, 0);
                f.Id = 2118;
                return f.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath);
            }
            set
            {
                if (value)
                {
                    List<uint> Fields = new List<uint>();
                    uint i;
                    Tibia.Objects.Item f;
                    f = new Tibia.Objects.Item(Kedrah.Client, 0);

                    for (i = 2118; i <= 2127; i++)
                    {
                        Fields.Add(i);
                    }

                    for (i = 2131; i <= 2135; i++)
                    {
                        Fields.Add(i);
                    }

                    foreach (uint fi in Fields)
                    {
                        f.Id = fi;
                        f.SetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath, false);
                    }
                }
                else
                {
                    List<uint> Fields = new List<uint>();
                    uint i;
                    Tibia.Objects.Item f;
                    f = new Tibia.Objects.Item(Kedrah.Client, 0);

                    for (i = 2118; i <= 2127; i++)
                    {
                        Fields.Add(i);
                    }

                    for (i = 2131; i <= 2135; i++)
                    {
                        Fields.Add(i);
                    }

                    foreach (uint fi in Fields)
                    {
                        f.Id = fi;
                        f.SetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath, true);
                    }
                }
            }
        }

        public bool WASDWalk
        {
            get
            {
                return WASDWalk;
            }
            set
            {
                if (value)
                {
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Enter, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                        {
                            talking = true;
                            return false;
                        }
                        else
                        {
                            if (Kedrah.Client.LoggedIn)
                                talking = false;
                            return true;
                        }
                    }));

                    #region Walk keys

                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.W, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{UP}");
                            
                            if (!Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt && SpeedBoost && holdingBoostKey)
                            {
                                Kedrah.Player.Walk(Tibia.Constants.Direction.Up);
                            }

                            holdingBoostKey = true;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.A, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{LEFT}");

                            if (!Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt && SpeedBoost && holdingBoostKey)
                            {
                                Kedrah.Player.Walk(Tibia.Constants.Direction.Left);
                            }

                            holdingBoostKey = true;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.S, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{DOWN}");

                            if (!Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt && SpeedBoost && holdingBoostKey)
                            {
                                Kedrah.Player.Walk(Tibia.Constants.Direction.Down);
                                holdingBoostKey = true;
                            }

                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{RIGHT}");

                            if (!Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt && SpeedBoost && holdingBoostKey)
                            {
                                Kedrah.Player.Walk(Tibia.Constants.Direction.Right);
                            }

                            holdingBoostKey = true;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));

                    Tibia.KeyboardHook.AddKeyUp(System.Windows.Forms.Keys.W, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    Tibia.KeyboardHook.AddKeyUp(System.Windows.Forms.Keys.A, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    Tibia.KeyboardHook.AddKeyUp(System.Windows.Forms.Keys.S, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    Tibia.KeyboardHook.AddKeyUp(System.Windows.Forms.Keys.D, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));

                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Q, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{HOME}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.E, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{PGUP}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Z, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{END}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.X, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{PGDN}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));

                    #endregion

                    #region F Keys

                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D1, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F1}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D2, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F2}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D3, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F3}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D4, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F4}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D5, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F5}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D6, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F6}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D7, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F7}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D8, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F8}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D9, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F9}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D0, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F10}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.OemMinus, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F11}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Oemplus, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{F12}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));

                    #endregion

                    foreach (System.Windows.Forms.Keys key in Enum.GetValues(typeof(System.Windows.Forms.Keys)))
                    {
                        if (key != System.Windows.Forms.Keys.Tab && (Char.IsUpper((char)key) || Char.IsWhiteSpace((char)key) || Char.IsDigit((char)key)))
                        {
                            Tibia.KeyboardHook.Add(key, new Tibia.KeyboardHook.KeyPressed(delegate()
                            {
                                if (Kedrah.Client.LoggedIn && !talking && Kedrah.Client.Window.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }));
                        }
                    }
                    WASDWalk = true;
                }
                else
                {
                    foreach (System.Windows.Forms.Keys key in Enum.GetValues(typeof(System.Windows.Forms.Keys)))
                    {
                        if (Char.IsUpper((char)key) || Char.IsWhiteSpace((char)key) || Char.IsDigit((char)key) && key != System.Windows.Forms.Keys.Tab)
                        {
                            Tibia.KeyboardHook.Remove(key);
                        }
                    }

                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Enter);
                    WASDWalk = false;
                }
            }
        }

        public bool WorldOnlyView
        {
            get
            {
                return Kedrah.Client.Window.WorldOnlyView;
            }
            set
            {
                if (value)
                {
                    PlayTimer("worldOnlyView");
                }
                else
                {
                    PauseTimer("worldOnlyView");
                }

                Kedrah.Client.Window.WorldOnlyView = value;
            }
        }

        #endregion

        #region Module Functions

        public void ChangeIP(string ip, short port)
        {
            if (ip.Length > 0 && port > 0)
            {
                Kedrah.Client.Login.SetOT(ip, port);
            }
        }

        public override void Enable()
        {
            base.Enable();
        }

        public void EnableLevelSpyKeys()
        {
            EnableLevelSpyKeys(System.Windows.Forms.Keys.Add, System.Windows.Forms.Keys.Subtract, System.Windows.Forms.Keys.Multiply);
        }

        public void EnableLevelSpyKeys(string prefix, string prefixSpy, string prefixCenter)
        {
            EnableLevelSpyKeys(System.Windows.Forms.Keys.Add, System.Windows.Forms.Keys.Subtract, System.Windows.Forms.Keys.Multiply, prefix, prefixSpy, prefixCenter);
        }

        public void EnableLevelSpyKeys(Keys plusKey, Keys minusKey, Keys centerKey)
        {
            EnableLevelSpyKeys(plusKey, minusKey, centerKey, "KedrahCore - ", "LevelSpy Floor = ", "Removing Roofs.");
        }

        public void EnableLevelSpyKeys(Keys plusKey, Keys minusKey, Keys centerKey, string prefix, string prefixSpy, string prefixCenter)
        {
            spyPlusKey = plusKey;
            spyMinusKey = minusKey;
            spyCenterKey = centerKey;

            #region LevelSpy Keys

            Tibia.KeyboardHook.Add(spyPlusKey, new Tibia.KeyboardHook.KeyPressed(delegate()
            {
                if (Kedrah.Client.Window.IsActive && Kedrah.Client.LoggedIn)
                {
                    Kedrah.Map.NameSpyOn();
                    Kedrah.Map.FullLightOn();
                    if (Kedrah.Map.LevelSpyOn(floorOfSpy + 1))
                    {
                        floorOfSpy++;
                    }

                    if (floorOfSpy == 0)
                    {
                        Kedrah.Map.LevelSpyOff();

                        if (showNames)
                        {
                            Kedrah.Map.NameSpyOn();
                        }
                        else
                        {
                            Kedrah.Map.NameSpyOff();
                        }

                        if (fullLight)
                        {
                            Kedrah.Map.FullLightOn();
                        }
                        else
                        {
                            Kedrah.Map.FullLightOff();
                        }
                    }
                    if (floorOfSpy > 0)
                    {
                        sign = "+";
                    }
                    else
                    {
                        sign = "";
                    }

                    Kedrah.Client.Statusbar = prefix + prefixSpy + sign + floorOfSpy;
                    return false;
                }
                return true;
            }));

            Tibia.KeyboardHook.Add(spyMinusKey, new Tibia.KeyboardHook.KeyPressed(delegate()
            {
                if (Kedrah.Client.Window.IsActive && Kedrah.Client.LoggedIn)
                {
                    if (floorOfSpy == 0 && Kedrah.Player.Z == 7)
                    {
                        Kedrah.Map.LevelSpyOff();
                        Kedrah.Client.Statusbar = prefix + prefixCenter;
                    }
                    else
                    {
                        Kedrah.Map.NameSpyOn();
                        Kedrah.Map.FullLightOn();

                        if (Kedrah.Map.LevelSpyOn(floorOfSpy - 1))
                        {
                            floorOfSpy--;
                        }

                        if (floorOfSpy == 0)
                        {
                            Kedrah.Map.LevelSpyOff();

                            if (showNames)
                            {
                                Kedrah.Map.NameSpyOn();
                            }
                            else
                            {
                                Kedrah.Map.NameSpyOff();
                            }

                            if (fullLight)
                            {
                                Kedrah.Map.FullLightOn();
                            }
                            else
                            {
                                Kedrah.Map.FullLightOff();
                            }
                        }
                        if (floorOfSpy > 0)
                        {
                            sign = "+";
                        }
                        else
                        {
                            sign = "";
                        }

                        Kedrah.Client.Statusbar = prefix + prefixSpy + sign + floorOfSpy;
                    }
                    return false;
                }
                return true;
            }));

            Tibia.KeyboardHook.Add(spyCenterKey, new Tibia.KeyboardHook.KeyPressed(delegate()
            {
                if (Kedrah.Client.Window.IsActive && Kedrah.Client.LoggedIn)
                {
                    Kedrah.Map.LevelSpyOff();
                    Kedrah.Client.Statusbar = prefix + prefixCenter;
                    Kedrah.Map.NameSpyOn();
                    Kedrah.Map.FullLightOn();
                    return false;
                }
                return true;
            }));

            #endregion
        }

        public void DisableLevelSpyKeys()
        {
            #region LevelSpy Keys

            Tibia.KeyboardHook.Remove(spyPlusKey);
            Tibia.KeyboardHook.Remove(spyMinusKey);
            Tibia.KeyboardHook.Remove(spyCenterKey);

            #endregion
        }

        public string GetLastMessage()
        {
            return Kedrah.Client.Memory.ReadString(Tibia.Addresses.Client.LastMSGText);
        }

        public override void Disable()
        {
            base.Disable();
        }

        #endregion

        #region Timers

        void EatFood_OnExecute()
        {
            Tibia.Objects.Item food = Kedrah.Inventory.GetItems().FirstOrDefault(i => i.IsInList(Tibia.Constants.ItemLists.Foods.Values));

            if (food != null)
            {
                food.Use();
            }
        }

        void MakeLight_OnExecute()
        {
            Kedrah.Player.LightColor = LightColor;
            Kedrah.Player.Light = Light;
        }

        void RevealFishSpots_OnExecute()
        {

        }

        void ReplaceTrees_OnExecute()
        {
            Kedrah.Map.ReplaceTrees();
        }

        void FramerateControl_OnExecute()
        {
            if (Kedrah.Client.Window.IsMinimized)
            {
                Kedrah.Client.Window.FPSLimit = 1.5;
            }
            else if (Kedrah.Client.Window.IsActive)
            {
                Kedrah.Client.Window.FPSLimit = 30;
            }
            else
            {
                Kedrah.Client.Window.FPSLimit = 15;
            }
        }

        void StackItems_OnExecute()
        {

        }

        void ClickReuse_OnExecute()
        {
            if (GetAsyncKeyState(Keys.RButton) != 0 && Kedrah.Client.ActionState == Tibia.Constants.ActionState.Using)
            {
                Kedrah.Client.Window.ActionStateFreezer = false;
                Kedrah.Client.ActionState = Tibia.Constants.ActionState.None;
            }
            else if (Kedrah.Client.ActionState == Tibia.Constants.ActionState.Using)
            {
                Kedrah.Client.Window.ActionStateFreezer = true;
            }
            else
            {
                Kedrah.Client.Window.ActionStateFreezer = false;
            }
        }

        void WorldOnlyView_OnExecute()
        {
            Kedrah.Client.Window.WorldOnlyView = true;
        }

        #endregion
    }
}
