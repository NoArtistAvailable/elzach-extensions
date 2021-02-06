using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace elZach.EditorHelper
{
    public static class Datahandling
    {
        public static string EnsureAssetDataPath(string path)
        {
            if (path.Contains(Application.dataPath))
                return "Assets" + path.Substring(Application.dataPath.Length);
            else return path;
        }
    }
}
