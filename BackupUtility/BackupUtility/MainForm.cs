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
        }

        private readonly string settingsFile = "backupSettings.json";

        // LocalApplicationData is a stable, per-user location for application files.
        // This avoids relying on whichever folder the program was launched from.
        private readonly string profileFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BackupUtility",
            "backupProfile.json"
        );

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

        private void SaveBackupProfile()
        {
            BackupProfile profile = new BackupProfile
            {
                SourceFolders = listSourceFolders.Items.Cast<string>().ToList(),
                DestinationPath = txtDestinationPath.Text
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

                txtDestinationPath.Text = profile.DestinationPath ?? string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("The saved backup profile could not be loaded: " + ex.Message);
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

            // Convert all folders currently stored in the ListBox
            // into a normal List<string> that can be passed to BackupService
            List<string> sourceFolders = listSourceFolders.Items
                .Cast<string>()
                .ToList();

            // Store the selected destination folder
            string destinationFolder = txtDestinationPath.Text;

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
                await backupService.RunBackupAsync(sourceFolders, destinationFolder, progress);

                // This text will only show once the backup has completed
                lblStatus.Text = "Backup complete!";

                lblLastBackup.Text = $"Last backup: {DateTime.Now:g}";

                SaveLastBackupTime();
           
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
