# BackupUtility

BackupUtility is a Windows desktop backup application built with C# and WinForms on .NET 8. It copies files from one or more source folders to a verified backup destination while preserving folder structure, reporting progress, and recording backup history.

The current implementation provides normal incremental backup behavior; versioned backups and restore functionality are planned but are not implemented yet.

## Current features

- Add and remove multiple source folders.
- Prevent duplicate source-folder entries.
- Reject source folders with the same final folder name to prevent destination collisions.
- Select and save one default backup destination.
- Save the default backup profile as JSON and restore it at startup.
- Recursively scan all files beneath each source folder.
- Preserve each source folder's directory structure in the destination.
- Copy files that do not exist in the backup.
- Overwrite destination files when the source has a newer modification time.
- Skip unchanged files.
- Run file processing asynchronously so the WinForms interface remains responsive.
- Allow an in-progress backup to be cancelled cooperatively.
- Display progress, percentage, files scanned, files copied, files skipped, and errors.
- Continue processing when an individual file fails.
- Record failed file paths and error messages.
- Store completed backup history as JSON.
- Display history in a read-only, sortable window.
- Refresh or clear history, with confirmation before clearing.
- Identify the configured backup destination using a GUID marker instead of relying only on a drive letter.
- Automatically refresh destination status every two seconds.
- Refuse to start a backup unless exactly one valid destination marker is found.

## Requirements

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), or Visual Studio 2022 with the **.NET desktop development** workload

No third-party NuGet packages are currently required.

## Build and run

From the repository root:

```powershell
dotnet build .\BackupUtility\BackupUtility.sln
dotnet run --project .\BackupUtility\BackupUtility\BackupUtility.csproj
```

You can also open `BackupUtility/BackupUtility.sln` in Visual Studio and run the project from there.

## Using the application

1. Click **Add Folder** and select one or more source folders.
2. Click **Select Destination** and choose the folder that will hold the backup.
3. Click **Save Profile**.
   - BackupUtility saves the selected configuration.
   - It creates or reuses a destination marker in the selected folder.
4. Confirm that **Backup Destination Status** displays `Connected`.
5. Click **Start Backup**.
6. Follow progress and statistics on the main form, or click **Cancel Backup** to stop processing.
7. Click **View History** to inspect completed runs and individual file errors.

Do not place the backup destination inside one of the selected source folders. Doing so could cause the backup destination to be included in the recursive source scan.

Each source folder must also have a unique final folder name. For example, `C:\Work\Documents` and `D:\Personal\Documents` cannot be selected together because both would map to the same `Documents` folder in the backup. BackupUtility rejects the conflicting source instead of risking merged or overwritten files.

## Backup behavior

Each selected source folder is placed beneath the destination using the source folder's name. For example:

```text
Source:       C:\Users\Example\Documents
Destination:  E:\Backups

Result:       E:\Backups\Documents\...
```

For every source file, BackupUtility currently applies these rules:

1. If the destination file does not exist, copy it.
2. If the source modification time is newer, overwrite the destination file.
3. Otherwise, skip the file.
4. If one file fails, record the error and continue with the remaining files.

This is an incremental copy backup. Older versions of overwritten files are not currently retained.

## Cancelling a backup

While a backup is running, **Start Backup** is disabled and **Cancel Backup** is enabled. Clicking **Cancel Backup** requests cooperative cancellation. BackupUtility stops before processing the next file or source folder after it notices the request.

Cancellation does not roll back work that already succeeded:

- Files copied before cancellation remain in the destination.
- A file currently being copied normally finishes before cancellation takes effect because `File.Copy` cannot be interrupted mid-copy.
- Files that have not been processed are left for a later run.
- Cancellation is not counted as a file error.
- A cancelled run does not update the last successful backup time.
- A cancelled run is not added to completed backup history.

Starting the backup again is safe: previously copied unchanged files will be skipped, and remaining files will continue to be processed.

## Saved profile

BackupUtility currently stores one default profile containing:

- Source folder paths
- Original destination path
- Destination marker ID
- Destination path relative to its drive root

The profile is stored at:

```text
%LocalAppData%\BackupUtility\backupProfile.json
```

The drive-relative path allows BackupUtility to locate the same destination if Windows assigns its drive a different letter.

## Destination verification

When a profile is saved, BackupUtility creates or reuses this file inside the selected destination:

```text
.backuputility-destination.json
```

The file contains a GUID identifying that specific backup destination folder. It identifies the destination folder, not the physical drive or whether the device is an SSD.

BackupUtility scans ready fixed and removable drives for the saved relative path and matching marker. The UI can report:

- `Connected` — exactly one matching destination was found.
- `Not Found` — no matching destination was found.
- `Multiple Matches` — more than one matching marker was found; BackupUtility refuses to guess.
- `Invalid Marker` — an expected marker exists but cannot be validated.
- `Not Configured` — the current path does not have a saved marker configuration.

The status is refreshed automatically while the application is idle. A fresh verification is also required immediately before every backup. Automatic status checks do not replace normal exception handling because a drive can still be disconnected after verification.

## Backup history

Completed runs are appended to:

```text
%LocalAppData%\BackupUtility\backupHistory.json
```

Each entry contains:

- Completion time
- Source folders
- Resolved destination path
- Files scanned
- Files copied
- Files skipped
- Error count
- Failed file paths and error messages

The **View History** window displays summary rows with the newest entry first. Columns can be sorted by clicking their headers. Selecting a row displays its paths and error details.

**Clear All History** replaces the history with an empty JSON array after confirmation. It does not delete backup files, the saved profile, or the last-backup timestamp.

## Application data

| Data | Location |
| --- | --- |
| Default profile | `%LocalAppData%\BackupUtility\backupProfile.json` |
| Backup history | `%LocalAppData%\BackupUtility\backupHistory.json` |
| Destination marker | `<selected destination>\.backuputility-destination.json` |
| Last successful backup time | `backupSettings.json` in the application's current working directory |

The last-backup settings file currently uses a relative path, unlike the profile and history files. Moving it to the same Local AppData directory is a possible future cleanup.

## Project structure

```text
BackupUtility/
├── BackupUtility.sln
└── BackupUtility/
    ├── Program.cs
    ├── MainForm.cs
    ├── MainForm.Designer.cs
    ├── BackupService.cs
    ├── BackupProgress.cs
    ├── BackupError.cs
    ├── BackupSettings.cs
    ├── BackupProfile.cs
    ├── BackupHistoryEntry.cs
    ├── BackupHistoryForm.cs
    ├── BackupHistoryForm.Designer.cs
    ├── BackupDestinationService.cs
    ├── BackupDestinationMarker.cs
    ├── BackupDestinationStatus.cs
    └── BackupDestinationDetectionResult.cs
```

### Main responsibilities

- `MainForm` handles UI events, profile persistence, status updates, backup orchestration, and history persistence.
- `BackupService` scans, compares, copies, skips, and reports file-processing results.
- `BackupProgress` carries live and final backup statistics and failed-file information.
- `BackupHistoryForm` displays saved history and selected-run details.
- `BackupDestinationService` creates, validates, and locates destination markers.
- The remaining model classes represent profiles, history entries, errors, settings, markers, and detection results.

## Current limitations

- Only one default backup profile is supported.
- Changed destination files are overwritten; older versions are not retained.
- Restore functionality is not implemented.
- Files removed from a source are not deleted from the destination.
- Files are compared using modification timestamps rather than content hashes.
- Retention limits, compression, encryption, and scheduling are not implemented.
- Source folders with the same final folder name cannot currently be selected together. Source aliases may be considered in a future version.
- A failure while initially enumerating an entire source directory can stop that backup run; individual file-processing errors are handled separately.

## Roadmap

Completed:

- Phase 1 — Saved default backup profile
- Phase 2 — Backup history and error logging
- Phase 3 — Backup destination detection and enforcement

Planned:

- Phase 4 — Optional versioned backups with older changed-file versions retained separately
- Phase 5 — Safe file and folder restore workflow

Versioning design will be agreed upon before the current overwrite behavior is changed.
