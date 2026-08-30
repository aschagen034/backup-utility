using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupUtility
{
    internal class BackupProgress
    {
        public int ProcessedFiles { get; set; }
        public int TotalFiles { get; set; }
        public int CopiedFiles { get; set; }
        public int SkippedFiles { get; set; }
        public int ErrorCount { get; set; }
        public List<BackupError> FailedFiles { get; set; } = new List<BackupError>();
    }
}
