namespace BackupUtility
{
    internal class BackupDestinationDetectionResult
    {
        public BackupDestinationStatus Status { get; set; } =
            BackupDestinationStatus.NotFound;

        // This contains the destination's current full path only when Status is Connected.
        public string ResolvedDestinationPath { get; set; } = string.Empty;

        // This provides a user-friendly explanation that MainForm can display later.
        public string Message { get; set; } = string.Empty;
    }
}
