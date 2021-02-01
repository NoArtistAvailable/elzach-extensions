namespace elZach.Common
{
    public static class FloatExtension
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        public static float Remap(this float value, float from, float to)
        {
            //return (value - 0f) / (1f - 0f) * (to - from) + from;
            return value * (to - from) + from;
        }
    }
}