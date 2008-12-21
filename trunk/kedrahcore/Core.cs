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

            /* Enable Keyboard Hook */
            Tibia.KeyboardHook.Enable();
            Tibia.MouseHook.Enable();

            foreach (Tibia.Objects.Client c in Tibia.Objects.Client.GetClients())
            {
                kedrahMutex = new System.Threading.Mutex(true, "Kedrah_" + c.Process.Id.ToString());
                if (!c.LoggedIn && kedrahMutex.WaitOne(0, false))
                {
                    client = c;
                    break;
                }
            }
            if (client == null)
            {
                string path = System.IO.Path.Combine(Environment.GetEnvironmentVariable(Environment.SpecialFolder.ProgramFiles.ToString()), @"Tibia\Tibia.exe");
                if (System.IO.File.Exists(path))
                    client = Tibia.Objects.Client.OpenMC(path, "");
                kedrahMutex = new System.Threading.Mutex(true, "Kedrah_" + client.Process.Id.ToString());
            }
            if (client != null)
            {
                // Using proxy for now while hook is not done
                client.StartProxy();
                client.Proxy.PlayerLogin += new Action(OnLogin);
                client.Proxy.PlayerLogout += new Action(OnLogout);
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

        void OnLogin()
        {
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

        void OnLogout()
        {
            Modules.Disable();
            if (client.WorldOnlyView)
                client.WorldOnlyView = false;
        }

        #endregion

        #region Timers

        #endregion
    }
}
