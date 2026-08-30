namespace BackupUtility
{
    internal partial class BackupHistoryForm : Form
    {
        public BackupHistoryForm(List<BackupHistoryEntry> history)
        {
            InitializeComponent();

            // Display the newest completed backup first.
            foreach (BackupHistoryEntry entry in history.OrderByDescending(entry => entry.CompletedAt))
            {
                historyGrid.Rows.Add(
                    entry.CompletedAt.ToString("g"),
                    entry.FilesScanned,
                    entry.FilesCopied,
                    entry.FilesSkipped,
                    entry.ErrorCount
                );
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
