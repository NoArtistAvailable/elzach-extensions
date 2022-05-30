using System.Text.RegularExpressions;

namespace elZach.Common
{
    public static class RegexUtility
    {
        private static readonly Regex indexRegex = new Regex(@"[(?<index>\d+)]$", RegexOptions.Compiled);
        private static readonly Regex indexAndListRegex = new Regex(@"(?<listname>\w+)\.Array\.data\[(?<index>\d+)\]$", RegexOptions.Compiled);
        private static readonly Regex propertyFromBackingField = new Regex(@"\<(?<propertyName>\w+)\>", RegexOptions.Compiled);

        public static bool TryGetLastIndex(string path, out int index)
        {
            var matches = indexRegex.Match(path);
            if (matches.Success)
            {
                var res = matches.Groups["index"].Value;
                if (int.TryParse(res, out index)) return true;
            }
            index = -1;
            return false;
        }

        public static bool TryGetLastIndex(string path, out int index, out string propertyName)
        {
            var match = indexAndListRegex.Match(path);
            if (match.Success)
            {
                propertyName = match.Groups["listname"].Value;
                var indexString = match.Groups["index"].Value;
                if (int.TryParse(indexString, out index)) return true;
            }
            index = -1;
            propertyName = null;
            return false;
        }

        public static bool TryGetPropertyNameFromBackingField(string backingField, out string propertyName)
        {
            var match = propertyFromBackingField.Match(backingField);
            propertyName = match.Groups["propertyName"].Value;
            return match.Success;
        }
    }
}