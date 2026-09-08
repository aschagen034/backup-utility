using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupUtility
{
    internal class BackupService
    {
        /*
            Runs the backup asynchronously across all selected source folders.
            It copies new or updated files, skips unchanged files, preserves folder
            structure, reports live progress, and returns the final backup results.
            Individual file errors are recorded without stopping the remaining backup.
        */
        public async Task<BackupProgress> RunBackupAsync(List<string> sourceFolders, string destinationFolder, IProgress<BackupProgress> progress)
        {
            // Run the file-copying work on a background thread
            // so the WinForms interface stays responsive
            return await Task.Run(() =>
            {
                int scannedCount = 0;
                int copiedCount = 0;
                int skippedCount = 0;
                int errorCount = 0;
                List<BackupError> failedFiles = new List<BackupError>();

                // Count the total number of files across all source folders
                int totalFiles = 0;

                foreach (string sourceFolder in sourceFolders)
                {
                    // Get every file inside the source folder, including files inside all subfolders
                    totalFiles += Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories).Length;

                }

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
                        scannedCount++;

                        try
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
                        }
                        catch (Exception ex)
                        {
                            // If an error occurs while processing a file, increase
                            // the error counter, remember the details, and continue.
                            errorCount++;
                            failedFiles.Add(new BackupError
                            {
                                FilePath = file,
                                ErrorMessage = ex.Message
                            });
                        }

                        // Send the current backup statistics back to MainForm.
                        // MainForm uses this information to update the progress bar,
                        // percentage, status text, and backup summary labels.
                        progress.Report(new BackupProgress
                        {
                            ProcessedFiles = scannedCount,
                            TotalFiles = totalFiles,
                            CopiedFiles = copiedCount,
                            SkippedFiles = skippedCount,
                            ErrorCount = errorCount
                        });                    

                    }
                }

                // Return one final snapshot after all files have been processed.
                // MainForm can use this reliable result after awaiting the backup.
                return new BackupProgress
                {
                    ProcessedFiles = scannedCount,
                    TotalFiles = totalFiles,
                    CopiedFiles = copiedCount,
                    SkippedFiles = skippedCount,
                    ErrorCount = errorCount,
                    FailedFiles = failedFiles
                };
            });
        }
    }
}
