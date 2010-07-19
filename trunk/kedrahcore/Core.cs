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

        private Tibia.Util.Timer loginChecker = new Tibia.Util.Timer(100, false);

        public Client Client = null;
        public Player Player = null;
        public Proxy Proxy = null;
        public DateTime StartTime = DateTime.Now;
        public double RandomRate = 1.07;

        public HModules Modules;

        #endregion

        #region Constructor

        public Core()
            : this("Kedrah Core", "", false, false)
        {
        }

        public Core(string clientChooserTitle, string mutexName, bool proxy, bool useWPF)
        {
            KeyboardHook.Enable();

            do
            {
                ClientChooserOptions clientChooserOptions = new ClientChooserOptions();
                clientChooserOptions.Title = clientChooserTitle;
                clientChooserOptions.ShowOTOption = true;
                clientChooserOptions.OfflineOnly = proxy;

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
                    Client.Process.Exited += new EventHandler(ClientClosed);

                    if (proxy)
                    {
                        Client.IO.StartProxy();
                        Proxy = Client.IO.Proxy;

                        Proxy.PlayerLogin += new EventHandler(OnLogin);
                        Proxy.PlayerLogout += new EventHandler(OnLogout);
                    }
                    else
                    {
                        loginChecker.Execute += new Tibia.Util.Timer.TimerExecution(loginChecker_Execute);
                        loginChecker.Start();
                    }

                    Modules = new HModules(this);
                    Kedrah.Extensions.Core = this;
                }
            } while (Client == null);
        }

        #endregion

        #region Core Functions

        private void OnLogin(object sender, EventArgs e)
        {
            Thread.Sleep(300);
            Player = Client.GetPlayer();
            Modules.Enable();
            StartTime = DateTime.Now;
        }

        private void OnLogout(object sender, EventArgs e)
        {
            Modules.Disable();

            if (Client.Window.WorldOnlyView)
            {
                Client.Window.WorldOnlyView = false;
            }

            Player = null;
        }

        void ClientClosed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        #endregion

        private void loginChecker_Execute()
        {
            if (Client.LoggedIn && Player == null)
                OnLogin(null, null);
            else if (!Client.LoggedIn && Player != null)
                OnLogout(null, null);
        }
    }
}
