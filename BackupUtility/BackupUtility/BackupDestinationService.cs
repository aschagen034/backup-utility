using System.Text.Json;

namespace BackupUtility
{
    internal class BackupDestinationService
    {
        private const string MarkerFileName = ".backuputility-destination.json";

        public string GetOrCreateMarkerId(string destinationPath)
        {
            if (!Directory.Exists(destinationPath))
            {
                throw new DirectoryNotFoundException(
                    "The selected backup destination does not exist or is not currently available."
                );
            }

            string markerFile = Path.Combine(destinationPath, MarkerFileName);

            if (File.Exists(markerFile))
            {
                return ReadValidMarkerId(markerFile);
            }

            BackupDestinationMarker marker = new BackupDestinationMarker
            {
                MarkerId = Guid.NewGuid().ToString()
            };

            string json = JsonSerializer.Serialize(marker, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(markerFile, json);
            return marker.MarkerId;
        }

        public string GetDestinationRelativePath(string destinationPath)
        {
            string fullDestinationPath = Path.GetFullPath(destinationPath);
            string? driveRoot = Path.GetPathRoot(fullDestinationPath);

            if (string.IsNullOrEmpty(driveRoot))
            {
                throw new InvalidOperationException(
                    "The selected backup destination does not have a valid drive root."
                );
            }

            string relativePath = Path.GetRelativePath(driveRoot, fullDestinationPath);

            // Path.GetRelativePath returns "." when the destination is the drive root.
            return relativePath == "." ? string.Empty : relativePath;
        }

        public BackupDestinationDetectionResult DetectDestination(
            string expectedMarkerId,
            string destinationRelativePath)
        {
            if (!Guid.TryParse(expectedMarkerId, out Guid expectedId))
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.InvalidMarker,
                    Message = "The saved profile does not contain a valid destination marker ID."
                };
            }

            if (Path.IsPathRooted(destinationRelativePath))
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.InvalidMarker,
                    Message = "The saved destination-relative path is invalid."
                };
            }

            List<string> matchingDestinations = new List<string>();
            bool invalidMarkerFound = false;

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

                    string candidateMarker = Path.Combine(
                        candidateDestination,
                        MarkerFileName
                    );

                    if (!File.Exists(candidateMarker))
                    {
                        continue;
                    }

                    try
                    {
                        string markerId = ReadValidMarkerId(candidateMarker);

                        if (Guid.TryParse(markerId, out Guid foundId) &&
                            foundId == expectedId)
                        {
                            matchingDestinations.Add(candidateDestination);
                        }
                    }
                    catch (IOException)
                    {
                        invalidMarkerFound = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
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

            if (matchingDestinations.Count > 1)
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.MultipleMatches,
                    Message = "Multiple matching backup destinations were found."
                };
            }

            if (matchingDestinations.Count == 1)
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.Connected,
                    ResolvedDestinationPath = matchingDestinations[0],
                    Message = "Backup destination connected."
                };
            }

            if (invalidMarkerFound)
            {
                return new BackupDestinationDetectionResult
                {
                    Status = BackupDestinationStatus.InvalidMarker,
                    Message = "A backup destination marker was found but could not be validated."
                };
            }

            return new BackupDestinationDetectionResult
            {
                Status = BackupDestinationStatus.NotFound,
                Message = "Backup destination not found."
            };
        }

        private string ReadValidMarkerId(string markerFile)
        {
            try
            {
                string json = File.ReadAllText(markerFile);
                BackupDestinationMarker? marker =
                    JsonSerializer.Deserialize<BackupDestinationMarker>(json);

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
