using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace elZach.EditorHelper
{
    public static class Datahandling
    {
        public static string EnsureAssetDataPath(string path)
        {
            path = path.Replace(@"\", "/");
            if (path.Contains(Application.dataPath))
                return "Assets" + path.Substring(Application.dataPath.Length);
            else return path;
        }

        public static string MakeAbsolutePath(string relativePath)
        {
            relativePath = relativePath.Replace(@"\", "/");
            if (relativePath.Contains(Application.dataPath)) return relativePath;
            return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + relativePath;
        }
    }
}
