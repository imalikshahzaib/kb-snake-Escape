using System;
using System.Collections.Generic;
using System.Linq;

namespace ArrowMaze.LevelContent
{
    public enum LevelValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    [Serializable]
    public sealed class LevelValidationIssue
    {
        public LevelValidationSeverity Severity;
        public string Code;
        public string Message;

        public LevelValidationIssue(LevelValidationSeverity severity, string code, string message)
        {
            Severity = severity;
            Code = code;
            Message = message;
        }
    }

    public sealed class LevelValidationResult
    {
        private readonly List<LevelValidationIssue> issues = new List<LevelValidationIssue>();

        public IReadOnlyList<LevelValidationIssue> Issues
        {
            get { return issues; }
        }

        public bool IsValid
        {
            get { return issues.All(issue => issue.Severity != LevelValidationSeverity.Error); }
        }

        public int ErrorCount
        {
            get { return issues.Count(issue => issue.Severity == LevelValidationSeverity.Error); }
        }

        public int WarningCount
        {
            get { return issues.Count(issue => issue.Severity == LevelValidationSeverity.Warning); }
        }

        public void Add(LevelValidationSeverity severity, string code, string message)
        {
            issues.Add(new LevelValidationIssue(severity, code, message));
        }

        public void AddError(string code, string message)
        {
            Add(LevelValidationSeverity.Error, code, message);
        }

        public void AddWarning(string code, string message)
        {
            Add(LevelValidationSeverity.Warning, code, message);
        }
    }
}
