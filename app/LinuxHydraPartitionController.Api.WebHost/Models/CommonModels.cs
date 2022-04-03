﻿using System;
using System.Collections.Generic;


namespace LinuxHydraPartitionController.Api.WebHost.Models
{
    // GosConfig.json data models
    public class GosConfig
    {
        public List<MachineConfig> machines { get; set; }
    }
    public class MachineConfig
    {
        public List<PartitionConfig> partitions { get; set; }
    }
    public class PartitionConfig
    {
        public int partition { get; set; }
    }

    // Endpoint data model
    public class Endpoint
    {
        public string Path { get; }
        public string Method { get; }

        internal Endpoint(string path, string method)
        {
            Path = path;
            Method = method;
        }
    }

    // Machine Metrics data models
    public class MachineMetrics
    {
        public CPU CPU { get; set; }
        public MemoryInMB MemoryInMB { get; set; }
        public UptimeInSeconds UptimeInSeconds { get; set; }
    }
    public class CPU
    {
        public int Cores { get; set; }
        public float Load1min { get; set; }
        public float Load5min { get; set; }
        public float Load15min { get; set; }
    }
    public class MemoryInMB
    {
        public int Total { get; set; }
        public int Used { get; set; }
        public int Free { get; set; }
        public int Buffers { get; set; }
        public int Available { get; set; }
    }
    public class UptimeInSeconds
    {
        public int Uptime { get; set; }
    }
}
