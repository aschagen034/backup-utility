namespace BackupUtility
{
    internal enum BackupDestinationStatus
    {
        // Exactly one destination marker matched the saved profile.
        Connected,

        // No matching destination marker was found on the available drives.
        NotFound,

        // More than one destination contained the same marker ID.
        // BackupUtility must refuse to guess which one should be used.
        MultipleMatches,

        // A marker was found at an expected location, but it could not be read or validated.
        InvalidMarker
    }
}
