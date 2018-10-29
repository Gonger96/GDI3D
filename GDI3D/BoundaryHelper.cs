using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDI3D
{
    static internal class BoundaryHelper
    {
        public static bool IsXOutside(Vector4 v, Vector4 v1, Vector4 v2)
	    {
	    	return (-v.W > v.X && -v1.W > v1.X && -v2.W > v2.X) || (v.X > v.W && v1.X > v1.W && v2.X > v2.W);
	    }

        public static bool IsYOutside(Vector4 v, Vector4 v1, Vector4 v2)
        {
            return (-v.W > v.Y && -v1.W > v1.Y && -v2.W > v2.Y) || (v.Y > v.W && v1.Y > v1.W && v2.Y > v2.W);
        }

        public static bool IsZOutside(Vector4 v, Vector4 v1, Vector4 v2, out bool needsClipping)
        {
            needsClipping = (-v.W > v.Z) || (-v1.W > v1.Z) || (-v2.W > v2.Z) || (v.Z > v.W) || (v1.Z > v1.W) || (v2.Z > v2.W);
            return (-v.W > v.Z && -v1.W > v1.Z && -v2.W > v2.Z) || (v.Z > v.W && v1.Z > v1.W && v2.Z > v2.W);
        }
    }
}
