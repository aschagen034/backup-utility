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
