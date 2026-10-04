using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    public sealed class WorldStreamingValidationIssue
    {
        public WorldStreamingValidationIssue(
            ValidationSeverity severity,
            string code,
            string message,
            UnityEngine.Object context = null,
            string regionId = null)
        {
            Severity = severity;
            Code = code;
            Message = message;
            Context = context;
            RegionId = regionId ?? string.Empty;
        }

        public ValidationSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public UnityEngine.Object Context { get; }
        public string RegionId { get; }
    }
}
