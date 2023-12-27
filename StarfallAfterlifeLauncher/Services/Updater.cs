using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Launcher.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace StarfallAfterlife.Launcher.Services
{
    public static class Updater
    {
        public static Task<Relese> GetLatestRelese()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    var requests = new Task<Relese>[]
                    {
                        new GitHubRepo().GetLatestRelese(),
                        new GitFlicRepo().GetLatestRelese(),
                    };

                    Task.WaitAll(requests, TimeSpan.FromSeconds(10));

                    var releses = requests
                        .Where(r => r.IsCompleted == true && r.Result is not null)
                        .Select(r => r.Result)
                        .OrderBy(r => r.Version)
                        .ToList();

                    return releses.LastOrDefault();
                });
            }
            catch { }

            return null;
        }

        public record Relese(
            Version Version,
            Uri DownloadLink,
            long Size,
            string FileName,
            string ReleseName,
            string Description,
            UpdateRepo Repo)
        {
            public bool IsUpdateRequired =>
                Version is not { Major: 0, Minor: 0, Build: 0 } &&
                Version > CurrentVersion.Value;

            public bool IsDownloaded { get; protected set; }
            
            protected string _updateLocation;
            private static Lazy<Version> CurrentVersion { get; } = new(
                () => Assembly.GetAssembly(typeof(Program)).GetName().Version);

            public Task<bool> Download(
                Action<long> lengtUpdated = null,
                IProgress<long> progress = null,
                CancellationToken ct = default)
            {
                return Task.Factory.StartNew(() =>
                {
                    string file = null;
                    IsDownloaded = false;

                    try
                    {
                        var dir = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "Update");
                        file = Path.Combine(dir, FileName);

                        if (Directory.Exists(dir) == false)
                            Directory.CreateDirectory(dir);

                        using var client = new HttpClient();
                        var response = client.GetAsync(DownloadLink, HttpCompletionOption.ResponseHeadersRead).Result;
                        var length = response.Content.Headers.ContentLength;
                        using var inputStream = response.Content.ReadAsStreamAsync(ct).Result;
                        using var outputStream = File.Open(file, FileMode.Create);
                        var buffer = new byte[524288];
                        long totalBytes = 0;
                        int bytesRead;

                        lengtUpdated?.Invoke(length ?? 0);
                        progress?.Report(0);

                        while ((bytesRead = inputStream.ReadAsync(buffer, 0, buffer.Length, ct).Result) > 0)
                        {
                            outputStream.WriteAsync(buffer, 0, bytesRead, ct).Wait();
                            totalBytes += bytesRead;
                            progress?.Report(totalBytes);
                        }

                    }
                    catch
                    {
                        return true;
                    }

                    _updateLocation = file;
                    IsDownloaded = true;
                    return false;
                });
            }

            public Task<bool> Install()
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (_updateLocation is null ||
                            File.Exists(_updateLocation) == false)
                            return false;

                        var exeLocation = Environment.ProcessPath;
                        var cmd = $"Taskkill /PID \"{Environment.ProcessId}\" /F";
                        cmd += $"&&msiexec /i \"{_updateLocation}\" /passive";

                        var psi = new ProcessStartInfo("cmd.exe", "/C  " + cmd)
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Normal,
                        };

                        Process.Start(psi);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                });
            }
        }

        public abstract class UpdateRepo
        {
            public abstract Task<Relese> GetLatestRelese();
        }

        public class GitHubRepo : UpdateRepo
        {
            public const string Request = "https://api.github.com/repos/MenY-dev/StarfallAfterlife/releases";
            public const string FileName = "starfall-afterlife-setup-win-x64";

            public override Task<Relese> GetLatestRelese()
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        using var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("User-Agent", "MenY-dev");

                        var response = client
                            .GetAsync(Request).Result.Content
                            .ReadAsStringAsync().Result;

                        var doc = JsonHelpers.ParseNodeUnbuffered(response);

                        if (doc?.AsArraySelf() is JsonArray array)
                        {
                            var releases = new List<Relese>();

                            foreach (var node in array)
                            {
                                if (node is not null &&
                                    (bool?)node["prerelease"] is not true &&
                                    (string)node["tag_name"] is string tagName &&
                                    Version.TryParse(tagName.Trim('v', ' '), out var version) == true &&
                                    node["assets"].AsArraySelf() is JsonArray assets)
                                {
                                    var name = (string)node["name"];
                                    var description = (string)node["body"];

                                    foreach (var asset in assets)
                                    {
                                        if (asset is not null &&
                                            (string)asset["name"] is string fileName &&
                                            fileName.StartsWith(FileName) &&
                                            (string)asset["browser_download_url"] is string link &&
                                            (int?)asset["size"] is int size &&
                                            Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out var uri) == true)
                                        {
                                            releases.Add(new(
                                                version,
                                                uri,
                                                size,
                                                fileName,
                                                name,
                                                description,
                                                this));

                                            break;
                                        }
                                    }
                                }
                            }

                            return releases.OrderBy(i => i.Version).LastOrDefault();
                        }
                    }
                    catch { }

                    return null;
                });
            }
        }

        public class GitFlicRepo : UpdateRepo
        {
            public override Task<Relese> GetLatestRelese()
            {
                return Task.FromResult<Relese>(null);
            }
        }
    }
}
