using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Tibia;
using Tibia.Constants;
using Tibia.Objects;
using Tibia.Packets;
using Tibia.Packets.Incoming;
using Tibia.Util;

namespace Kedrah.Modules
{
    public class General : Module
    {
        #region Variables/Objects

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        private bool talking = false;
        private bool holdingBoostKey = true;
        private bool showNames = false;
        private bool speedBoost = false;
        private int floorOfSpy = 0;
        private Keys spyPlusKey;
        private Keys spyMinusKey;
        private Keys spyCenterKey;
        private string sign = "";

        public bool FullLight = false;
        public bool Reusing = false;
        public bool OpenSmall = false;

        public int MaximumFishes = 0;
        public int MinimumCap = 0;

        #endregion

        #region Constructor/Destructor

        public General(ref Core core)
            : base(ref core)
        {
            Core.Proxy.ReceivedContainerOpenIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedContainerOpenIncomingPacket);

            #region Timers

            Timers.Add("eatFood", new Tibia.Util.Timer(5000, false));
            Timers["eatFood"].Execute += new Tibia.Util.Timer.TimerExecution(EatFood_OnExecute);

            Timers.Add("makeLight", new Tibia.Util.Timer(1000, false));
            Timers["makeLight"].Execute += new Tibia.Util.Timer.TimerExecution(MakeLight_OnExecute);

            Timers.Add("revealFishSpots", new Tibia.Util.Timer(1000, false));
            Timers["revealFishSpots"].Execute += new Tibia.Util.Timer.TimerExecution(RevealFishSpots_OnExecute);

            Timers.Add("fishing", new Tibia.Util.Timer(1000, false));
            Timers["fishing"].Execute += new Tibia.Util.Timer.TimerExecution(Fishing_OnExecute);

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
                Item f;
                f = new Item(Core.Client, 0);
                f.Id = 1066;
                return !f.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath);
            }
            set
            {
                List<uint> Falls = new List<uint>();
                Item f;
                f = new Item(Core.Client, 0);

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
                if (Timers["clickReuse"].State == TimerState.Running)
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
                if (Timers["eatFood"].State == TimerState.Running)
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

        public bool Fishing
        {
            get
            {
                if (Timers["fishing"].State == TimerState.Running)
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
                    PlayTimer("fishing");
                }
                else
                {
                    PauseTimer("fishing");
                }
            }
        }

        public bool FramerateControl
        {
            get
            {
                if (Timers["framerateControl"].State == TimerState.Running)
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
                return Core.Player.Light;
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
                return Core.Player.LightColor;
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
                if (Timers["makeLight"].State == TimerState.Running)
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

        public bool RevealFishSpots
        {
            get
            {
                if (Timers["revealFishSpots"].State == TimerState.Running)
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
                return showNames;
            }
            set
            {
                showNames = value;

                if (value)
                {
                    Core.Client.Map.NameSpyOn();
                }
                else
                {
                    Core.Client.Map.NameSpyOff();
                }
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
                speedBoost = value;
                if (value)
                {
                    KeyboardHook.Add(Keys.Up, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                        {
                            if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive && !KeyboardHook.Control && !KeyboardHook.Alt)
                            {
                                Core.Player.Walk(Direction.Up);
                            }
                        }
                        holdingBoostKey = true;
                        return true;
                    }));
                    KeyboardHook.Add(Keys.Left, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                        {
                            if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive && !KeyboardHook.Control && !KeyboardHook.Alt)
                            {
                                Core.Player.Walk(Direction.Left);
                            }
                        }
                        holdingBoostKey = true;
                        return true;
                    }));
                    KeyboardHook.Add(Keys.Down, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                        {
                            if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive && !KeyboardHook.Control && !KeyboardHook.Alt)
                            {
                                Core.Player.Walk(Direction.Down);
                            }
                        }
                        holdingBoostKey = true;
                        return true;
                    }));
                    KeyboardHook.Add(Keys.Right, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (holdingBoostKey)
                        {
                            if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive && !KeyboardHook.Control && !KeyboardHook.Alt)
                            {
                                Core.Player.Walk(Direction.Right);
                            }
                        }
                        holdingBoostKey = true;
                        return true;
                    }));

                    KeyboardHook.AddKeyUp(Keys.Up, new KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    KeyboardHook.AddKeyUp(Keys.Down, new KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    KeyboardHook.AddKeyUp(Keys.Left, new KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    KeyboardHook.AddKeyUp(Keys.Right, new KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                }
                else
                {
                    KeyboardHook.Remove(Keys.Up);
                    KeyboardHook.Remove(Keys.Down);
                    KeyboardHook.Remove(Keys.Left);
                    KeyboardHook.Remove(Keys.Right);
                }
            }
        }

        public bool StackItems
        {
            get
            {
                if (Timers["stackItems"].State == TimerState.Running)
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
                Item f;
                f = new Item(Core.Client, 0);
                f.Id = 2118;
                return f.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath);
            }
            set
            {
                if (value)
                {
                    List<uint> Fields = new List<uint>();
                    uint i;
                    Item f;
                    f = new Item(Core.Client, 0);

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
                    Item f;
                    f = new Item(Core.Client, 0);

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
                    KeyboardHook.Add(Keys.Enter, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive && !KeyboardHook.Control && !KeyboardHook.Alt)
                        {
                            talking = true;
                            return false;
                        }
                        else
                        {
                            if (Core.Client.LoggedIn)
                                talking = false;
                            return true;
                        }
                    }));

                    #region Walk keys

                    KeyboardHook.Add(Keys.W, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{UP}");

                            if (!KeyboardHook.Control && !KeyboardHook.Alt && SpeedBoost && holdingBoostKey)
                            {
                                Core.Player.Walk(Direction.Up);
                            }

                            holdingBoostKey = true;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.A, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{LEFT}");

                            if (!KeyboardHook.Control && !KeyboardHook.Alt && SpeedBoost && holdingBoostKey)
                            {
                                Core.Player.Walk(Direction.Left);
                            }

                            holdingBoostKey = true;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.S, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{DOWN}");

                            if (!KeyboardHook.Control && !KeyboardHook.Alt && SpeedBoost && holdingBoostKey)
                            {
                                Core.Player.Walk(Direction.Down);
                                holdingBoostKey = true;
                            }

                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{RIGHT}");

                            if (!KeyboardHook.Control && !KeyboardHook.Alt && SpeedBoost && holdingBoostKey)
                            {
                                Core.Player.Walk(Direction.Right);
                            }

                            holdingBoostKey = true;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));

                    KeyboardHook.AddKeyUp(Keys.W, new KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    KeyboardHook.AddKeyUp(Keys.A, new KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    KeyboardHook.AddKeyUp(Keys.S, new KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));
                    KeyboardHook.AddKeyUp(Keys.D, new KeyboardHook.KeyPressed(delegate()
                    {
                        holdingBoostKey = false;
                        return true;
                    }));

                    KeyboardHook.Add(Keys.Q, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{HOME}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.E, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{PGUP}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.Z, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{END}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.X, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{PGDN}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));

                    #endregion

                    #region F Keys

                    KeyboardHook.Add(Keys.D1, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F1}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D2, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F2}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D3, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F3}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D4, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F4}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D5, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F5}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D6, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F6}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D7, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F7}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D8, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F8}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D9, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F9}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.D0, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F10}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.OemMinus, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F11}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));
                    KeyboardHook.Add(Keys.Oemplus, new KeyboardHook.KeyPressed(delegate()
                    {
                        if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive)
                        {
                            SendKeys.Send("{F12}");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }));

                    #endregion

                    foreach (Keys key in Enum.GetValues(typeof(Keys)))
                    {
                        if (key != Keys.Tab && (Char.IsUpper((char)key) || Char.IsWhiteSpace((char)key) || Char.IsDigit((char)key)))
                        {
                            KeyboardHook.Add(key, new KeyboardHook.KeyPressed(delegate()
                            {
                                if (Core.Client.LoggedIn && !talking && Core.Client.Window.IsActive && !KeyboardHook.Control && !KeyboardHook.Alt)
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
                    foreach (Keys key in Enum.GetValues(typeof(Keys)))
                    {
                        if (Char.IsUpper((char)key) || Char.IsWhiteSpace((char)key) || Char.IsDigit((char)key) && key != Keys.Tab)
                        {
                            KeyboardHook.Remove(key);
                        }
                    }

                    KeyboardHook.Remove(Keys.Enter);
                    WASDWalk = false;
                }
            }
        }

        public bool WorldOnlyView
        {
            get
            {
                return Core.Client.Window.WorldOnlyView;
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

                Core.Client.Window.WorldOnlyView = value;
            }
        }

        #endregion

        #region Module Functions

        private bool Proxy_ReceivedContainerOpenIncomingPacket(IncomingPacket packet)
        {
            if (OpenSmall)
            {
                ContainerOpenPacket p = (ContainerOpenPacket)packet;
                List<Item> items = p.Items;
                p.Items = new List<Item>();
                p.Send();
                p.Items = items;
            }

            return true;
        }

        public void ChangeIP(string ip, short port)
        {
            if (ip.Length > 0 && port > 0)
            {
                Core.Client.Login.SetOT(ip, port);
            }
        }

        public override void Enable()
        {
            base.Enable();
        }

        public void EnableLevelSpyKeys()
        {
            EnableLevelSpyKeys(Keys.Add, Keys.Subtract, Keys.Multiply);
        }

        public void EnableLevelSpyKeys(string prefix, string prefixSpy, string prefixCenter)
        {
            EnableLevelSpyKeys(Keys.Add, Keys.Subtract, Keys.Multiply, prefix, prefixSpy, prefixCenter);
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

            KeyboardHook.Add(spyPlusKey, new KeyboardHook.KeyPressed(delegate()
            {
                if (Core.Client.Window.IsActive && Core.Client.LoggedIn)
                {
                    Core.Client.Map.NameSpyOn();
                    Core.Client.Map.FullLightOn();
                    if (Core.Client.Map.LevelSpyOn(floorOfSpy + 1))
                    {
                        floorOfSpy++;
                    }

                    if (floorOfSpy == 0)
                    {
                        Core.Client.Map.LevelSpyOff();

                        if (ShowNames)
                        {
                            Core.Client.Map.NameSpyOn();
                        }
                        else
                        {
                            Core.Client.Map.NameSpyOff();
                        }

                        if (FullLight)
                        {
                            Core.Client.Map.FullLightOn();
                        }
                        else
                        {
                            Core.Client.Map.FullLightOff();
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

                    Core.Client.Statusbar = prefix + prefixSpy + sign + floorOfSpy;
                    return false;
                }
                return true;
            }));

            KeyboardHook.Add(spyMinusKey, new KeyboardHook.KeyPressed(delegate()
            {
                if (Core.Client.Window.IsActive && Core.Client.LoggedIn)
                {
                    if (floorOfSpy == 0 && Core.Player.Z == 7)
                    {
                        Core.Client.Map.LevelSpyOff();
                        Core.Client.Statusbar = prefix + prefixCenter;
                    }
                    else
                    {
                        Core.Client.Map.NameSpyOn();
                        Core.Client.Map.FullLightOn();

                        if (Core.Client.Map.LevelSpyOn(floorOfSpy - 1))
                        {
                            floorOfSpy--;
                        }

                        if (floorOfSpy == 0)
                        {
                            Core.Client.Map.LevelSpyOff();

                            if (ShowNames)
                            {
                                Core.Client.Map.NameSpyOn();
                            }
                            else
                            {
                                Core.Client.Map.NameSpyOff();
                            }

                            if (FullLight)
                            {
                                Core.Client.Map.FullLightOn();
                            }
                            else
                            {
                                Core.Client.Map.FullLightOff();
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

                        Core.Client.Statusbar = prefix + prefixSpy + sign + floorOfSpy;
                    }
                    return false;
                }
                return true;
            }));

            KeyboardHook.Add(spyCenterKey, new KeyboardHook.KeyPressed(delegate()
            {
                if (Core.Client.Window.IsActive && Core.Client.LoggedIn)
                {
                    Core.Client.Map.LevelSpyOff();
                    Core.Client.Statusbar = prefix + prefixCenter;
                    Core.Client.Map.NameSpyOn();
                    Core.Client.Map.FullLightOn();
                    return false;
                }
                return true;
            }));

            #endregion
        }

        public void DisableLevelSpyKeys()
        {
            #region LevelSpy Keys

            KeyboardHook.Remove(spyPlusKey);
            KeyboardHook.Remove(spyMinusKey);
            KeyboardHook.Remove(spyCenterKey);

            #endregion
        }

        public void ReplaceTrees()
        {
            Core.Client.Map.ReplaceTrees();
        }

        public string GetLastMessage()
        {
            return Core.Client.Memory.ReadString(Tibia.Addresses.Client.LastMSGText);
        }

        public override void Disable()
        {
            base.Disable();
        }

        #endregion

        #region Timers

        void EatFood_OnExecute()
        {
            Item food = Core.Client.Inventory.GetItems().FirstOrDefault(i => i.IsInList(ItemLists.Foods.Values));

            if (food != null)
            {
                food.Use();
            }
        }

        void Fishing_OnExecute()
        {
            if (MaximumFishes < 0 && Core.Client.Inventory.CountItems(Items.Food.Fish.Id) >= MaximumFishes)
            {
                return;
            }

            if (Core.Player.Cap <= MinimumCap)
            {
                return;
            }

            if (Core.Client.Inventory.CountItems(Items.Tool.Worms.Id) >= 0)
            {
                return;
            }

            List < Tile > tiles = Core.Client.Map.GetTilesOnSameFloor().Where(delegate(Tile t)
            {
                return (t.ObjectCount == 0 && t.Ground.Id >= Tiles.Water.FishStart &&
                    t.Ground.Id <= Tiles.Water.FishEnd && t.Location.Distance() < 9 &&
                    t.Location.IsShootable());
            }).ToList();
            Core.Client.Inventory.UseItemOnTile(Items.Tool.FishingRod.Id, tiles[(new Random((int)DateTime.Now.Ticks)).Next(0, tiles.Count - 1)]);
        }

        void MakeLight_OnExecute()
        {
            Core.Player.LightColor = LightColor;
            Core.Player.Light = Light;
        }

        void RevealFishSpots_OnExecute()
        {
            foreach (Tile t in Core.Client.Map.GetTilesOnSameFloor())
            {
                if (t.Ground.Id >= Tiles.Water.NoFishStart || t.Ground.Id >= Tiles.Water.NoFishEnd)
                {
                    t.ReplaceGround(5581);
                }
            }
        }

        void FramerateControl_OnExecute()
        {
            if (Core.Client.Window.IsMinimized)
            {
                Core.Client.Window.FPSLimit = 1.5;
            }
            else if (Core.Client.Window.IsActive)
            {
                Core.Client.Window.FPSLimit = 30;
            }
            else
            {
                Core.Client.Window.FPSLimit = 15;
            }
        }

        void StackItems_OnExecute()
        {
            Core.Client.Inventory.Stack();
        }

        void ClickReuse_OnExecute()
        {
            if (GetAsyncKeyState(Keys.RButton) != 0 && Core.Client.ActionState == ActionState.Using)
            {
                Core.Client.Window.ActionStateFreezer = false;
                Core.Client.ActionState = ActionState.None;
            }
            else if (Core.Client.ActionState == ActionState.Using)
            {
                Core.Client.Window.ActionStateFreezer = true;
            }
            else
            {
                Core.Client.Window.ActionStateFreezer = false;
            }
        }

        void WorldOnlyView_OnExecute()
        {
            Core.Client.Window.WorldOnlyView = true;
        }

        #endregion
    }
}
