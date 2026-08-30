namespace BackupUtility
{
    internal class BackupProfile
    {
        // These properties describe the backup configuration that will
        // eventually be saved to and loaded from a JSON file.
        public List<string> SourceFolders { get; set; } = new List<string>();

        public string DestinationPath { get; set; } = string.Empty;
    }
}
