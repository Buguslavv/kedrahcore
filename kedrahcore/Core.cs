using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah {
    public class Core {
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
        Tibia.Packets.HookProxy proxy = null;

        // Hardek Modules
        HModules modules;

        // Timers

        #endregion

        #region Constructor

        /// <summary>
        /// Core constructor.
        /// </summary>
        public Core()
            : this("Kedrah Core") {
        }

        /// <summary>
        /// Core constructor.
        /// </summary>
        public Core(string clientChooserTitle) {
            /* Instantiate modules */
            modules = new HModules(this);

            /* Instantiate timers */

            /* Enable Keyboard/Mouse Hook */
            Tibia.KeyboardHook.Enable();
            Tibia.MouseHook.Enable();

            do {
                Tibia.Util.ClientChooserOptions clientChooserOptions = new Tibia.Util.ClientChooserOptions();
                clientChooserOptions.Title = clientChooserTitle;
                clientChooserOptions.ShowOTOption = true;
                client = Tibia.Util.ClientChooser.ShowBox(clientChooserOptions);

                if (client != null) {
                    kedrahMutex = new System.Threading.Mutex(true, "Kedrah_" + client.Process.Id.ToString());

                    if (!kedrahMutex.WaitOne(0, false)) {
                        client = null;
                        continue;
                    }

                    client.Dll.InitializePipe();
                    proxy = new Tibia.Packets.HookProxy(client);
                    proxy.ReceivedSelfAppearIncomingPacket += new Tibia.Packets.ProxyBase.IncomingPacketListener(OnLogin);
                    proxy.ReceivedLogoutOutgoingPacket += new Tibia.Packets.ProxyBase.OutgoingPacketListener(OnLogout);

                    if (client.LoggedIn)
                        OnLogin(null);
                }

                break;
            } while (client == null);
        }

        #endregion

        #region Get/Set Objects

        public Tibia.Objects.Client Client {
            get {
                if (client == null || client.HasExited)
                    Environment.Exit(1);

                return client;
            }
            set {
                client = value;
            }
        }

        public Tibia.Objects.Player Player {
            get {
                return player;
            }
            set {
                player = value;
            }
        }

        public Tibia.Objects.Map Map {
            get {
                return map;
            }
            set {
                map = value;
            }
        }

        public Tibia.Objects.Screen Screen {
            get {
                return screen;
            }
            set {
                screen = value;
            }
        }

        public Tibia.Objects.BattleList BattleList {
            get {
                return battleList;
            }
            set {
                battleList = value;
            }
        }

        public Tibia.Objects.Inventory Inventory {
            get {
                return inventory;
            }
            set {
                inventory = value;
            }
        }

        public Tibia.Packets.HookProxy Proxy {
            get {
                return proxy;
            }
            set {
                proxy = value;
            }
        }

        public Tibia.Objects.Console Console {
            get {
                return console;
            }
            set {
                console = value;
            }
        }

        public HModules Modules {
            get {
                return modules;
            }
            set {
                modules = value;
            }
        }

        #endregion

        #region Core Functions

        bool OnLogin(Tibia.Packets.IncomingPacket packet) {
            map = new Tibia.Objects.Map(client);
            screen = new Tibia.Objects.Screen(client);
            battleList = new Tibia.Objects.BattleList(client);
            inventory = new Tibia.Objects.Inventory(client);
            console = new Tibia.Objects.Console(client);
            System.Threading.Thread.Sleep(300);
            player = client.GetPlayer();
            Modules.Enable();

            return true;
        }

        bool OnLogout(Tibia.Packets.OutgoingPacket packet) {
            Modules.Disable();

            if (client.Window.WorldOnlyView)
                client.Window.WorldOnlyView = false;

            return true;
        }

        void Close() {
            client.Dll.DisconnectPipe();
        }

        #endregion

        #region Timers

        #endregion
    }
}
