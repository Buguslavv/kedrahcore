﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah.Objects
{
    public interface IScript
    {
        void Run(Core core);
        void Stop();
    }
}
