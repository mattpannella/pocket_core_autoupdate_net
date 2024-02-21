using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;

namespace Pannella.Services;

[UnconditionalSuppressMessage("Trimming",
    "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
    Justification = "<Pending>")]
public static class AssetsService
{
    private const string BLACKLIST = "https://raw.githubusercontent.com/mattpannella/pupdate/main/blacklist.json";

    public static void BackupSaves(string directory, string backupLocation)
    {
        BackupDirectory(directory, "Saves", backupLocation);
    }

    public static void BackupMemories(string directory, string backupLocation)
    {
        BackupDirectory(directory, "Memories", backupLocation);
    }
    private static void BackupDirectory(string rootDirectory, string folderName, string backupLocation)
    {
        if (string.IsNullOrEmpty(rootDirectory))
        {
            throw new ArgumentNullException(nameof(rootDirectory));
        }

        if (string.IsNullOrEmpty(backupLocation))
        {
            throw new ArgumentNullException(nameof(backupLocation));
        }

        Console.WriteLine($"Compressing and backing up {folderName} directory...");
        string savesPath = Path.Combine(rootDirectory, folderName);
        string fileName = $"{folderName}_Backup_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.zip";
        string archiveName = Path.Combine(backupLocation, fileName);

        if (Directory.Exists(savesPath))
        {
            if (!Directory.Exists(backupLocation))
            {
                Directory.CreateDirectory(backupLocation);
            }

            ZipFile.CreateFromDirectory(savesPath, archiveName);
            Console.WriteLine("Complete.");
        }
        else
        {
            Console.WriteLine($"No {folderName} directory found, skipping backup...");
        }
    }

    public static List<string> GetBlacklist()
    {
#if DEBUG
        string json = File.ReadAllText("blacklist.json");
#else
        string json = GlobalHelper.SettingsManager.GetConfig().use_local_blacklist
            ? File.ReadAllText("blacklist.json")
            : HttpHelper.Instance.GetHTML(BLACKLIST);
#endif
        var files = JsonSerializer.Deserialize<List<string>>(json);

        return files ?? new List<string>();
    }
}
