using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah
{
    public class Core
    {
        #region Objects/Variables

        /// <sumary>
        /// Instance of the program for the current client
        /// </sumary>
        static private System.Threading.Mutex kedrahMutex;

        // Tibia API Objects
        Tibia.Objects.Client client = null;
        Tibia.Objects.Screen screen = null;
        Tibia.Objects.Player player = null;
        Tibia.Objects.Map map = null;
        Tibia.Objects.BattleList battleList = null;
        Tibia.Objects.Inventory inventory = null;
        Tibia.Objects.Console console = null;

        // Hardek Modules
        HModules modules;

        // Timers
        Tibia.Util.Timer control;

        bool wasLoggedIn = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Core constructor.
        /// </summary>
        public Core()
        {
            /* Instantiate modules */
            modules = new HModules(this);

            /* Instantiate timers */
            control = new Tibia.Util.Timer(100, false);
            control.OnExecute += new Tibia.Util.Timer.TimerExecution(control_OnExecute);

            /* Enable Keyboard Hook */
            Tibia.KeyboardHook.Enable();
            Tibia.MouseHook.Enable();

            foreach (Tibia.Objects.Client c in Tibia.Objects.Client.GetClients())
            {
                kedrahMutex = new System.Threading.Mutex(true, "Kedrah_" + c.Process.Id.ToString());
                if (kedrahMutex.WaitOne(0, false))
                {
                    client = c;
                    break;
                }
            }
        }

        #endregion

        #region Get/Set Objects

        public Tibia.Objects.Client Client
        {
            get
            {
                return client;
            }
            set
            {
                client = value;
            }
        }

        public Tibia.Objects.Player Player
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
            }
        }

        public Tibia.Objects.Map Map
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
            }
        }

        public Tibia.Objects.Screen Screen
        {
            get
            {
                return screen;
            }
            set
            {
                screen = value;
            }
        }

        public Tibia.Objects.BattleList BattleList
        {
            get
            {
                return battleList;
            }
            set
            {
                battleList = value;
            }
        }

        public Tibia.Objects.Inventory Inventory
        {
            get
            {
                return inventory;
            }
            set
            {
                inventory = value;
            }
        }

        public Tibia.Objects.Console Console
        {
            get
            {
                return console;
            }
            set
            {
                console = value;
            }
        }

        public HModules Modules
        {
            get
            {
                return modules;
            }
            set
            {
                modules = value;
            }
        }

        #endregion

        #region Core Functions

        public void Play()
        {
            control.Start();
        }

        #endregion

        #region Timers

        void control_OnExecute()
        {
            if (client != null && client.LoggedIn && !wasLoggedIn)
            {
                wasLoggedIn = true;
                /* Start objects */
                map = new Tibia.Objects.Map(client);
                screen = new Tibia.Objects.Screen(client);
                battleList = new Tibia.Objects.BattleList(client);
                inventory = new Tibia.Objects.Inventory(client);
                console = new Tibia.Objects.Console(client);
                System.Threading.Thread.Sleep(300);
                player = client.GetPlayer();
                Modules.Enable();
            }
            else if (client != null && !client.LoggedIn)
            {
                wasLoggedIn = false;
                Modules.Disable();
                if (client.WorldOnlyView)
                    client.WorldOnlyView = false;
            }
        }

        #endregion
    }
}
