using System.IO;
using ArrowMaze.Common;
using ArrowMaze.LevelContent;
using UnityEditor;
using UnityEngine;

namespace ArrowMaze.Editor
{
    public static class GeneratedLevelAuditMenu
    {
        private const string LevelJsonFolder = "Assets/Resources/Levels";
        private const string ReportPath = "Research/From APK/Codex/generated_level_audit_report.md";

        [MenuItem("ArrowMaze/Generate Original Levels/Audit Current Level Pack")]
        public static void AuditCurrentLevelPack()
        {
            if (!Directory.Exists(LevelJsonFolder))
            {
                Debug.LogError("Level folder does not exist: " + LevelJsonFolder);
                return;
            }

            string[] files = Directory.GetFiles(LevelJsonFolder, "Level_*.json");
            System.Array.Sort(files, CompareLevelFileNames);

            int validCount = 0;
            int solvableCount = 0;
            int failedCount = 0;
            var report = new System.Text.StringBuilder();
            report.AppendLine("# Generated Level Audit Report");
            report.AppendLine();
            report.AppendLine("Folder: `" + LevelJsonFolder + "`");
            report.AppendLine();
            report.AppendLine("| File | Valid | Solvable | Snakes | Objects | Nodes | Difficulty | Notes |");
            report.AppendLine("|---|---:|---:|---:|---:|---:|---|---|");

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string fileName = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);
                Result<LevelDefinition> parsed = LevelJsonSerializer.FromCurrentJson(json);
                if (!parsed.IsSuccess)
                {
                    failedCount++;
                    report.AppendLine("| `" + fileName + "` | no | no | 0 | 0 | 0 | n/a | " + parsed.Error + " |");
                    continue;
                }

                LevelValidationResult validation = LevelValidator.Validate(parsed.Value, fileName);
                DifficultyEstimate difficulty = DifficultyEstimator.Estimate(parsed.Value);
                LevelSolvabilityResult solvability = validation.IsValid ? LevelSolvabilityChecker.Check(parsed.Value) : new LevelSolvabilityResult { Error = "Validation failed." };

                if (validation.IsValid) validCount++;
                if (solvability.IsSolvable) solvableCount++;
                if (!validation.IsValid || !solvability.IsSolvable) failedCount++;

                string notes = validation.IsValid ? solvability.Error : validation.ErrorCount + " validation error(s)";
                if (string.IsNullOrEmpty(notes)) notes = "ok";

                report.AppendLine(
                    "| `" + fileName + "` | " +
                    (validation.IsValid ? "yes" : "no") + " | " +
                    (solvability.IsSolvable ? "yes" : "no") + " | " +
                    difficulty.SnakeCount + " | " +
                    (parsed.Value.Objects != null ? parsed.Value.Objects.Count : 0) + " | " +
                    difficulty.TotalPathNodes + " | " +
                    difficulty.Difficulty + " | " +
                    notes + " |");
            }

            report.AppendLine();
            report.AppendLine("Checked: `" + files.Length + "`");
            report.AppendLine("Valid: `" + validCount + "`");
            report.AppendLine("Solvable: `" + solvableCount + "`");
            report.AppendLine("Failed: `" + failedCount + "`");

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString());
            AssetDatabase.Refresh();

            Debug.Log("Level audit complete. Report: " + ReportPath);
        }

        private static int CompareLevelFileNames(string left, string right)
        {
            return ExtractLevelNumber(left).CompareTo(ExtractLevelNumber(right));
        }

        private static int ExtractLevelNumber(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(name) || !name.StartsWith("Level_")) return 0;

            int number;
            return int.TryParse(name.Substring("Level_".Length), out number) ? number : 0;
        }
    }
}
