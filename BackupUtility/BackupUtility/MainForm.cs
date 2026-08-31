using System.Text.Json;

namespace BackupUtility
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            LoadLastBackupTime();
            LoadBackupProfile();
            destinationStatusTimer.Start();
        }

        private readonly string settingsFile = "backupSettings.json";

        // LocalApplicationData is a stable, per-user location for application files.
        // This avoids relying on whichever folder the program was launched from.
        private readonly string profileFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BackupUtility",
            "backupProfile.json"
        );

        private readonly string historyFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BackupUtility",
            "backupHistory.json"
        );

        // Keeps the destination identity that was last loaded from or saved to the profile.
        private BackupProfile? activeProfile;

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        /*
            This function runs when the user clicks the Add Folder button. The function opens a 
            folder picker, and adds the selected folder path to the list of source folders.
        */
        private void btnAddSource_Click(object sender, EventArgs e)
        {
            // Creates a folder selection dialog
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // Opens the dialog and checks if the user selected a folder
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderDialog.SelectedPath;

                    // Check whether this folder is already in the source list
                    bool alreadyAdded = listSourceFolders.Items
                        .Cast<string>()
                        .Any(folder => folder.Equals(
                            selectedFolder,
                            StringComparison.OrdinalIgnoreCase
                        ));

                    if (alreadyAdded)
                    {
                        MessageBox.Show("This folder has already been added.");
                        return;
                    }

                    // Add the folder if it is not already in the list
                    listSourceFolders.Items.Add(selectedFolder);
                }
            }
        }

        /*
            This function runs when the user clicks the select destination folder button. The function opens a 
            folder picker, gets the path of the selected folder, and displays the path in the corresponding textbox.
         */
        private void btnSelectDestination_Click(object sender, EventArgs e)
        {
            // Creates a folder selection dialog
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // Opens the dialog and checks if the user selected a folder
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    // Displays the selected backup destination in the textbox
                    txtDestinationPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void txtDestinationPath_TextChanged(object sender, EventArgs e)
        {
            // A changed path has not been verified against the saved profile yet.
            ShowDestinationNotConfigured();
        }

        private void destinationStatusTimer_Tick(object sender, EventArgs e)
        {
            // The pre-backup check already protects a running backup. Pausing these
            // display-only checks avoids unnecessary drive scans during processing.
            if (!btnStartBackup.Enabled)
            {
                return;
            }

            if (activeProfile == null ||
                string.IsNullOrWhiteSpace(activeProfile.DestinationMarkerId) ||
                !DestinationTextMatchesActiveProfile())
            {
                ShowDestinationNotConfigured();
                return;
            }

            // A WinForms timer runs ticks one at a time on the UI thread, so checks
            // cannot overlap with one another.
            UpdateDestinationStatus(activeProfile);
        }

        /*
            Removes the currently selected source folder from the ListBox.
            If no folder is selected, show a message and stop the function.
         */
        private void btnRemoveSource_Click(object sender, EventArgs e)
        {
            // Check whether the user selected a folder in the ListBox.
            if (listSourceFolders.SelectedItem == null)
            {
                MessageBox.Show("Please select a folder to remove.");
                return;
            }

            // Remove the selected folder from the list of source folders
            listSourceFolders.Items.Remove(listSourceFolders.SelectedItem);
        }

        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            if (listSourceFolders.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one source folder before saving the profile.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                MessageBox.Show("Please select a destination folder before saving the profile.");
                return;
            }

            try
            {
                SaveBackupProfile();
                MessageBox.Show("Backup profile saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("The backup profile could not be saved: " + ex.Message);
            }
        }

        private void btnViewHistory_Click(object sender, EventArgs e)
        {
            if (!File.Exists(historyFile))
            {
                MessageBox.Show("No backup history is available yet.");
                return;
            }

            try
            {
                string json = File.ReadAllText(historyFile);
                List<BackupHistoryEntry> history =
                    JsonSerializer.Deserialize<List<BackupHistoryEntry>>(json)
                    ?? new List<BackupHistoryEntry>();

                if (history.Count == 0)
                {
                    MessageBox.Show("No backup history is available yet.");
                    return;
                }

                using BackupHistoryForm historyForm = new BackupHistoryForm(history, historyFile);
                historyForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The backup history could not be loaded: " + ex.Message);
            }
        }

        private void SaveBackupProfile()
        {
            string fullDestinationPath = Path.GetFullPath(txtDestinationPath.Text);
            BackupDestinationService destinationService = new BackupDestinationService();

            string markerId = destinationService.GetOrCreateMarkerId(fullDestinationPath);
            string relativePath =
                destinationService.GetDestinationRelativePath(fullDestinationPath);

            BackupProfile profile = new BackupProfile
            {
                SourceFolders = listSourceFolders.Items.Cast<string>().ToList(),
                DestinationPath = fullDestinationPath,
                DestinationMarkerId = markerId,
                DestinationRelativePath = relativePath
            };

            string? profileDirectory = Path.GetDirectoryName(profileFile);

            if (!string.IsNullOrEmpty(profileDirectory))
            {
                Directory.CreateDirectory(profileDirectory);
            }

            // WriteIndented makes the saved JSON easier to read
            // and if the profile file needs to be inspected during testing.
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(profileFile, json);
            activeProfile = profile;
            UpdateDestinationStatus(profile);
        }

        private void LoadBackupProfile()
        {
            // A missing file simply means the user has not saved a profile yet.
            if (!File.Exists(profileFile))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(profileFile);
                BackupProfile? profile = JsonSerializer.Deserialize<BackupProfile>(json);

                if (profile == null)
                {
                    return;
                }

                listSourceFolders.Items.Clear();

                // Distinct uses the same case-insensitive behavior as the Add Folder button.
                foreach (string sourceFolder in profile.SourceFolders
                    .Where(folder => !string.IsNullOrWhiteSpace(folder))
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    listSourceFolders.Items.Add(sourceFolder);
                }

                activeProfile = profile;
                txtDestinationPath.Text = profile.DestinationPath ?? string.Empty;
                UpdateDestinationStatus(profile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The saved backup profile could not be loaded: " + ex.Message);
            }
        }

        private BackupDestinationDetectionResult? UpdateDestinationStatus(BackupProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.DestinationMarkerId))
            {
                ShowDestinationNotConfigured();
                return null;
            }

            BackupDestinationService destinationService = new BackupDestinationService();
            BackupDestinationDetectionResult result = destinationService.DetectDestination(
                profile.DestinationMarkerId,
                profile.DestinationRelativePath
            );

            lblDestinationStatus.Text = $"Backup Destination Status: {FormatStatus(result.Status)}";

            lblDestinationStatus.ForeColor = result.Status switch
            {
                BackupDestinationStatus.Connected => Color.DarkGreen,
                BackupDestinationStatus.InvalidMarker => Color.DarkOrange,
                BackupDestinationStatus.NotFound => Color.Firebrick,
                BackupDestinationStatus.MultipleMatches => Color.Firebrick,
                _ => Color.DimGray
            };

            return result;
        }

        private string FormatStatus(BackupDestinationStatus status)
        {
            return status switch
            {
                BackupDestinationStatus.NotFound => "Not Found",
                BackupDestinationStatus.MultipleMatches => "Multiple Matches",
                BackupDestinationStatus.InvalidMarker => "Invalid Marker",
                _ => status.ToString()
            };
        }

        private void ShowDestinationNotConfigured()
        {
            lblDestinationStatus.Text = "Backup Destination Status: Not Configured";
            lblDestinationStatus.ForeColor = Color.DimGray;
        }

        private bool DestinationTextMatchesActiveProfile()
        {
            if (activeProfile == null)
            {
                return false;
            }

            try
            {
                string visiblePath = Path.TrimEndingDirectorySeparator(
                    Path.GetFullPath(txtDestinationPath.Text)
                );
                string savedPath = Path.TrimEndingDirectorySeparator(
                    Path.GetFullPath(activeProfile.DestinationPath)
                );

                return visiblePath.Equals(savedPath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /*
            Starts the backup when the user clicks the button.
            It scans all files across multiple source folders, copies new or modified files in the background,
            skips unchanged files, and updates the progress bar and backup results. 
            The use of async in this event handler allows the backup work to run in the background without
            freezing the interface.
         */
        private async void btnStartBackup_Click(object sender, EventArgs e)
        {
            // Make sure the user selected a source folder
            if (listSourceFolders.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one source folder.");
                return;
            }

            // Make sure the user selected a destination/backup folder
            if (string.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                MessageBox.Show("Please select a destination folder.");
                return;
            }

            if (activeProfile == null ||
                string.IsNullOrWhiteSpace(activeProfile.DestinationMarkerId))
            {
                ShowDestinationNotConfigured();
                MessageBox.Show(
                    "Backup cannot start because the destination has not been configured. " +
                    "Please save the profile first."
                );
                return;
            }

            if (!DestinationTextMatchesActiveProfile())
            {
                ShowDestinationNotConfigured();
                MessageBox.Show(
                    "Backup cannot start because the destination path has changed. " +
                    "Please save the profile before starting the backup."
                );
                return;
            }

            // Always perform a fresh check immediately before starting. Do not rely
            // only on the status that was displayed when the application opened.
            BackupDestinationDetectionResult? destinationResult =
                UpdateDestinationStatus(activeProfile);

            if (destinationResult?.Status != BackupDestinationStatus.Connected)
            {
                string reason = destinationResult?.Message
                    ?? "The destination is not configured.";

                MessageBox.Show("Backup cannot start. " + reason);
                return;
            }

            // Convert all folders currently stored in the ListBox
            // into a normal List<string> that can be passed to BackupService
            List<string> sourceFolders = listSourceFolders.Items
                .Cast<string>()
                .ToList();

            // Use the path found by the marker scan. Its drive letter may differ
            // from the one that was originally saved in the profile.
            string destinationFolder = destinationResult.ResolvedDestinationPath;

            // Reset the progress UI before starting a new backup
            progressBarBackup.Value = 0;
            lblProgressPercent.Text = "0%";
            lblStatus.Text = "Starting backup...";

            // Disbale the Start Backup button so the user cannot
            // start another backup while one is already running
            btnStartBackup.Enabled = false;

            // Reset the backup statistics from the previous backup
            lblFilesScanned.Text = "Files scanned: 0";
            lblFilesCopied.Text = "Files copied: 0";
            lblFilesSkipped.Text = "Files skipped: 0";
            lblErrors.Text = "Errors: 0";

            try
            {
                // Create an object that receives progress updates from BackupService.
                // Every time BackupService calls progress.Report(...),
                // this code runs and updates the Winforms controls.
                var progress = new Progress<BackupProgress>(p =>
                {
                    // Update the progress bar using the total number
                    // of files and the number already processed
                    progressBarBackup.Maximum = p.TotalFiles;
                    progressBarBackup.Value = p.ProcessedFiles;

                    // Calculate the percentage of the backup that is complete
                    int percent = p.TotalFiles > 0
                        ? (int)((double)p.ProcessedFiles / p.TotalFiles * 100)
                        : 0;

                    // Display the current backup percentage
                    lblProgressPercent.Text = $"{percent}%";

                    // Show how many files have been processed so far
                    lblStatus.Text =
                        $"Processing {p.ProcessedFiles} of {p.TotalFiles} files";

                    // Update the backup statistics on the form
                    lblFilesScanned.Text =
                        $"Files scanned: {p.ProcessedFiles:N0}";
                    lblFilesCopied.Text =
                        $"Files copied: {p.CopiedFiles:N0}";
                    lblFilesSkipped.Text =
                        $"Files skipped: {p.SkippedFiles:N0}";
                    lblErrors.Text =
                        $"Errors: {p.ErrorCount:N0}";
                });

                // Create the service that contains the actual backup logic
                BackupService backupService = new BackupService();

                // Start the backup and wait for it to finish.
                // BackupService handles scanning, comparing, copying, 
                // skipping, and reporting progress back to this form.
                BackupProgress result = await backupService.RunBackupAsync(
                    sourceFolders,
                    destinationFolder,
                    progress
                );

                // This text will only show once the backup has completed
                lblStatus.Text = "Backup complete!";

                // Use the completed result for the final summary. Progress updates
                // are still used while the backup is running.
                lblFilesScanned.Text = $"Files scanned: {result.ProcessedFiles:N0}";
                lblFilesCopied.Text = $"Files copied: {result.CopiedFiles:N0}";
                lblFilesSkipped.Text = $"Files skipped: {result.SkippedFiles:N0}";
                lblErrors.Text = $"Errors: {result.ErrorCount:N0}";

                DateTime completedAt = DateTime.Now;

                lblLastBackup.Text = $"Last backup: {completedAt:g}";

                SaveLastBackupTime();

                try
                {
                    SaveBackupHistoryEntry(new BackupHistoryEntry
                    {
                        CompletedAt = completedAt,
                        SourceFolders = new List<string>(sourceFolders),
                        DestinationPath = destinationFolder,
                        FilesScanned = result.ProcessedFiles,
                        FilesCopied = result.CopiedFiles,
                        FilesSkipped = result.SkippedFiles,
                        ErrorCount = result.ErrorCount,
                        FailedFiles = new List<BackupError>(result.FailedFiles)
                    });
                }
                catch (Exception ex)
                {
                    // The backup itself succeeded even if its history could not be saved.
                    MessageBox.Show(
                        "The backup completed, but its history could not be saved: " + ex.Message
                    );
                }
           
            }
            catch (Exception ex)
            {           
                // If anything goes wrong during backup, show the error instead of crashing the program
                MessageBox.Show("Backup failed: " + ex.Message);
            }
            finally
            {
                // Always turn the Start Backup button back on,
                // whether the backup succeeded or failed
                btnStartBackup.Enabled = true;
            }
        }

        private void SaveLastBackupTime()
        {
            BackupSettings settings = new BackupSettings
            {
                LastBackup = DateTime.Now
            };

            string json = JsonSerializer.Serialize(settings);

            File.WriteAllText(settingsFile, json);
        }

        private void SaveBackupHistoryEntry(BackupHistoryEntry newEntry)
        {
            List<BackupHistoryEntry> history = new List<BackupHistoryEntry>();

            if (File.Exists(historyFile))
            {
                string existingJson = File.ReadAllText(historyFile);

                // The history file contains a JSON array, so deserialize it into a list.
                history = JsonSerializer.Deserialize<List<BackupHistoryEntry>>(existingJson)
                    ?? new List<BackupHistoryEntry>();
            }

            history.Add(newEntry);

            string? historyDirectory = Path.GetDirectoryName(historyFile);

            if (!string.IsNullOrEmpty(historyDirectory))
            {
                Directory.CreateDirectory(historyDirectory);
            }

            string json = JsonSerializer.Serialize(history, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(historyFile, json);
        }

        private void LoadLastBackupTime()
        {
            if (!File.Exists(settingsFile))
            {
                lblLastBackup.Text = "Last backup: Never";
                return;
            }

            string json = File.ReadAllText(settingsFile);

            BackupSettings? settings =
                JsonSerializer.Deserialize<BackupSettings>(json);

            if (settings?.LastBackup != null)
            {
                lblLastBackup.Text = $"Last backup: {settings.LastBackup.Value:g}";
            }
        }

        
    }
}
