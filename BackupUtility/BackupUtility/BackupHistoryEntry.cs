namespace BackupUtility
{
    internal class BackupHistoryEntry
    {
        // One BackupHistoryEntry represents the final result of one backup run.
        public DateTime CompletedAt { get; set; }

        public List<string> SourceFolders { get; set; } = new List<string>();

        public string DestinationPath { get; set; } = string.Empty;

        public int FilesScanned { get; set; }

        public int FilesCopied { get; set; }

        public int FilesSkipped { get; set; }

        public int ErrorCount { get; set; }
    }
}
