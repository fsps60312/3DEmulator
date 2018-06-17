using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DEmulator.Libraries
{
    public static class Events
    {
        public delegate void MyEventHandler<T>(object sender,T value);
    }
}
