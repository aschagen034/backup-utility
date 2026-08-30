namespace BackupUtility
{
    internal class BackupError
    {
        // Identifies the source file that could not be processed.
        public string FilePath { get; set; } = string.Empty;

        // Stores the explanation supplied by Windows/.NET for the failure.
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
