using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah
{
    public interface IModule
    {
        #region Module Functions

        void Enable();

        void Disable();

        void Play();

        void Pause();

        void PauseTimer(string p);

        void PlayTimer(string p);

        #endregion
    }
}