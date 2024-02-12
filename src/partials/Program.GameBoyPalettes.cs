using System.IO.Compression;
using Pannella.Helpers;
using Pannella.Models.Github;
using Pannella.Services;
using File = System.IO.File;

namespace Pannella;

internal partial class Program
{
    private static async Task DownloadGameBoyPalettes(string directory)
    {
        Release release = await GithubApiService.GetLatestRelease("davewongillies", "openfpga-palettes");
        Asset asset = release.assets.FirstOrDefault(a => a.name.EndsWith(".zip"));

        if (asset != null)
        {
            string localFile = Path.Combine(directory, asset.name);
            string extractPath = Path.Combine(directory, "temp");

            try
            {
                Console.WriteLine($"Downloading asset '{asset.name}'...");
                await HttpHelper.Instance.DownloadFileAsync(asset.browser_download_url, localFile);
                Console.WriteLine("Download complete.");
                Console.WriteLine("Installing...");

                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                ZipFile.ExtractToDirectory(localFile, extractPath);
                File.Delete(localFile);
                Util.CopyDirectory(extractPath, directory, true, true);

                Directory.Delete(extractPath, true);
                Console.WriteLine("Complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something happened while trying to install the asset files...");
                Console.WriteLine(ex);
            }
        }
    }
}
