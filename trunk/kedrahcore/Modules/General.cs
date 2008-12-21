using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kedrah.Modules
{
    /// <summary>
    /// General functions that are just utilities.
    /// </summary>
    public class General : Module
    {
        #region Variables/Objects

        int floorOfSpy = 0, light, lightColor;
        string sign = "";
        bool showNames = false, fullLight = false, reusing = false, talking = false, wasdWalk = false, speedBoost = false, holdingBoostKey = true;
        Keys spyPlusKey, spyMinusKey, spyCenterKey;

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// General module constructor.
        /// </summary>
        public General(Core core)
            : base(core)
        {
            #region Timers
            
            // Food eater
            timers.Add("eatFood", new Tibia.Util.Timer(5000, false));
            timers["eatFood"].OnExecute += new Tibia.Util.Timer.TimerExecution(eatFood_OnExecute);

            // Make light
            timers.Add("makeLight", new Tibia.Util.Timer(1000, false));
            timers["makeLight"].OnExecute += new Tibia.Util.Timer.TimerExecution(makeLight_OnExecute);

            // Reveal fish spots
            timers.Add("revealFishSpots", new Tibia.Util.Timer(1000, false));
            timers["revealFishSpots"].OnExecute += new Tibia.Util.Timer.TimerExecution(revealFishSpots_OnExecute);

            // Replace trees
            timers.Add("replaceTrees", new Tibia.Util.Timer(1000, false));
            timers["replaceTrees"].OnExecute += new Tibia.Util.Timer.TimerExecution(replaceTrees_OnExecute);

            // Framerate control
            timers.Add("framerateControl", new Tibia.Util.Timer(50, false));
            timers["framerateControl"].OnExecute += new Tibia.Util.Timer.TimerExecution(framerateControl_OnExecute);

            // Stack items
            timers.Add("stackItems", new Tibia.Util.Timer(50, false));
            timers["stackItems"].OnExecute += new Tibia.Util.Timer.TimerExecution(stackItems_OnExecute);

            // Click Reuse
            timers.Add("clickReuse", new Tibia.Util.Timer(1, false));
            timers["clickReuse"].OnExecute += new Tibia.Util.Timer.TimerExecution(clickReuse_OnExecute);
            timers.Add("clickReuseControl", new Tibia.Util.Timer(500, false));
            timers["clickReuseControl"].OnExecute += new Tibia.Util.Timer.TimerExecution(clickReuseControl_OnExecute);

            // World only view
            timers.Add("worldOnlyView", new Tibia.Util.Timer(300, false));
            timers["worldOnlyView"].OnExecute += new Tibia.Util.Timer.TimerExecution(worldOnlyView_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool AvoidPitfalls
        {
            get
            {
                Tibia.Objects.Item f;
                f = new Tibia.Objects.Item(kedrah.Client, 0);
                f.Id = 1066;
                return !f.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath);
            }
            set
            {
                List<uint> Falls = new List<uint>();
                Tibia.Objects.Item f;
                f = new Tibia.Objects.Item(kedrah.Client, 0);

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
                if (timers["clickReuse"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                {
                    if (Tibia.MouseHook.MouseDown == null)
                    {
                        Tibia.MouseHook.MouseDown = null;
                        Tibia.MouseHook.MouseDown += new Tibia.MouseHook.MouseHookHandler(delegate(System.Windows.Forms.MouseButtons buttons)
                        {
                            if (kedrah.Client.IsActive && buttons == System.Windows.Forms.MouseButtons.Right)
                            {
                                reusing = false;
                                kedrah.Client.ActionState = Tibia.Constants.ActionState.None;
                            }
                            return true;
                        });
                        Tibia.MouseHook.MouseDown += null;
                    }
                    PlayTimer("clickReuse");
                    PlayTimer("clickReuseControl");
                }
                else
                {
                    Tibia.MouseHook.MouseDown = null;
                    PauseTimer("clickReuse");
                    PauseTimer("clickReuseControl");
                }
            }
        }

        public bool EatFood
        {
            get
            {
                if (timers["eatFood"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("eatFood");
                else
                    PauseTimer("eatFood");
            }
        }

        public bool FramerateControl
        {
            get
            {
                if (timers["framerateControl"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("framerateControl");
                else
                    PauseTimer("framerateControl");
            }
        }

        public int Light
        {
            get
            {
                return kedrah.Player.Light;
            }
            set
            {
                light = value;
            }
        }

        public int LightColor
        {
            get
            {
                return kedrah.Player.LightColor;
            }
            set
            {
                lightColor = value;
            }
        }

        public bool LightHack
        {
            get
            {
                if (timers["makeLight"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("makeLight");
                else
                    PauseTimer("makeLight");
            }
        }

        public bool ReplaceTrees
        {
            get
            {
                if (timers["replaceTrees"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("replaceTrees");
                else
                    PauseTimer("replaceTrees");
            }
        }

        public bool RevealFishSpots
        {
            get
            {
                if (timers["revealFishSpots"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("revealFishSpots");
                else
                    PauseTimer("revealFishSpots");
            }
        }

        public bool ShowNames
        {
            get
            {
                return showNames;
            }
            set
            {
                showNames = value;
                kedrah.Map.ShowNames(value);
            }
        }

        public bool SpeedBoost
        {
            get
            {
                return speedBoost;
            }
            set
            {
                if (value)
                {
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Up, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                            if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                                kedrah.Player.Walk(Tibia.Constants.WalkDirection.Up);
                        holdingBoostKey = true;
                        return true;
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Left, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                            if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                                kedrah.Player.Walk(Tibia.Constants.WalkDirection.Left);
                        holdingBoostKey = true;
                        return true;
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Down, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                            if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                                kedrah.Player.Walk(Tibia.Constants.WalkDirection.Down);
                        holdingBoostKey = true;
                        return true;
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Right, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                            if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                                kedrah.Player.Walk(Tibia.Constants.WalkDirection.Right);
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

                    speedBoost = true;
                }
                else
                {
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Up);
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Down);
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Left);
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Right);
                    speedBoost = false;
                }
            }
        }

        public bool StackItems
        {
            get
            {
                if (timers["stackItems"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("stackItems");
                else
                    PauseTimer("stackItems");
            }
        }

        public bool WalkOverFields
        {
            get
            {
                Tibia.Objects.Item f;
                f = new Tibia.Objects.Item(kedrah.Client, 0);
                f.Id = 2118;
                return  f.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath);
            }
            set
            {
                if (value)
                {
                    List<uint> Fields = new List<uint>();
                    uint i;
                    Tibia.Objects.Item f;
                    f = new Tibia.Objects.Item(kedrah.Client, 0);

                    for (i = 2118; i <= 2127; i++)
                        Fields.Add(i);
                    for (i = 2131; i <= 2135; i++)
                        Fields.Add(i);

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
                    f = new Tibia.Objects.Item(kedrah.Client, 0);

                    for (i = 2118; i <= 2127; i++)
                        Fields.Add(i);
                    for (i = 2131; i <= 2135; i++)
                        Fields.Add(i);

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
                return wasdWalk;
            }
            set
            {
                if (value)
                {
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.Enter, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
                        {
                            talking = true;
                            return false;
                        }
                        else
                        {
                            if (kedrah.Client.LoggedIn)
                                talking = false;
                            return true;
                        }
                    }));

                    #region Walk keys

                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.W, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{UP}");
                            if (!Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt && speedBoost && holdingBoostKey)
                                kedrah.Player.Walk(Tibia.Constants.WalkDirection.Up);
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{LEFT}");
                            if (!Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt && speedBoost && holdingBoostKey)
                                kedrah.Player.Walk(Tibia.Constants.WalkDirection.Left);
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{DOWN}");
                            if (!Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt && speedBoost && holdingBoostKey)
                                kedrah.Player.Walk(Tibia.Constants.WalkDirection.Down);
                            holdingBoostKey = true;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    Tibia.KeyboardHook.Add(System.Windows.Forms.Keys.D, new Tibia.KeyboardHook.KeyPressed(delegate()
                    {
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
                        {
                            System.Windows.Forms.SendKeys.Send("{RIGHT}");
                            if (!Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt && speedBoost && holdingBoostKey)
                                kedrah.Player.Walk(Tibia.Constants.WalkDirection.Right);
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                        if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive)
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
                                if (kedrah.Client.LoggedIn && !talking && kedrah.Client.IsActive && !Tibia.KeyboardHook.Control && !Tibia.KeyboardHook.Alt)
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
                    wasdWalk = true;
                }
                else
                {
                    foreach (System.Windows.Forms.Keys key in Enum.GetValues(typeof(System.Windows.Forms.Keys)))
                        if (Char.IsUpper((char)key) || Char.IsWhiteSpace((char)key) || Char.IsDigit((char)key) && key != System.Windows.Forms.Keys.Tab)
                            Tibia.KeyboardHook.Remove(key);
                    Tibia.KeyboardHook.Remove(System.Windows.Forms.Keys.Enter);
                    wasdWalk = false;
                }
            }
        }

        public bool WorldOnlyView
        {
            get
            {
                return kedrah.Client.WorldOnlyView;
            }
            set
            {
                if (value)
                    PlayTimer("worldOnlyView");
                else
                    PauseTimer("worldOnlyView");
                kedrah.Client.WorldOnlyView = value;
            }
        }

        #endregion

        #region Module Functions

        /// <summary>
        /// Enables the General module.
        /// </summary>
        public override void Enable()
        {
            base.Enable();            
        }

        /// <summary>
        /// Main wrapper for LevelSpyKeys
        /// </summary>
        public void EnableLevelSpyKeys()
        {
            EnableLevelSpyKeys(System.Windows.Forms.Keys.Add, System.Windows.Forms.Keys.Subtract, System.Windows.Forms.Keys.Multiply);
        }

        /// <summary>
        /// Wrapper for LevelSpyKeys
        /// <param name="prefix"></param>
        /// <param name="prefixSpy"></param>
        /// <param name="prefixCenter"></param>
        /// </summary>
        public void EnableLevelSpyKeys(string prefix, string prefixSpy, string prefixCenter)
        {
            EnableLevelSpyKeys(System.Windows.Forms.Keys.Add, System.Windows.Forms.Keys.Subtract, System.Windows.Forms.Keys.Multiply, prefix, prefixSpy, prefixCenter);
        }

        /// <summary>
        /// Wrapper for LevelSpyKeys
        /// <param name="plusKey"></param>
        /// <param name="minusKey"></param>
        /// <param name="centerKey"></param>
        /// </summary>
        public void EnableLevelSpyKeys(Keys plusKey, Keys minusKey, Keys centerKey)
        {
            EnableLevelSpyKeys(plusKey, minusKey, centerKey, "KedrahCore - ", "LevelSpy Floor = ", "Removing Roofs.");
        }

        /// <summary>
        /// LevelSpy keys function
        /// <param name="plusKey"></param>
        /// <param name="minusKey"></param>
        /// <param name="centerKey"></param>
        /// <param name="prefix"></param>
        /// <param name="prefixSpy"></param>
        /// <param name="prefixCenter"></param>
        /// </summary>
        public void EnableLevelSpyKeys(Keys plusKey, Keys minusKey, Keys centerKey, string prefix, string prefixSpy, string prefixCenter)
        {
            spyPlusKey = plusKey;
            spyMinusKey = minusKey;
            spyCenterKey = centerKey;

            #region LevelSpy Keys

            Tibia.KeyboardHook.Add(spyPlusKey, new Tibia.KeyboardHook.KeyPressed(delegate()
            {
                if (kedrah.Client.IsActive && kedrah.Client.LoggedIn)
                {
                    kedrah.Map.ShowNames(true);
                    kedrah.Map.FullLight(true);
                    if (kedrah.Map.ShowFloor(floorOfSpy + 1, true))
                        floorOfSpy++;
                    if (floorOfSpy == 0)
                    {
                        kedrah.Map.ShowFloor(0, false);
                        kedrah.Map.ShowNames(showNames);
                        kedrah.Map.FullLight(fullLight);
                    }
                    if (floorOfSpy > 0) sign = "+"; else sign = "";
                    kedrah.Client.Statusbar = prefix + prefixSpy + sign + floorOfSpy;
                    return false;
                }
                return true;
            }));

            Tibia.KeyboardHook.Add(spyMinusKey, new Tibia.KeyboardHook.KeyPressed(delegate()
            {
                if (kedrah.Client.IsActive && kedrah.Client.LoggedIn)
                {
                    if (floorOfSpy == 0 && kedrah.Player.Z == 7)
                    {
                        kedrah.Map.ShowFloor(0, true);
                        kedrah.Client.Statusbar = prefix + prefixCenter;
                    }
                    else
                    {
                        kedrah.Map.ShowNames(true);
                        kedrah.Map.FullLight(true);
                        if (kedrah.Map.ShowFloor(floorOfSpy - 1, true))
                            floorOfSpy--;
                        if (floorOfSpy == 0)
                        {
                            kedrah.Map.ShowFloor(0, false);
                            kedrah.Map.ShowNames(showNames);
                            kedrah.Map.FullLight(fullLight);
                        }
                        if (floorOfSpy > 0) sign = "+"; else sign = "";
                        kedrah.Client.Statusbar = prefix + prefixSpy + sign + floorOfSpy;
                    }
                    return false;
                }
                return true;
            }));

            Tibia.KeyboardHook.Add(spyCenterKey, new Tibia.KeyboardHook.KeyPressed(delegate()
            {
                if (kedrah.Client.IsActive && kedrah.Client.LoggedIn)
                {
                    kedrah.Map.ShowFloor(0, true);
                    kedrah.Client.Statusbar = prefix + prefixCenter;
                    kedrah.Map.ShowNames(true);
                    kedrah.Map.FullLight(true);
                    return false;
                }
                return true;
            }));

            #endregion
        }

        /// <summary>
        /// Disables the hotkeys for level spy
        /// </summary>
        public void DisableLevelSpyKeys()
        {
            #region LevelSpy Keys

            // Removes (+) key from hook.
            Tibia.KeyboardHook.Remove(spyPlusKey);
            // Removes (-) key from hook.
            Tibia.KeyboardHook.Remove(spyMinusKey);
            // Removes (*) key from hook.
            Tibia.KeyboardHook.Remove(spyCenterKey);

            #endregion
        }

        public string GetLastMessage()
        {
            return kedrah.Client.ReadString(Tibia.Addresses.Client.LastMSGText);
        }

        /// <summary>
        /// Disables the General module.
        /// </summary>
        public override void Disable()
        {
            base.Disable();
        }

        #endregion

        #region Timers

        void eatFood_OnExecute()
        {
            kedrah.Client.EatFood();
        }

        void makeLight_OnExecute()
        {
            kedrah.Player.LightColor = lightColor;
            kedrah.Player.Light = light;
        }

        void revealFishSpots_OnExecute()
        {
            foreach (uint t in Tibia.Constants.Tiles.Water.GetNoFishIds())
            {
                kedrah.Map.ReplaceTile(t, 622,true);
            }
        }

        void replaceTrees_OnExecute()
        {
            kedrah.Map.ReplaceTrees();
        }

        void framerateControl_OnExecute()
        {
            if (kedrah.Client.IsMinimized)
                kedrah.Client.FPSLimit = 1.5;
            else if (kedrah.Client.IsActive)
                kedrah.Client.FPSLimit = 30;
            else
                kedrah.Client.FPSLimit = 15;
        }

        void stackItems_OnExecute()
        {
            foreach (Tibia.Objects.Container c in kedrah.Inventory.GetContainers())
                foreach (Tibia.Objects.Item i in c.GetItems())
                    if (i.GetFlag(Tibia.Addresses.DatItem.Flag.IsStackable))
                        kedrah.Inventory.Stack(i.Id);
        }

        void clickReuse_OnExecute()
        {
            if (reusing)
                kedrah.Client.ActionState = Tibia.Constants.ActionState.Using;
        }

        void clickReuseControl_OnExecute()
        {
            if (kedrah.Client.ActionState == Tibia.Constants.ActionState.Using)
                reusing = true;
        }

        void worldOnlyView_OnExecute()
        {
            kedrah.Client.WorldOnlyView = true;
        }

        #endregion
    }
}
