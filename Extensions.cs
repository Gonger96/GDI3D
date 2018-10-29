using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GDI3D
{
    public static class Extensions
    {
        
        public static Vector3 ToVector3(this Color c)
        {
            return new Vector3(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
        }
    }
}
