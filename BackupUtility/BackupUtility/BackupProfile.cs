namespace BackupUtility
{
    internal class BackupProfile
    {
        // These properties describe the backup configuration that will
        // eventually be saved to and loaded from a JSON file.
        public List<string> SourceFolders { get; set; } = new List<string>();

        public string DestinationPath { get; set; } = string.Empty;

        // This ID will eventually match the marker stored inside the configured
        // backup destination folder. It identifies the destination, not the drive hardware.
        public string DestinationMarkerId { get; set; } = string.Empty;

        // Stores the destination path without its drive letter, such as "Backups".
        // This will let the destination be found again if Windows changes the letter.
        public string DestinationRelativePath { get; set; } = string.Empty;
    }
}
