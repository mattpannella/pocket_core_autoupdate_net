using System.IO.Compression;
using Pannella.Helpers;
using Pannella.Models.OpenFPGA_Cores_Inventory;
using AnalogueCore = Pannella.Models.Analogue.Core.Core;
using GithubFile = Pannella.Models.Github.File;

namespace Pannella.Services;

public partial class CoresService
{
    private const string BETA_KEY_FILENAME = "jtbeta.zip";
    private const string EXTRACT_LOCATION = "betakeys";

    private Dictionary<string, string> renamedPlatformFiles;

    public Dictionary<string, string> RenamedPlatformFiles
    {
        get { return renamedPlatformFiles ??= this.LoadRenamedPlatformFiles(); }
    }

    private Dictionary<string, string> LoadRenamedPlatformFiles()
    {
        Dictionary<string, string> platformFiles = new();

        try
        {
            List<GithubFile> files = GithubApiService.GetFiles("dyreschlock", "pocket-platform-images",
                "arcade/Platforms", this.settingsService.GetConfig().github_token);

            foreach (var file in files)
            {
                string url = file.download_url;
                string filename = file.name;

                if (filename.EndsWith(".json"))
                {
                    string platform = Path.GetFileNameWithoutExtension(filename);

                    platformFiles.Add(platform, url);
                }
            }
        }
        catch (Exception e)
        {
            WriteMessage("Unable to retrieve archive contents. Asset download may not work.");
#if DEBUG
            WriteMessage(e.ToString());
#else
            WriteMessage(e.Message);
#endif
        }

        return platformFiles;
    }

    public (bool, string, int) IsBetaCore(string identifier)
    {
        var data = this.ReadDataJson(identifier);
        var slot = data.data.data_slots.FirstOrDefault(x => x.name == "JTBETA");

        return slot != null
            ? (true, slot.id, slot.GetPlatformIdIndex())
            : (false, null, 0);
    }

    public void CopyBetaKey(Core core)
    {
        AnalogueCore info = this.ReadCoreJson(core.identifier);
        string path = Path.Combine(
            this.installPath,
            "Assets",
            info.metadata.platform_ids[core.beta_slot_platform_id_index],
            "common");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string keyPath = Path.Combine(this.installPath, EXTRACT_LOCATION);

        if (Directory.Exists(keyPath) && Directory.Exists(path))
        {
            Util.CopyDirectory(keyPath, path, false, true);
            WriteMessage("Beta key copied to common directory.");
        }
    }

    public bool ExtractBetaKey()
    {
        string keyPath = Path.Combine(this.installPath, EXTRACT_LOCATION);
        string file = Path.Combine(this.installPath, BETA_KEY_FILENAME);

        if (File.Exists(file))
        {
            WriteMessage("Extracting JT beta key...");
            ZipFile.ExtractToDirectory(file, keyPath, true);

            return true;
        }

        return false;
    }

    public void DeleteBetaKey()
    {
        string keyPath = Path.Combine(this.installPath, EXTRACT_LOCATION);

        if (Directory.Exists(keyPath))
            Directory.Delete(keyPath, true);
    }
}
