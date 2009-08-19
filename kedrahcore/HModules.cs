using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah {
    public class HModules {
        #region Objects/Variables

        // Hardek Modules
        Modules.General general;
        Modules.Heal heal;
        Modules.Targeting targeting;

        #endregion

        #region Constructor

        /// <summary>
        /// HModules constructor.
        /// </summary>
        public HModules(Core core) {
            /* Instantiate modules */
            general = new Modules.General(core);
            heal = new Modules.Heal(core);
            targeting = new Modules.Targeting(core);
        }

        #endregion

        #region Get/Set Objects

        public Modules.General General {
            get {
                return general;
            }
            set {
                general = value;
            }
        }

        public Modules.Heal Heal {
            get {
                return heal;
            }
            set {
                heal = value;
            }
        }

        public Modules.Targeting Targeting {
            get {
                return targeting;
            }
            set {
                targeting = value;
            }
        }

        #endregion

        #region HModules Functions

        internal void Enable() {
            General.Enable();
            Heal.Enable();
            Targeting.Enable();
        }

        internal void Disable() {
            General.Disable();
            Heal.Disable();
            Targeting.Disable();
        }

        #endregion
    }
}
