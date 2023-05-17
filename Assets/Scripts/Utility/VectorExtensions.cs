using UnityEngine;

namespace Utility
{
    public static class VectorExtensions
    {
        public static Vector3 OnlyYX(this Vector3 vector3)
        {
            return new Vector3(vector3.y, vector3.x, 0);
        }
    }
}