using UnityEngine;

namespace elZach.Common
{
    public static class Vector3Extension
    {

        public static Vector3 ZeroY(this Vector3 xyz)
        {
            return new Vector3(xyz.x, 0f, xyz.z);
        }

        public static Vector3 XZDirection(this Vector3 xyz)
        {
            return new Vector3(xyz.x, 0f, xyz.z).normalized;
        }

        public static Vector3 YZDirection(this Vector3 xyz)
        {
            return new Vector3(0f, xyz.y, xyz.z).normalized;
        }

        public static Vector2 xz(this Vector3 xyz)
        {
            return new Vector2(xyz.x, xyz.z);
        }

        public static Vector2 xy(this Vector3 xyz)
        {
            return new Vector2(xyz.x, xyz.y);
        }

        public static bool Approximately(this Vector3 xyz, Vector3 compareTo, float precision = 0.0001f)
        {
            if (Vector3.Distance(xyz, compareTo) >= precision) return false;
            return true;
        }

        public static float GetRandom(this Vector2 range) => UnityEngine.Random.Range(range.x, range.y);
        public static int GetRandom(this Vector2Int range) => UnityEngine.Random.Range(range.x, range.y);
    }
}