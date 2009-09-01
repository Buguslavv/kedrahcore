using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah {
    public class Core {
        #region Objects/Variables

        static private System.Threading.Mutex kedrahMutex;

        public Tibia.Objects.Client Client = null;
        public Tibia.Objects.Screen Screen = null;
        public Tibia.Objects.Player Player = null;
        public Tibia.Objects.Map Map = null;
        public Tibia.Objects.BattleList BattleList = null;
        public Tibia.Objects.Inventory Inventory = null;
        public Tibia.Objects.Console Console = null;
        public Tibia.Packets.HookProxy Proxy = null;

        public HModules Modules;

        #endregion

        #region Constructor

        public Core()
            : this("Kedrah Core", "", false) {
        }

        public Core(string clientChooserTitle, string mutexName, bool useWPF) {
            Modules = new HModules(this);

            Tibia.KeyboardHook.Enable();
            Tibia.MouseHook.Enable();

            do {
                Tibia.Util.ClientChooserOptions clientChooserOptions = new Tibia.Util.ClientChooserOptions();
                clientChooserOptions.Title = clientChooserTitle;
                clientChooserOptions.ShowOTOption = true;

                if (useWPF)
                    Client = Tibia.Util.ClientChooserWPF.ShowBox(clientChooserOptions);
                else
                    Client = Tibia.Util.ClientChooser.ShowBox(clientChooserOptions);

                if (Client != null) {
                    kedrahMutex = new System.Threading.Mutex(true, "Kedrah_" + mutexName + Client.Process.Id.ToString());

                    if (!kedrahMutex.WaitOne(0, false)) {
                        Client = null;
                        continue;
                    }

                    Proxy = new Tibia.Packets.HookProxy(Client);
                    Client.Process.Exited += new EventHandler(ClientClosed);
                    Proxy.ReceivedSelfAppearIncomingPacket += new Tibia.Packets.ProxyBase.IncomingPacketListener(OnLogin);
                    Proxy.ReceivedLogoutOutgoingPacket += new Tibia.Packets.ProxyBase.OutgoingPacketListener(OnLogout);

                    if (Client.LoggedIn) {
                        try {
                            OnLogin(null);
                        }
                        catch {}
                    }
                }

                break;
            } while (Client == null);
        }

        #endregion

        #region Core Functions

        private bool OnLogin(Tibia.Packets.IncomingPacket packet) {
            Map = new Tibia.Objects.Map(Client);
            Screen = new Tibia.Objects.Screen(Client);
            BattleList = new Tibia.Objects.BattleList(Client);
            Inventory = new Tibia.Objects.Inventory(Client);
            Console = new Tibia.Objects.Console(Client);
            System.Threading.Thread.Sleep(300);
            Player = Client.GetPlayer();
            Modules.Enable();

            return true;
        }

        private bool OnLogout(Tibia.Packets.OutgoingPacket packet) {
            Modules.Disable();

            if (Client.Window.WorldOnlyView)
                Client.Window.WorldOnlyView = false;

            return true;
        }

        void ClientClosed(object sender, EventArgs args) {
            Environment.Exit(0);
        }

        #endregion
    }
}
