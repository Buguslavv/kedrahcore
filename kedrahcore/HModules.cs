using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kedrah.Constants;
using Kedrah.Modules;

namespace Kedrah
{
    public class HModules
    {
        #region Objects/Variables

        public Cavebot Cavebot;
        public General General;
        public Heal Heal;
        public Looter Looter;
        public Targeting Targeting;
        public WaitStatus WaitStatus;

        #endregion

        #region Constructor

        public HModules(Core core)
        {
            WaitStatus = WaitStatus.Idle;
            Cavebot = new Cavebot(ref core);
            General = new General(ref core);
            Heal = new Heal(ref core);
            Looter = new Looter(ref core);
            Targeting = new Targeting(ref core);
        }

        #endregion

        #region HModules Functions

        internal void Enable()
        {
            Cavebot.Enable();
            General.Enable();
            Heal.Enable();
            Looter.Enable();
            Targeting.Enable();
        }

        internal void Disable()
        {
            Cavebot.Disable();
            General.Disable();
            Heal.Disable();
            Looter.Disable();
            Targeting.Disable();
        }

        #endregion
    }
}
