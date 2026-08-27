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

        private void btnSelectSource_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnSelectDestination_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtDestinationPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnStartBackup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSourcePath.Text))
            {
                MessageBox.Show("Please select a source folder.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDestinationPath.Text))
            {
                MessageBox.Show("Please select a destination folder.");
                return;
            }

            string sourceFolder = txtSourcePath.Text;
            string destinationFolder = txtDestinationPath.Text;

            try
            {
                string[] files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string relativePath = Path.GetRelativePath(sourceFolder, file);

                    string destFile = Path.Combine(destinationFolder, relativePath);

                    string? destDirectory = Path.GetDirectoryName(destFile);

                    if (!string.IsNullOrEmpty(destDirectory))
                    {
                        Directory.CreateDirectory(destDirectory);
                    }

                    if (!File.Exists(destFile))
                    {
                        File.Copy(file, destFile);
                    }
                    else
                    {
                        DateTime sourceModified = File.GetLastWriteTime(file);
                        DateTime destinationModified = File.GetLastWriteTime(destFile);

                        if (sourceModified > destinationModified)
                        {
                            File.Copy(file, destFile, true);
                        }
                    }
                }

                MessageBox.Show("All files copied successfully!");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Backup failed: " + ex.Message);
            }
        }
    }
}
