using System.Threading.Tasks;
using UnityEngine;

namespace elZach.Common
{
    public static class WebTask
    {
        public static async Task Delay(float value)
        {
            float endTime = Time.unscaledTime + value;
            while (Time.unscaledTime < endTime) await Task.Yield();
        }
    }
}
