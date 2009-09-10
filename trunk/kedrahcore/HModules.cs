using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah
{
    public class HModules
    {
        #region Objects/Variables

        public Kedrah.Modules.Cavebot Cavebot;
        public Kedrah.Modules.General General;
        public Kedrah.Modules.Heal Heal;
        public Kedrah.Modules.Looter Looter;
        public Kedrah.Modules.Targeting Targeting;
        public WaitStatus WaitStatus;

        #endregion

        #region Constructor

        public HModules(Core core)
        {
            WaitStatus = WaitStatus.Idle;
            Cavebot = new Kedrah.Modules.Cavebot(ref core);
            General = new Kedrah.Modules.General(ref core);
            Heal = new Kedrah.Modules.Heal(ref core);
            Looter = new Kedrah.Modules.Looter(ref core);
            Targeting = new Kedrah.Modules.Targeting(ref core);
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

    public enum WaitStatus
    {
        Idle,
        OpenBody,
        OpenContainer,
        LootItems
    }
}
