using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArrowMaze.LevelContent
{
    public static class LevelCatalog
    {
        public static List<LevelDefinition> LoadResourcesLevels(string resourcesPath)
        {
            string path = string.IsNullOrWhiteSpace(resourcesPath) ? "Levels" : resourcesPath;
            TextAsset[] assets = Resources.LoadAll<TextAsset>(path);

            return assets
                .Where(asset => asset != null)
                .OrderBy(asset => GetLevelSortKey(asset.name))
                .ThenBy(asset => asset.name)
                .Select(asset => LevelJsonSerializer.FromCurrentJson(asset.text))
                .Where(result => result.IsSuccess)
                .Select(result => result.Value)
                .ToList();
        }

        public static int GetLevelSortKey(string levelName)
        {
            if (string.IsNullOrWhiteSpace(levelName)) return int.MaxValue;

            const string prefix = "Level_";
            if (levelName.StartsWith(prefix) && int.TryParse(levelName.Substring(prefix.Length), out int number))
            {
                return number;
            }

            return int.MaxValue;
        }
    }
}
