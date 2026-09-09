using System.Text.Json;

namespace BackupUtility
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Restore the time of the most recent successful backup
            LoadLastBackupTime();

            // Restore the saved source folders and destination, then check
            // whether the configured backup destination is currently available.
            LoadBackupProfile();

            // Begin checking the destination status at the timer's configured interval.
            destinationStatusTimer.Start();
        }

        // Store the most recent successful backup time in a small settings file
        // Because this is a relative path, its exact location depends on the folder
        // from which BackupUtility is launched
        private readonly string settingsFile = "backupSettings.json";

        // Store the default backup profile in the current user's Local AppData folder.
        // This provides a stable, per-user location that does not depend on where
        // the application executable was launched.
        private readonly string profileFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BackupUtility",
            "backupProfile.json"
        );

        // Store all completed backup-history entries in the same stable,
        // per-user BackupUtility application-data folder.
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

                    string? conflictingFolder = FindSourceFolderNameCollision(
                        selectedFolder,
                        listSourceFolders.Items.Cast<string>()
                    );

                    if (conflictingFolder != null)
                    {
                        string backupFolderName = GetSourceBackupFolderName(selectedFolder);

                        MessageBox.Show(
                            "This source folder cannot be added because another source folder " +
                            $"already uses the backup folder name \"{backupFolderName}\".\n\n" +
                            $"Existing: {conflictingFolder}\n" +
                            $"Selected: {selectedFolder}"
                        );
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

        /*
            Returns an existing source folder whose final folder name would map
            to the same destination subfolder as the candidate source.
        */
        private string? FindSourceFolderNameCollision(
            string candidateFolder,
            IEnumerable<string> existingFolders)
        {
            string candidateName = GetSourceBackupFolderName(candidateFolder);

            return existingFolders.FirstOrDefault(existingFolder =>
                GetSourceBackupFolderName(existingFolder).Equals(
                    candidateName,
                    StringComparison.OrdinalIgnoreCase
                ));
        }

        /*
            Gets the final folder name that BackupService uses as the source's
            top-level destination folder. Trailing separators are removed first.
        */
        private string GetSourceBackupFolderName(string sourceFolder)
        {
            string trimmedPath = Path.TrimEndingDirectorySeparator(sourceFolder);
            string folderName = Path.GetFileName(trimmedPath);

            // Drive roots do not have a normal final folder name and currently
            // map directly into the backup destination root.
            return string.IsNullOrEmpty(folderName) ? "<destination root>" : folderName;
        }

        /*
            Checks all selected sources for duplicate destination folder names.
            This protects against collisions loaded from older or edited profile JSON.
        */
        private bool TryFindSourceFolderNameCollision(
            List<string> sourceFolders,
            out string firstFolder,
            out string secondFolder,
            out string backupFolderName)
        {
            for (int firstIndex = 0; firstIndex < sourceFolders.Count; firstIndex++)
            {
                for (int secondIndex = firstIndex + 1;
                    secondIndex < sourceFolders.Count;
                    secondIndex++)
                {
                    string firstName = GetSourceBackupFolderName(sourceFolders[firstIndex]);
                    string secondName = GetSourceBackupFolderName(sourceFolders[secondIndex]);

                    if (firstName.Equals(secondName, StringComparison.OrdinalIgnoreCase))
                    {
                        firstFolder = sourceFolders[firstIndex];
                        secondFolder = sourceFolders[secondIndex];
                        backupFolderName = firstName;
                        return true;
                    }
                }
            }

            firstFolder = string.Empty;
            secondFolder = string.Empty;
            backupFolderName = string.Empty;
            return false;
        }

        /*
            Runs when the user clicks the Save Profile button.
            It validates that the required folders are selected, then
            save the current source folders and destination as the default profile.
         */
        private void btnSaveProfile_Click(object sender, EventArgs e)
        {
            // A profile must contain at least one source folder
            if (listSourceFolders.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one source folder before saving the profile.");
                return;
            }

            // A profile cannot be saved without a backup destination
            if (string.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                MessageBox.Show("Please select a destination folder before saving the profile.");
                return;
            }

            // Attempt to save the profile and report whether the operation succeeded
            try
            {
                SaveBackupProfile();
                MessageBox.Show("Backup profile saved successfully.");
            }
            catch (Exception ex)
            {
                // Display file, permission, marker, or JSON errors without crashing the application.
                MessageBox.Show("The backup profile could not be saved: " + ex.Message);
            }
        }

        /*
            Runs when the user clicks View History.
            It loads the saved history entries from JSON and opens the
            read-only BackupHistoryForm.
        */
        private void btnViewHistory_Click(object sender, EventArgs e)
        {
            // Stop early if no history file has been created yet
            if (!File.Exists(historyFile))
            {
                MessageBox.Show("No backup history is available yet.");
                return;
            }

            try
            {
                // Read the complete history JSON from the user's application-data folder
                string json = File.ReadAllText(historyFile);

                // Convert the JSON array into a list of BackupHistoryEntry objects.
                // Use an empty list if deserialization produces no result.
                List<BackupHistoryEntry> history =
                    JsonSerializer.Deserialize<List<BackupHistoryEntry>>(json)
                    ?? new List<BackupHistoryEntry>();

                // Do not open an empty history window when there are no saved entries
                if (history.Count == 0)
                {
                    MessageBox.Show("No backup history is available yet.");
                    return;
                }

                // Open the history window as a modal dialog
                // using ensures the form's resources are disposed after it closes
                using BackupHistoryForm historyForm = new BackupHistoryForm(history, historyFile);
                historyForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                // Handle unreadable or invalid history JSON without crashing the application
                MessageBox.Show("The backup history could not be loaded: " + ex.Message);
            }
        }

        /*
            Saves the current source folders and backup destination as the default profile.
            It also creates or reuses the destination marker and stores the information needed
            to find that destination if its drive letter changes.
         */
        private void SaveBackupProfile()
        {
            // Convert the destination into a complete, normalized path before saving it
            string fullDestinationPath = Path.GetFullPath(txtDestinationPath.Text);

            // Create the service responsible for destination markers and drive-relative paths
            BackupDestinationService destinationService = new BackupDestinationService();

            // Reuse the destination's existing marker ID or create one if none exists
            string markerId = destinationService.GetOrCreateMarkerId(fullDestinationPath);

            // Remove the drive-specific portion of the path so the destination
            // can still be found if Windows assigns the drive a different letter
            string relativePath =
                destinationService.GetDestinationRelativePath(fullDestinationPath);

            // Collect the current UI settings and destination identity into one profile object
            BackupProfile profile = new BackupProfile
            {
                SourceFolders = listSourceFolders.Items.Cast<string>().ToList(),
                DestinationPath = fullDestinationPath,
                DestinationMarkerId = markerId,
                DestinationRelativePath = relativePath
            };

            // Get the directory where the profile JSON file will be stored
            string? profileDirectory = Path.GetDirectoryName(profileFile);

            if (!string.IsNullOrEmpty(profileDirectory))
            {
                // Create the application-data directory if it does not already exist
                // This method is safe to call when the directory already exists
                Directory.CreateDirectory(profileDirectory);
            }

            // WriteIndented makes the saved JSON easier to read
            // and if the profile file needs to be inspected during testing.
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Save the JSON, replacing the previous default profile
            File.WriteAllText(profileFile, json);

            // Keep the successfully saved profile in memory for status checks
            // and pre-backup destination validation
            activeProfile = profile;

            // Immediately verify the saved destination and update its status label
            UpdateDestinationStatus(profile);
        }

        /*
            Loads the saved default backup profile when the application starts.
            It restores the source folders and destination path, then checks
            whether the saved backup destination is currently available.
         */
        private void LoadBackupProfile()
        {
            // A missing file simply means the user has not saved a profile yet.
            if (!File.Exists(profileFile))
            {
                return;
            }

            try
            {
                // Read the JSON and convert it back into a BackupProfile object
                string json = File.ReadAllText(profileFile);
                BackupProfile? profile = JsonSerializer.Deserialize<BackupProfile>(json);

                // Deserialization can return null if the JSON does not contain a profile
                if (profile == null)
                {
                    return;
                }

                // Remove any existing UI entries before restoring the saved folders
                listSourceFolders.Items.Clear();

                // Ignore blank paths and remove duplicates using the same
                // case-insensitive comparison as the Add Folder button
                foreach (string sourceFolder in profile.SourceFolders
                    .Where(folder => !string.IsNullOrWhiteSpace(folder))
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    listSourceFolders.Items.Add(sourceFolder);
                }

                // Keep the loaded profile in memory for automatic status checks
                // and pre-backup destination validation.
                activeProfile = profile;

                // Restore the saved destination path in the textbox.
                txtDestinationPath.Text =
                    profile.DestinationPath ?? string.Empty; // use DestinationPath unless it is null, otherwise, use an empty string

                // Check whether the marked destination is currently available.
                UpdateDestinationStatus(profile);
            }
            catch (Exception ex)
            {
                // Handle unreadable or invalid profile data without crashing the application
                MessageBox.Show("The saved backup profile could not be loaded: " + ex.Message);
            }
        }
        /*
            Scans the available drives for the marker stored in the supplied profile.
            It updates the status label and returns the complete detection result
            so other code can decide whether a backup is allowed to start.
        */
        private BackupDestinationDetectionResult? UpdateDestinationStatus(BackupProfile profile)
        {
            // A profile without a marker ID has not been configured
            // for reliable destination detection
            if (string.IsNullOrWhiteSpace(profile.DestinationMarkerId))
            {
                ShowDestinationNotConfigured();
                return null;
            }

            // Ask the destination service to search for the marker saved in the profile
            BackupDestinationService destinationService = new BackupDestinationService();

            BackupDestinationDetectionResult result = destinationService.DetectDestination(
                profile.DestinationMarkerId,
                profile.DestinationRelativePath
            );

            // Convert the enum value into readable text for the status label
            lblDestinationStatus.Text = $"Backup Destination Status: {FormatStatus(result.Status)}";

            // Use a different color to make each status easier to recognize
            lblDestinationStatus.ForeColor = result.Status switch
            {
                BackupDestinationStatus.Connected => Color.DarkGreen,
                BackupDestinationStatus.InvalidMarker => Color.DarkOrange,
                BackupDestinationStatus.NotFound => Color.Firebrick,
                BackupDestinationStatus.MultipleMatches => Color.Firebrick,
                _ => Color.DimGray
            };

            // Return the result so the Start Backup handler can enforce it
            return result;
        }

        /*
            Converts destination-status enum values into user-friendly text.
            Values that do not need special spacing use their enum name directly.
        */
        private string FormatStatus(BackupDestinationStatus status)
        {
            return status switch
            {
                // Add spaces to enum names that contain multiple words
                BackupDestinationStatus.NotFound => "Not Found",
                BackupDestinationStatus.MultipleMatches => "Multiple Matches",
                BackupDestinationStatus.InvalidMarker => "Invalid Marker",

                // Connected already has the correct display format
                _ => status.ToString()
            };
        }

        /*
            Resets the destination label when there is no saved and verified
            marker configuration for the path currently shown in the textbox.
        */
        private void ShowDestinationNotConfigured()
        {
            // Use neutral text and color because this is a configuration state,
            // not a missing or invalid destination error
            lblDestinationStatus.Text = "Backup Destination Status: Not Configured";
            lblDestinationStatus.ForeColor = Color.DimGray;
        }

        /*
            Checks whether the destination shown in the textbox still matches
            the destination stored in the active profile. This prevents an unsaved
            path change from using the previously verified marker.
         */
        private bool DestinationTextMatchesActiveProfile()
        {
            // There is no saved destination to compare when no profile is active
            if (activeProfile == null)
            {
                return false;
            }

            try
            {
                // Normalize both paths and remove trailing directory separators
                // so equivalent paths such as "E:\\Backups" and "E:\\Backups\\"
                // are treated as equal
                string visiblePath = Path.TrimEndingDirectorySeparator(
                    Path.GetFullPath(txtDestinationPath.Text)
                );

                string savedPath = Path.TrimEndingDirectorySeparator(
                    Path.GetFullPath(activeProfile.DestinationPath)
                );

                // Windows paths are compared without treating letter casing as significant
                return visiblePath.Equals(
                    savedPath,
                    StringComparison.OrdinalIgnoreCase
                );
            }
            catch
            {
                // An invalid path cannot safely match the saved destination
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
            // A backup requires at least one source folder
            // Stop before starting any backup work if the list is empty
            if (listSourceFolders.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one source folder.");
                return;
            }

            // Convert the ListBox entries into a normal list for validation
            // and for passing to BackupService later.
            List<string> sourceFolders = listSourceFolders.Items
                .Cast<string>()
                .ToList();

            if (TryFindSourceFolderNameCollision(
                sourceFolders,
                out string firstFolder,
                out string secondFolder,
                out string backupFolderName))
            {
                MessageBox.Show(
                    "Backup cannot start because two source folders use the same " +
                    $"backup folder name \"{backupFolderName}\".\n\n" +
                    $"First: {firstFolder}\n" +
                    $"Second: {secondFolder}"
                );
                return;
            }

            // A destination path must be displayed before its saved marker can be verified
            if (string.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                MessageBox.Show("Please select a destination folder.");
                return;
            }

            // A reliable backup destination must have an active profile and marker ID
            // Without them, BackupUtility cannot confirm that it found the intended destination
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

            // Prevent an edited destination textbox from using the identity stored
            // for a different saved path. The changed path must be saved and verified first
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
                    progress,
                    CancellationToken.None
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

        /*
            Saves the current date and time after a backup completes successfully.
            This value is stored separately from backup history and is restored
            in the Last Backup label when the application starts.
         */
        private void SaveLastBackupTime()
        {
            // Create a settings object containing the current completion time
            BackupSettings settings = new BackupSettings
            {
                LastBackup = DateTime.Now
            };

            // Convert the settings object into JSON
            string json = JsonSerializer.Serialize(settings);

            // Save the JSON, replacing the previously stored backup time
            File.WriteAllText(settingsFile, json);
        }

        /*
            Adds one completed backup result to the persistent history.
            Existing entries are loaded first so the new entry is appended
            instead of replacing the previous backup history.
         */
        private void SaveBackupHistoryEntry(BackupHistoryEntry newEntry)
        {
            // Begin with an empty list for the application's first history entry
            List<BackupHistoryEntry> history = new List<BackupHistoryEntry>();

            // Load the existing entries when a history file has already been created
            if (File.Exists(historyFile))
            {
                string existingJson = File.ReadAllText(historyFile);

                // The history file contains a JSON array, so deserialize it into a list.
                // Use an empty list if deserialization does not produce a result
                history = JsonSerializer.Deserialize<List<BackupHistoryEntry>>(existingJson)
                    ?? new List<BackupHistoryEntry>();
            }

            // Append this copmleted backup without removing previous entries
            history.Add(newEntry);

            // Get the application-data directory containing the history file
            string? historyDirectory = Path.GetDirectoryName(historyFile);

            if (!string.IsNullOrEmpty(historyDirectory))
            {
                // Create the directory if necessary. Nothing happens if it already exists
                Directory.CreateDirectory(historyDirectory);
            }

            // Convert the full history list into readable, indented JSON
            string json = JsonSerializer.Serialize(history, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Save the updated list, replacing the old JSON with the new complete list
            File.WriteAllText(historyFile, json);
        }

        /*
            Loads the most recent successful backup time when the application starts.
            If no settings file exists, the label indicates that a backup has
            never been completed.
        */
        private void LoadLastBackupTime()
        {
            // A missing settings file means no successful backup time has been saved
            if (!File.Exists(settingsFile))
            {
                lblLastBackup.Text = "Last backup: Never";
                return;
            }

            // Return the saved settings JSON
            string json = File.ReadAllText(settingsFile);

            // Convert the JSON back into a BackupSetting object
            // The result is nullable because deserialization may not produce an object
            BackupSettings? settings =
                JsonSerializer.Deserialize<BackupSettings>(json);

            // Update the label only when both settings object and timestamp exist
            if (settings?.LastBackup != null)
            {
                // The "g" format display a short date together with a short
                lblLastBackup.Text = $"Last backup: {settings.LastBackup.Value:g}";
            }
        }

        
    }
}
