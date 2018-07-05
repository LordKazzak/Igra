using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Igra
{
    public class Interrupt
    {
        Interrupt()
        {
            Gl.koncaj = false;
        }

        public void interrupt()
        {
            while (true)
            {
                if (Gl.koncaj == true)
                    return;
            }
        }
    }
}
