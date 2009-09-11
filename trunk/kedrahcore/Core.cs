using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Tibia.Packets;
using Tibia;
using Tibia.Util;
using System.Threading;

namespace Kedrah
{
    public class Core
    {
        #region Objects/Variables

        static private System.Threading.Mutex kedrahMutex;

        public Client Client = null;
        public Screen Screen = null;
        public Player Player = null;
        public Map Map = null;
        public BattleList BattleList = null;
        public Inventory Inventory = null;
        public Tibia.Objects.Console Console = null;
        public Proxy Proxy = null;

        public HModules Modules;

        #endregion

        #region Constructor

        public Core()
            : this("Kedrah Core", "", false, false)
        {
        }

        public Core(string clientChooserTitle, string mutexName, bool hookProxy, bool useWPF)
        {
            KeyboardHook.Enable();

            do
            {
                ClientChooserOptions clientChooserOptions = new ClientChooserOptions();
                clientChooserOptions.Title = clientChooserTitle;
                clientChooserOptions.ShowOTOption = true;
                clientChooserOptions.OfflineOnly = true;

                if (useWPF)
                {
                    Client = ClientChooserWPF.ShowBox(clientChooserOptions);
                }
                else
                {
                    Client = ClientChooser.ShowBox(clientChooserOptions);
                }

                if (Client != null)
                {
                    kedrahMutex = new Mutex(true, "Kedrah_" + mutexName + Client.Process.Id.ToString());

                    if (!kedrahMutex.WaitOne(0, false))
                    {
                        Client = null;
                        continue;
                    }

                    System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;

                    Client.IO.StartProxy();
                    Proxy = Client.IO.Proxy;

                    Client.Process.Exited += new EventHandler(ClientClosed);
                    Proxy.PlayerLogin += new EventHandler(OnLogin);
                    Proxy.PlayerLogout += new EventHandler(OnLogout);

                    Modules = new HModules(this);
                    Kedrah.Extensions.Kedrah = this;
                }

                break;
            } while (Client == null);
        }

        #endregion

        #region Core Functions

        private void OnLogin(object sender, EventArgs e)
        {
            Map = Client.Map;
            Screen = Client.Screen;
            BattleList = Client.BattleList;
            Inventory = Client.Inventory;
            Console = Client.Console;
            Thread.Sleep(300);
            Player = Client.GetPlayer();
            Modules.Enable();
        }

        private void OnLogout(object sender, EventArgs e)
        {
            Modules.Disable();

            if (Client.Window.WorldOnlyView)
            {
                Client.Window.WorldOnlyView = false;
            }
        }

        void ClientClosed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        #endregion
    }
}
