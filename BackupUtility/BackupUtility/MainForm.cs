namespace BackupUtility
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

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
                    // Displays the selected source folder path in the textbox
                    listSourceFolders.Items.Add(folderDialog.SelectedPath);
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
            Starts the backup when the user clicks the button.
            It scans all source files, copies new or modified files in the background,
            skips unchanged files, and updates the progress bar and backup results. 
            The use of async in this event handler allows it to wait for asynchronous work without
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

            // Store all source folders from the ListBox in a normal list
            List<string> sourceFolders = listSourceFolders.Items
                .Cast<string>()
                .ToList();

            // Store the selected destination folder
            string destinationFolder = txtDestinationPath.Text;

            try
            {
                // Count the total number of files across all source folders
                int totalFiles = 0;

                foreach (string sourceFolder in sourceFolders)
                {
                    // Get every file inside the source folder, including files inside all subfolders
                    totalFiles += Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories).Length;

                }


                // Set up the progress bar based on the total number of files
                progressBarBackup.Minimum = 0;
                progressBarBackup.Maximum = totalFiles;
                progressBarBackup.Value = 0;

                // Update the UI to show that the backup is starting
                lblStatus.Text = "Starting backup...";

                // Prevent the user from starting another backup while one is already running
                btnStartBackup.Enabled = false;

                // Counters used for the final backup results
                int copiedCount = 0;
                int skippedCount = 0;

                // Run the file-copying work on a background thread
                // so the WinForms interface stays responsive
                await Task.Run(() =>
                {
                    // Go through each selected source folder
                    foreach (string sourceFolder in sourceFolders)
                    {
                        // Get the name of the source folder itself
                        string sourceFolderName = Path.GetFileName(sourceFolder);

                        // Get every file inside the source folder
                        string[] files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

                        // Go through every file found in the source folder
                        foreach (string file in files)
                        {
                            // Get the file's path relative to the source folder
                            string relativePath = Path.GetRelativePath(sourceFolder, file);

                            // Build the matching path inside the backup folder
                            string destFile = Path.Combine(destinationFolder, sourceFolderName, relativePath);

                            // Get the folder that the destination file belongs in 
                            string? destDirectory = Path.GetDirectoryName(destFile);

                            // Create the destination folder if it doesn't already exist
                            if (!string.IsNullOrEmpty(destDirectory))
                            {
                                Directory.CreateDirectory(destDirectory);
                            }

                            // If the file does not exist in the backup yet, copy it
                            if (!File.Exists(destFile))
                            {
                                File.Copy(file, destFile);
                                copiedCount++;
                            }
                            else
                            {
                                // If the file already exists, compare when each version was last modified
                                DateTime sourceModified = File.GetLastWriteTime(file);
                                DateTime destinationModified = File.GetLastWriteTime(destFile);

                                // If the source version is newer, overwrite the backup version
                                if (sourceModified > destinationModified)
                                {
                                    File.Copy(file, destFile, true);
                                    copiedCount++;
                                }
                                else
                                {
                                    // Otherwise the file has not been changed so we can skip it
                                    skippedCount++;
                                }
                            }

                            // Task.Run is using a background thread.
                            // WinForms controls must be updated from the UI thread,
                            // so Invoke safely updates the progress bar and label.
                            Invoke(() =>
                            {
                                progressBarBackup.Value++;

                                lblStatus.Text = $"Processing {progressBarBackup.Value} of {totalFiles} files";

                            });

                        }
                    }

                });

                // This runs after Task.Run has completely finished
                lblStatus.Text = "Backup complete!";

                // Show the final backup results
                MessageBox.Show($"Backup complete!\nFiles copied: {copiedCount}\nFiles skipped: {skippedCount}");
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

        private void btnRemoveSource_Click(object sender, EventArgs e)
        {
            if (listSourceFolders.SelectedItem == null)
            {
                MessageBox.Show("Please select a folder to remove.");
                return;
            }

            listSourceFolders.Items.Remove(listSourceFolders.SelectedItem);
        }
    }
}
