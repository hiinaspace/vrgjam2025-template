using System;

namespace Party.Vrg.Jam
{
    public enum ValidationCategory
    {
        Size,
        Bounds,
        References,
        Manifest,
    }

    public enum ValidationSeverity
    {
        Warning,
        Success,
    }

    [Serializable]
    public class ValidationResult
    {
        public ValidationSeverity Severity { get; }
        public ValidationCategory Category { get; }
        public string Message { get; }
        public string Details { get; }

        public ValidationResult(
            ValidationCategory category,
            string message,
            string details = null,
            ValidationSeverity severity = ValidationSeverity.Warning
        )
        {
            Severity = severity;
            Category = category;
            Message = message;
            Details = details;
        }

        public override string ToString()
        {
            return $"[{Category}] {Message}"
                + (string.IsNullOrEmpty(Details) ? "" : $" - {Details}");
        }
    }
}
