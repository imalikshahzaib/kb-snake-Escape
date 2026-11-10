using System.IO;
using ArrowMaze.Common;
using ArrowMaze.LevelContent;
using ArrowMaze.Tools.LevelGeneration;
using UnityEditor;
using UnityEngine;

namespace ArrowMaze.Editor
{
    public static class GeneratedLevelPackMenu
    {
        private const string LevelJsonFolder = "Assets/Resources/Levels";
        private const int DefaultGeneratedCount = 25;

        [MenuItem("ArrowMaze/Generate Original Levels/Generate Next 25 Main Levels")]
        public static void GenerateNext25MainLevels()
        {
            EnsureLevelFolder();

            int firstLevelNumber = FindMaxLevelNumber() + 1;
            int savedCount = 0;
            int failedCount = 0;

            for (int i = 0; i < DefaultGeneratedCount; i++)
            {
                int levelNumber = firstLevelNumber + i;
                if (TryGenerateAndSave(levelNumber))
                {
                    savedCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Generated original level pack. Saved: " + savedCount + ", failed: " + failedCount + ".");
        }

        [MenuItem("ArrowMaze/Generate Original Levels/Generate One Next Level")]
        public static void GenerateOneNextLevel()
        {
            EnsureLevelFolder();

            int levelNumber = FindMaxLevelNumber() + 1;
            bool saved = TryGenerateAndSave(levelNumber);

            AssetDatabase.Refresh();
            if (saved)
            {
                Debug.Log("Generated original level " + levelNumber + ".");
            }
        }

        private static bool TryGenerateAndSave(int levelNumber)
        {
            int seedBase = 73001 + (levelNumber * 7919);
            for (int attempt = 0; attempt < 40; attempt++)
            {
                GeneratedLevelSettings settings = CleanRoomLevelGenerator.CreateSettingsForLevelNumber(levelNumber, seedBase + attempt * 101);
                Result<LevelDefinition> result = CleanRoomLevelGenerator.Generate(settings);
                if (!result.IsSuccess)
                {
                    continue;
                }

                string fileName = "Level_" + levelNumber;
                result.Value.Name = fileName;
                string json = LevelJsonSerializer.ToCurrentJson(result.Value, true);
                string path = Path.Combine(LevelJsonFolder, fileName + ".json").Replace("\\", "/");

                File.WriteAllText(path, json);
                AssetDatabase.ImportAsset(path);

                DifficultyEstimate estimate = DifficultyEstimator.Estimate(result.Value);
                LevelSolvabilityResult solvability = LevelSolvabilityChecker.Check(result.Value);
                Debug.Log(
                    "Saved generated " + fileName +
                    " snakes=" + estimate.SnakeCount +
                    " objects=" + (result.Value.Objects != null ? result.Value.Objects.Count : 0) +
                    " nodes=" + estimate.TotalPathNodes +
                    " difficulty=" + estimate.Difficulty +
                    " solutionSteps=" + solvability.SolutionSnakeIds.Count +
                    " path=" + path);
                return true;
            }

            Debug.LogError("Could not generate a valid original level for Level_" + levelNumber + ".");
            return false;
        }

        private static void EnsureLevelFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(LevelJsonFolder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Levels");
            }
        }

        private static int FindMaxLevelNumber()
        {
            int maxLevel = 0;
            if (!Directory.Exists(LevelJsonFolder))
            {
                return maxLevel;
            }

            string[] files = Directory.GetFiles(LevelJsonFolder, "Level_*.json");
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileNameWithoutExtension(files[i]);
                if (string.IsNullOrEmpty(name) || !name.StartsWith("Level_")) continue;

                string numberPart = name.Substring("Level_".Length);
                int levelNumber;
                if (int.TryParse(numberPart, out levelNumber) && levelNumber > maxLevel)
                {
                    maxLevel = levelNumber;
                }
            }

            return maxLevel;
        }
    }
}
