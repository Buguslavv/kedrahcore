using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah {
    public class HModules {
        #region Objects/Variables

        public Kedrah.Modules.Cavebot Cavebot;
        public Kedrah.Modules.General General;
        public Kedrah.Modules.Heal Heal;
        public Kedrah.Modules.Looter Looter;
        public Kedrah.Modules.Targeting Targeting;

        #endregion

        #region Constructor

        public HModules(Core core) {
            Cavebot = new Kedrah.Modules.Cavebot(core);
            General = new Kedrah.Modules.General(core);
            Heal = new Kedrah.Modules.Heal(core);
            Looter = new Kedrah.Modules.Looter(core);
            Targeting = new Kedrah.Modules.Targeting(core);
        }

        #endregion

        #region HModules Functions

        internal void Enable() {
            Cavebot.Enable();
            General.Enable();
            Heal.Enable();
            Looter.Enable();
            Targeting.Enable();
        }

        internal void Disable() {
            Cavebot.Disable();
            General.Disable();
            Heal.Disable();
            Looter.Disable();
            Targeting.Disable();
        }

        #endregion
    }
}
