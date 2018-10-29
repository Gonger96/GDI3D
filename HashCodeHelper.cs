using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDI3D
{
    internal static class HashCodeHelper
    {
        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }
    }
}
