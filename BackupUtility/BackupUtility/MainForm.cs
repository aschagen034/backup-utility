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
            This function runs when the user clicks the select source folder button. The function opens a 
            folder picker, gets the path of the selected folder, and displays the path in the corresponding textbox.
        */
        private void btnSelectSource_Click(object sender, EventArgs e)
        {
            // Creates a folder selection dialog
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // Opens the dialog and checks if the user selected a folder
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    // Displays the selected source folder path in the textbox
                    txtSourcePath.Text = folderDialog.SelectedPath;
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
            if (string.IsNullOrWhiteSpace(txtSourcePath.Text))
            {
                MessageBox.Show("Please select a source folder.");
                return;
            }

            // Make sure the user selected a destination/backup folder
            if (string.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                MessageBox.Show("Please select a destination folder.");
                return;
            }

            // Store the selected source and destination paths in variables
            string sourceFolder = txtSourcePath.Text;
            string destinationFolder = txtDestinationPath.Text;

            try
            {
                // Get every file inside the source folder, including files inside all subfolders
                string[] files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

                // Set up the progress bar based on the total number of files
                progressBarBackup.Minimum = 0;
                progressBarBackup.Maximum = files.Length;
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

                    // Go through every file found in the source folder
                    foreach (string file in files)
                    {
                        // Get the file's path relative to the source folder
                        string relativePath = Path.GetRelativePath(sourceFolder, file);

                        // Build the matching path inside the backup folder
                        string destFile = Path.Combine(destinationFolder, relativePath);

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

                            lblStatus.Text = $"Processing {progressBarBackup.Value} of {files.Length} files";

                        });

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

        
    }
}
