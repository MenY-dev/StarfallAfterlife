using Mono.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking
{
    public static class NatPuncher
    {
        private static readonly List<INatDevice> _devices = new();
        private static readonly List<Mapping> _openPorts = new();
        private static readonly object _locker = new();

        public static void Init()
        {
            if (NatUtility.IsSearching == true)
                return;

            NatUtility.DeviceFound -= DeviceFound;
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.StartDiscovery();
        }

        private static void DeviceFound(object sender, DeviceEventArgs e)
        {
            lock (_locker)
            {
                try
                {
                    var device = e.Device;

                    _devices.RemoveAll(d =>
                        d.NatProtocol == device.NatProtocol &&
                        d.DeviceEndpoint == device.DeviceEndpoint);

                    _devices.Add(device);

                    foreach (var item in _openPorts)
                        SendToDevice(device, item);
                }
                catch { }
            }
        }

        public static Task<bool> Create(ProtocolType protocol, int privatePort, int publicPort, int lifetime = 0)
        {
            lock (_locker)
            {
                Init();

                var mapping = new Mapping(
                    protocol == ProtocolType.Udp ? Protocol.Udp : Protocol.Tcp,
                    privatePort,
                    publicPort,
                    lifetime,
                    "StarfallAfterlife");

                SaveMapping(mapping);
                return SendToAllDevices(mapping);
            }
        }

        public static Task CloseAll()
        {
            var ports = new List<Mapping>();

            lock (_locker)
                ports.AddRange(_openPorts);

            var requests = ports.Select(port => Close(
                port.Protocol switch { Protocol.Tcp => ProtocolType.Tcp, _ => ProtocolType.Udp },
                port.PrivatePort,
                port.PublicPort)).ToArray();

            return Task.WhenAll(requests);
        }

        public static Task Close(ProtocolType protocol, int privatePort, int publicPort)
        {
            lock (_locker)
            {
                var mapping = new Mapping(
                    protocol == ProtocolType.Udp ? Protocol.Udp : Protocol.Tcp,
                    privatePort,
                    publicPort);

                try
                {
                    foreach (var port in _openPorts?.ToArray())
                    {
                        if (port.Protocol == mapping.Protocol &&
                            port.PublicPort == mapping.PublicPort &&
                            port.PrivatePort == mapping.PrivatePort)
                        {
                            _openPorts.Remove(port);
                        }
                    }
                }
                catch { }

                var devices = _devices.ToArray();

                var requests = devices.Select(device => Task.Factory.StartNew(() =>
                {
                    try
                    {
                        device.DeletePortMapAsync(mapping).Wait();
                    }
                    catch { }
                })).ToArray();

                return Task.WhenAll(requests);
            }
        }

        private static Task<bool> SendToDevice(INatDevice device, Mapping mapping)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    lock (_locker)
                        device.CreatePortMap(mapping);

                    return true;
                }
                catch { }

                if (mapping.Lifetime != 0)
                {
                    try
                    {
                        lock (_locker)
                            device.CreatePortMap(new Mapping(
                                mapping.Protocol, mapping.PublicPort,
                                mapping.PublicPort, 0, mapping.Description));

                        return true;
                    }
                    catch { }
                }

                return false;
            });
        }

        private static Task<bool> SendToAllDevices(Mapping mapping)
        {
            lock (_locker)
            {
                var completion = new TaskCompletionSource<bool>();
                var isSet = false;

                var requests = _devices.Select(device =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var result = SendToDevice(device, mapping).Result;

                            lock (_locker)
                            {
                                if (result == true && isSet == false)
                                {
                                    completion.SetResult(true);
                                    isSet = true;
                                }
                            }
                        }
                        catch { }
                    });
                }).ToArray();

                Task.WhenAll(requests).ContinueWith(_ =>
                {
                    if (isSet == false)
                        completion.SetResult(false);
                });

                return completion.Task;
            }
        }

        private static void SaveMapping(Mapping mapping)
        {
            lock (_locker)
            {
                var ports = _openPorts?.ToArray();
                var needSave = true;

                foreach (var item in ports)
                {
                    if (item.PrivatePort == mapping.PrivatePort &&
                        item.PublicPort == mapping.PublicPort)
                    {
                        if (item.Expiration > mapping.Expiration)
                            needSave = false;
                        else
                            _openPorts.Remove(item);
                    }
                }

                if (needSave == true)
                    _openPorts.Add(mapping);
            }
        }

        public static Task<(IPAddress Internal, IPAddress External)[]> GetExternalAdresses()
        {
            Task<(IPAddress, IPAddress)>[] requests;

            lock (_locker)
            {
                requests = _devices.Select(d => Task.Factory.StartNew(() =>
                {
                    var result = d.GetExternalIP();
                    return (d.DeviceEndpoint?.Address, result);
                })).ToArray();
                
                return Task.Factory.StartNew(() =>
                {
                    Task.WaitAll(requests);

                    return requests
                        .Where(r => r.IsCompleted == true)
                        .Select(r => r.Result)
                        .ToArray();
                });
            }
        }
    }
}
