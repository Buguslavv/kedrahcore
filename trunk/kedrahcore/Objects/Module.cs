using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah
{
    public partial class Module : IModule
    {
        #region Variables/Objects

        public Core Core;

        public bool Enabled = false, Running = false;
        public Dictionary<string, Tibia.Util.Timer> Timers = new Dictionary<string, Tibia.Util.Timer>();

        #endregion

        #region Constructor/Destructor

        public Module(ref Core core)
        {
            Core = core;
            Disable();
        }

        #endregion

        #region Module Functions

        public virtual void Enable()
        {
            Enabled = true;

            if (Running)
            {
                Play();
            }
        }

        public virtual void Disable()
        {
            foreach (Tibia.Util.Timer timer in Timers.Values)
            {
                if (timer.State == Tibia.Util.TimerState.Running)
                {
                    timer.Pause();
                }
            }

            Enabled = false;
        }

        public virtual void Play()
        {
            Running = true;

            if (Enabled)
            {
                foreach (Tibia.Util.Timer timer in Timers.Values)
                {
                    if (timer.State == Tibia.Util.TimerState.Paused)
                    {
                        timer.Start();
                    }
                }
            }
        }

        public virtual void Pause()
        {
            Running = false;

            foreach (Tibia.Util.Timer timer in Timers.Values)
            {
                if (timer.State == Tibia.Util.TimerState.Running)
                {
                    timer.Pause();
                }
            }
        }

        public void PauseTimer(string p)
        {
            Timers[p].Stop();
        }

        public void PlayTimer(string p)
        {
            if (Enabled)
            {
                Timers[p].Start();
            }
            else
            {
                Timers[p].Pause();
            }
        }

        #endregion

    }
}
