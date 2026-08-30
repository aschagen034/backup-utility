using System.Text.Json;

namespace BackupUtility
{
    internal partial class BackupHistoryForm : Form
    {
        private readonly string historyFile;

        public BackupHistoryForm(List<BackupHistoryEntry> history, string historyFile)
        {
            InitializeComponent();
            this.historyFile = historyFile;

            PopulateHistory(history);
        }

        private void PopulateHistory(List<BackupHistoryEntry> history)
        {
            historyGrid.Rows.Clear();
            txtDetails.Clear();

            // Display the newest completed backup first.
            foreach (BackupHistoryEntry entry in history.OrderByDescending(entry => entry.CompletedAt))
            {
                int rowIndex = historyGrid.Rows.Add(
                    entry.CompletedAt.ToString("g"),
                    entry.FilesScanned,
                    entry.FilesCopied,
                    entry.FilesSkipped,
                    entry.ErrorCount
                );

                // Tag lets each visual row keep a reference to its full history entry.
                historyGrid.Rows[rowIndex].Tag = entry;
            }

            if (historyGrid.Rows.Count > 0)
            {
                historyGrid.CurrentCell = historyGrid.Rows[0].Cells[0];

                if (historyGrid.Rows[0].Tag is BackupHistoryEntry firstEntry)
                {
                    DisplayEntryDetails(firstEntry);
                }
            }
            else
            {
                txtDetails.Text = "No backup history is available.";
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                List<BackupHistoryEntry> history = new List<BackupHistoryEntry>();

                if (File.Exists(historyFile))
                {
                    string json = File.ReadAllText(historyFile);
                    history = JsonSerializer.Deserialize<List<BackupHistoryEntry>>(json)
                        ?? new List<BackupHistoryEntry>();
                }

                // Reuse the same population logic used when the form first opens.
                PopulateHistory(history);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The backup history could not be refreshed: " + ex.Message);
            }
        }

        private void historyGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (historyGrid.CurrentRow?.Tag is BackupHistoryEntry selectedEntry)
            {
                DisplayEntryDetails(selectedEntry);
            }
        }

        private void DisplayEntryDetails(BackupHistoryEntry entry)
        {
            List<string> detailLines = new List<string>
            {
                $"Completed: {entry.CompletedAt:g}",
                "",
                "Source folders:"
            };

            detailLines.AddRange(entry.SourceFolders.Select(folder => $"  {folder}"));
            detailLines.Add("");
            detailLines.Add($"Destination: {entry.DestinationPath}");
            detailLines.Add("");
            detailLines.Add("Failed files:");

            if (entry.FailedFiles.Count == 0)
            {
                detailLines.Add("  No file errors were recorded.");
            }
            else
            {
                foreach (BackupError error in entry.FailedFiles)
                {
                    detailLines.Add($"  File: {error.FilePath}");
                    detailLines.Add($"  Error: {error.ErrorMessage}");
                    detailLines.Add("");
                }
            }

            txtDetails.Lines = detailLines.ToArray();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
