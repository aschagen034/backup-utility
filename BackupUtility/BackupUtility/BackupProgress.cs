using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupUtility
{
    /*
        Stores the current statistics for a backup operation.
        BackupService uses this class to report live progress to MainForm
        and to return the final results after processing finishes.
     */
    internal class BackupProgress
    {
        // Number of files processed so far, including copied, skipped, and failed files.
        public int ProcessedFiles { get; set; }

        // Total number of files discovered across all selected source folders.
        public int TotalFiles { get; set; }

        // Number of new or updated files successfully copied to the destination.
        public int CopiedFiles { get; set; }

        // Number of files skipped because the destination copy was already up to date.
        public int SkippedFiles { get; set; }

        // Number of individual files that could not be processed.
        public int ErrorCount { get; set; }

        // Details about each failed file, including its path and error message.
        public List<BackupError> FailedFiles { get; set; } =
            new List<BackupError>();
    }
}
