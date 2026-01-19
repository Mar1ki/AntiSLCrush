using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport), nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    internal static class ProcessConnectionRequestPatch
    {
        private static readonly Dictionary<string, List<float>> _hexCount = new Dictionary<string, List<float>>();
        private static readonly Dictionary<string, List<float>> _ipCount = new Dictionary<string, List<float>>();

        private static bool Prefix(ConnectionRequest request)
        {
            string ip = request.RemoteEndPoint.Address.ToString();

            if (Main.BannedIp.Contains(ip))
                return false;

            var dataSizeField = typeof(NetDataReader).GetField("_dataSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int dataSize = (int)dataSizeField.GetValue(request.Data);

            byte[] data = new byte[request.Data.AvailableBytes];
            Buffer.BlockCopy(request.Data._data, request.Data._position, data, 0, request.Data.AvailableBytes);
            string hex = BitConverter.ToString(data).Replace("-", "");

            if (Main.BannedHEX.Contains(hex))
                return false;

            if (!_ipCount.TryGetValue(ip, out List<float> times))
                times = new List<float>();

            times.Add(Time.time);

            ushort count = 0;

            foreach (float time in times.ToArray())
            {
                if (time + 1 > Time.time)
                    count++;
                else
                    times.Remove(time);
            }

            if (count > 5)
            {
                if (Main.config.BanIp)
                    Main.BanIpAtSystemLevel(ip, "Too many HEX from same IP");

                return false;
            }

            _ipCount[ip] = times;

            if (!_hexCount.TryGetValue(hex, out times))
                times = new List<float>();

            times.Add(Time.time);

            count = 0;

            foreach (float time in times.ToArray())
            {
                if (time + 1 > Time.time)
                    count++;
                else
                    times.Remove(time);
            }

            if (count > 5)
            {
                if (Main.config.BanHex)
                    Main.BanHexAtSystemLevel(hex, dataSize, "Too many HEX.");

                return false;
            }

            _hexCount[hex] = times;

            return true;
        }
    }
}