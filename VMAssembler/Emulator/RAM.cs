using System;
using System.Collections.Generic;
using System.Text;

namespace Emulator

{
    class RAM
    {
        public ushort[] data;
        
        public RAM (int size)
        {
            data = new ushort[size];
        }
    }
}
  