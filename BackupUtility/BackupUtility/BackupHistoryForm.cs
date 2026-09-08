using System.Text.Json;

namespace BackupUtility
{
    internal partial class BackupHistoryForm : Form
    {
        // Store the history file location so the Refresh and Clear buttons
        // can read from or write to the same JSON file used by MainForm
        private readonly string historyFile;

        /*
            Creates the history window using the entries that MainForm already loaded.
            The file path is saved so this form can refresh or clear the history later.
        */
        public BackupHistoryForm(List<BackupHistoryEntry> history, string historyFile)
        {
            InitializeComponent();
            this.historyFile = historyFile;

            PopulateHistory(history);
        }

        /*
            Rebuilds the read-only history table from a list of backup entries.
            It also selects the newest entry and displays its full details.
        */
        private void PopulateHistory(List<BackupHistoryEntry> history)
        {
            // Remove any old rows and details before rebuilding the display
            historyGrid.Rows.Clear();
            txtDetails.Clear();

            // Display the newest completed backup first
            foreach (BackupHistoryEntry entry in history.OrderByDescending(entry => entry.CompletedAt))
            {
                int rowIndex = historyGrid.Rows.Add(
                    entry.CompletedAt.ToString("g"),
                    entry.FilesScanned,
                    entry.FilesCopied,
                    entry.FilesSkipped,
                    entry.ErrorCount
                );

                // Tag lets each visual row keep a reference to its full history entry
                historyGrid.Rows[rowIndex].Tag = entry;
            }

            if (historyGrid.Rows.Count > 0)
            {
                // Select the first row, which represents the newest backup
                historyGrid.CurrentCell = historyGrid.Rows[0].Cells[0];

                // Retrieve the complete history entry stored in the row's Tag
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

        /*
            Reloads backup history from the JSON file when the user clicks Refresh.
            If the file is missing or empty, the form displays an empty history list.
        */
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

                // Reuse the same population logic used when the form first opens
                PopulateHistory(history);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The backup history could not be refreshed: " + ex.Message);
            }
        }

        /*
            Clears all saved backup history after the user confirms the action.
            Choosing No leaves both the JSON file and the displayed rows unchanged.
        */
        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            // Button2 makes No the default choice to reduce accidental deletion.
            DialogResult confirmation = MessageBox.Show(
                "Permanently clear all backup history?\n\nThis action cannot be undone.",
                "Clear Backup History",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                // Ensure the history folder exists before writing the empty JSON list.
                string? historyDirectory = Path.GetDirectoryName(historyFile);

                if (!string.IsNullOrEmpty(historyDirectory))
                {
                    Directory.CreateDirectory(historyDirectory);
                }

                // Keep a valid but empty JSON array instead of deleting the file.
                File.WriteAllText(historyFile, "[]");
                PopulateHistory(new List<BackupHistoryEntry>());

                MessageBox.Show("Backup history has been cleared.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("The backup history could not be cleared: " + ex.Message);
            }
        }

        /*
            Updates the details area whenever the user selects a different table row.
            The row's Tag contains the complete BackupHistoryEntry for that row.
        */
        private void historyGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (historyGrid.CurrentRow?.Tag is BackupHistoryEntry selectedEntry)
            {
                DisplayEntryDetails(selectedEntry);
            }
        }

        /*
            Formats one backup entry as readable lines containing its time,
            source folders, destination, and any individual file errors.
        */
        private void DisplayEntryDetails(BackupHistoryEntry entry)
        {
            // Build the text one line at a time so lists of folders and errors
            // can be added without creating one large formatted string.
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

            // Assigning Lines displays each string as a separate line in the TextBox.
            txtDetails.Lines = detailLines.ToArray();
        }

        // Close only the history window and return the user to MainForm.
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
