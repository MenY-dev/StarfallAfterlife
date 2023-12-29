using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {
        public string ServerListFile => Path.Combine(WorkingDirectory, "Launcher", "ServerList.json");

        public List<RemoteServerInfo> ServerList { get; protected set; } = new();

        private readonly object _serverListLocker = new();

        private CancellationTokenSource _serverListUpdateCTS;

        public (bool Result, string Reason) AddServer(string address)
        {
            try
            {
                if (address is null ||
                    IPEndPoint.TryParse(address, out _) == false)
                    return (false, "bad_address");

                var list = ServerList ??= new();

                if (list.ToArray().Any(s => s is not null && s.Address == address))
                    return (false, "already_exist");

                list.Add(new() { Address = address });
                return (true, null);
            }
            catch { }

            return (false, "internal_error");
        }

        public Task UpdateServerList(IProgress<RemoteServerInfo> progress = null)
        {
            lock (_serverListLocker)
            {
                if (_serverListUpdateCTS is not null)
                    _serverListUpdateCTS.Cancel();

                var cts = _serverListUpdateCTS = new();

                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var servers = (ServerList ??= new()).ToArray();
                        var requests = servers
                            .Where(s => s is not null)
                            .Select(s => s.Update(5000, cts.Token).ContinueWith(t => progress.Report(s)))
                            .ToArray();

                        Task.WaitAll(requests, 5050, cts.Token);
                    }
                    catch { }

                    lock (_serverListLocker)
                    {
                        if (_serverListUpdateCTS == cts)
                            _serverListUpdateCTS = null;
                    }
                }, cts.Token);
            }
        }

        public void SaveServerList()
        {
            try
            {
                if (ServerListFile is string path &&
                    JsonHelpers.SerializeUnbuffered(ServerList ??= new(), new() { WriteIndented = true }) is string text)
                {
                    if (Path.GetDirectoryName(path) is string dir &&
                            Directory.Exists(dir) == false)
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(path, text);
                }
            }
            catch { }
        }

        public void LoadServerList()
        {
            try
            {
                if (ServerListFile is string path &&
                    File.Exists(path) == true &&
                    File.ReadAllText(path) is string text &&
                    JsonHelpers.DeserializeUnbuffered<List<RemoteServerInfo>>(text) is List<RemoteServerInfo> list)
                {
                    ServerList = list.Where(i => i is not null).ToList() ?? new();
                }
            }
            catch { }
        }
    }
}
