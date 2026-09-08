using System.Text.Json;

namespace BackupUtility
{
    internal class BackupDestinationService
    {
        private const string MarkerFileName = ".backuputility-destination.json";

        /*
            Gets the unique marker ID for a backup destination.
            If the destination already contains a valid marker, reuse its ID.
            Otherwise, create a new marker so the destination can be recognized
            even if Windows later assigns the drive a different letter.
         */
        public string GetOrCreateMarkerId(string destinationPath)
        {
            // A marker can only read or created if the destination is currently available
            if (!Directory.Exists(destinationPath))
            {
                throw new DirectoryNotFoundException(
                    "The selected backup destination does not exist or is not currently available."
                );
            }

            // Build the full path to the marker file inside the selected destination folder
            string markerFile = Path.Combine(destinationPath, MarkerFileName);

            // Reuse an existing marker so saving the profile does not change its identity
            if (File.Exists(markerFile))
            {
                return ReadValidMarkerId(markerFile);
            }

            // Generate a new unique ID because this destination does not have marker yet
            BackupDestinationMarker marker = new BackupDestinationMarker
            {
                MarkerId = Guid.NewGuid().ToString()
            };

            // Convert the marker object into readable JSON before saving it
            string json = JsonSerializer.Serialize(marker, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Save the marker inside the destination and return the same ID to MainForm
            File.WriteAllText(markerFile, json);
            return marker.MarkerId;
        }

        /*
            Converts the full destination path into a path relative to its drive root.
            This allows BackupUtility to rebuild the destination path if Windows changes
            the drive letter.
         */
        public string GetDestinationRelativePath(string destinationPath)
        {
            // Normalize the path before separating its drive root and relative portion
            string fullDestinationPath = Path.GetFullPath(destinationPath);
            string? driveRoot = Path.GetPathRoot(fullDestinationPath);

            // Stop if the path cannot be associated with a valid drive or volume root
            if (string.IsNullOrEmpty(driveRoot))
            {
                throw new InvalidOperationException(
                    "The selected backup destination does not have a valid drive root."
                );
            }

            // Get the destination path relative to the drive root.
            // For example, "E:\\Backups" becomes "Backups".
            string relativePath = Path.GetRelativePath(driveRoot, fullDestinationPath);

            // Path.GetRelativePath returns "." when the destination is the drive root.
            return relativePath == "." ? string.Empty : relativePath;
        }

        /*
            Searches the computer's available drives for the destination marker stored
            in the saved profile. The method returns a status explaining whether one matching destination,
            no destination, mulitple destinations, or an invalid marker was found.
         */
        public BackupDestinationDetectionResult DetectDestination(
            string expectedMarkerId,
            string destinationRelativePath)
        {
            // The saved marker ID must be a valid GUID before it can be compared safely
            if (!Guid.TryParse(expectedMarkerId, out Guid expectedId))
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.InvalidMarker,
                    Message = "The saved profile does not contain a valid destination marker ID."
                };
            }

            // This value must be relative because it will be combined with each drive root
            if (Path.IsPathRooted(destinationRelativePath))
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.InvalidMarker,
                    Message = "The saved destination-relative path is invalid."
                };
            }

            // Store every valid match so duplicate markers can be detected safely
            List<string> matchingDestinations = new List<string>();
            bool invalidMarkerFound = false;

            // Inspect each drive currently known to Windows
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    // External SSDs may be reported as Fixed or Removable depending
                    // on their enclosure and driver, so support both drive types.
                    if (!drive.IsReady ||
                        (drive.DriveType != DriveType.Fixed &&
                         drive.DriveType != DriveType.Removable))
                    {
                        continue;
                    }

                    string driveRoot = drive.RootDirectory.FullName;

                    // Rebuild the expected destination path using this drive's current letter
                    string candidateDestination = Path.GetFullPath(
                        Path.Combine(driveRoot, destinationRelativePath)
                    );

                    // Protect against a manually edited relative path containing "..".
                    if (!candidateDestination.StartsWith(
                        driveRoot,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return new BackupDestinationDetectionResult
                        {
                            Status = BackupDestinationStatus.InvalidMarker,
                            Message = "The saved destination-relative path is invalid."
                        };
                    }

                    // Build the marker location expected on this candidate drive
                    string candidateMarker = Path.Combine(
                        candidateDestination,
                        MarkerFileName
                    );

                    // This drive cannot be a match if the expected marker file is absent
                    if (!File.Exists(candidateMarker))
                    {
                        continue;
                    }

                    try
                    {
                        // Read the candidate marker and compare its GUID with the saved profile ID
                        string markerId = ReadValidMarkerId(candidateMarker);

                        if (Guid.TryParse(markerId, out Guid foundId) &&
                            foundId == expectedId)
                        {
                            // Save every matching destination so duplicate markers
                            // can be detected after all drives have been checked
                            matchingDestinations.Add(candidateDestination);
                        }
                    }
                    catch (InvalidDataException)
                    {
                        // The marker exists, but its JSON or marker ID is invalid
                        invalidMarkerFound = true;
                    }
                    catch (IOException)
                    {
                        // The marker exists, but it could not be read because of an I/O problem
                        invalidMarkerFound = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // The marker exists, but BackupUtility does not have permission to read it
                        invalidMarkerFound = true;
                    }
                }
                catch (IOException)
                {
                    // A drive can be disconnected while it is being inspected.
                    // Skip it and continue checking the remaining drives.
                }
                catch (UnauthorizedAccessException)
                {
                    // An inaccessible drive cannot be used as the backup destination.
                }
            }

            // More than one matching marker is unsafe because BackupUtility
            // cannot determine which destination the user intended
            if (matchingDestinations.Count > 1)
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.MultipleMatches,
                    Message = "Multiple matching backup destinations were found."
                };
            }

            // Exactly one matching marker means the destination was identified safely.
            // Return its current full path because its drive letter may have changed
            if (matchingDestinations.Count == 1)
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.Connected,
                    ResolvedDestinationPath = matchingDestinations[0],
                    Message = "Backup destination connected."
                };
            }

            // Only report an invalid marker when no valid matching destination was found
            if (invalidMarkerFound)
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.InvalidMarker,
                    Message = "A backup destination marker was found but could not be validated."
                };
            }

            // No matching destination marker was found on any available drive
            return new BackupDestinationDetectionResult
            {
                Status = BackupDestinationStatus.NotFound,
                Message = "Backup destination not found."
            };
        }

        /*
            Reads and validates one destination marker file.
            A valid marker must contain JSON that can be converted into a
            BackupDestinationMarker with a correctly formatted GUID.
        */
        private string ReadValidMarkerId(string markerFile)
        {
            try
            {
                // Read the marker JSON and convert it back into a marker object
                string json = File.ReadAllText(markerFile);
                BackupDestinationMarker? marker =
                    JsonSerializer.Deserialize<BackupDestinationMarker>(json);

                // Reject missing marker data or IDs that are not valid GUIDs
                if (marker == null || !Guid.TryParse(marker.MarkerId, out _))
                {
                    throw new InvalidDataException(
                        "The existing backup destination marker does not contain a valid ID."
                    );
                }

                return marker.MarkerId;
            }
            catch (JsonException ex)
            {
                // Never replace an invalid marker automatically. The user may need
                // to inspect it before deciding how the destination should be repaired.
                throw new InvalidDataException(
                    "The existing backup destination marker contains invalid JSON.",
                    ex
                );
            }
        }
    }
}
