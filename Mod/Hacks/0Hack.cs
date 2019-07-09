using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Hacks
{
    public abstract class IHack
    {

        public abstract bool IsActivated { get; set; }

        public abstract void drawGUI();


    }
}
