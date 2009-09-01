using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah {
    public class HModules {
        #region Objects/Variables

        // Hardek Modules
        Kedrah.Modules.Cavebot cavebot;
        Kedrah.Modules.General general;
        Kedrah.Modules.Heal heal;
        Kedrah.Modules.Targeting targeting;

        #endregion

        #region Constructor

        /// <summary>  
        /// HModules constructor.
        /// </summary> 
        public HModules(Core core) {
            /* Instantiate modules */
            cavebot = new Kedrah.Modules.Cavebot(core);
            general = new Kedrah.Modules.General(core);
            heal = new Kedrah.Modules.Heal(core);
            targeting = new Kedrah.Modules.Targeting(core);
        }

        #endregion

        #region Get/Set Objects

        public Kedrah.Modules.Cavebot Cavebot {
            get {
                return cavebot;
            }
            set {
                cavebot = value;
            }
        }

        public Kedrah.Modules.General General {
            get {
                return general;
            }
            set {
                general = value;
            }
        }

        public Kedrah.Modules.Heal Heal {
            get {
                return heal;
            }
            set {
                heal = value;
            }
        }

        public Kedrah.Modules.Targeting Targeting {
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
