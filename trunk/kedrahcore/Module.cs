using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah {
    public class Module : IModule {
        #region Variables/Objects

        public Core kedrah;

        public bool enabled = false, running = false;
        public Dictionary<string, Tibia.Util.Timer> timers = new Dictionary<string, Tibia.Util.Timer>();

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// General module constructor.
        /// <param name="core"></param>
        /// </summary>
        public Module(Core core) {
            kedrah = core;
            Disable();
        }

        #endregion

        #region Module Functions

        public virtual void Enable() {
            enabled = true;
            if (running)
                Play();
        }

        public virtual void Disable() {
            foreach (Tibia.Util.Timer timer in timers.Values)
                if (timer.State == Tibia.Util.TimerState.Running)
                    timer.Pause();
            enabled = false;
        }

        public virtual void Play() {
            running = true;
            if (enabled)
                foreach (Tibia.Util.Timer timer in timers.Values)
                    if (timer.State == Tibia.Util.TimerState.Paused)
                        timer.Start();
        }

        public virtual void Pause() {
            running = false;
            foreach (Tibia.Util.Timer timer in timers.Values)
                if (timer.State == Tibia.Util.TimerState.Running)
                    timer.Pause();
        }

        public void PauseTimer(string p) {
            timers[p].Stop();
        }

        public void PlayTimer(string p) {
            if (enabled)
                timers[p].Start();
            else
                timers[p].Pause();
        }

        #endregion

    }
}
