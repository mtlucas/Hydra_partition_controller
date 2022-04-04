﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using LinuxHydraPartitionController.Api.WebHost.Models;

namespace LinuxHydraPartitionController.Api.WebHost
{
    public class Metrics
    {
        private MachineMetrics machineMetrics = new MachineMetrics();
        private CPU cPU = new CPU();
        private MemoryInMB memoryInMB = new MemoryInMB();
        private UptimeInSeconds uptimeInSeconds = new UptimeInSeconds();

        private readonly ILogger<Metrics> _logger;

        public Metrics(ILogger<Metrics> logger)
        {
            _logger = logger;
        }
        public MachineMetrics GetMetrics()
        {
            var manageProcess = new ManageProcess(_logger);
            ProcessStartInfo machineMetricsProcessStartInfo = manageProcess.BuildProcessStartInfo("/usr/bin/nproc;/usr/bin/cat /proc/loadavg|/usr/bin/awk '{print $1\\\"\\n\\\"$2\\\"\\n\\\"$3}'");
            string[] cpuLines = manageProcess.Execute(machineMetricsProcessStartInfo).Split("\n");
            if (cpuLines.Length == 4)
            {
                cPU.Cores = Convert.ToInt32(cpuLines[0].ToString());
                cPU.Load1min = Convert.ToSingle(cpuLines[1].ToString());
                cPU.Load5min = Convert.ToSingle(cpuLines[2].ToString());
                cPU.Load15min = Convert.ToSingle(cpuLines[3].ToString());
            }
            else
            {
                _logger.Log(LogLevel.Warning, $"WARNING: CPU Metrics cmd results failed --> Output: {cpuLines}");
            }
            machineMetricsProcessStartInfo = manageProcess.BuildProcessStartInfo("/usr/bin/free -m|/usr/bin/head -2|/usr/bin/tail -n +2|/usr/bin/awk '{print $2\\\"\\n\\\"$3\\\"\\n\\\"$4\\\"\\n\\\"$6\\\"\\n\\\"$7}'");
            string[] memLines = manageProcess.Execute(machineMetricsProcessStartInfo).Split("\n");
            if (memLines.Length == 5)
            {
                memoryInMB.Total = Convert.ToInt32(memLines[0].ToString());
                memoryInMB.Used = Convert.ToInt32(memLines[1].ToString());
                memoryInMB.Free = Convert.ToInt32(memLines[2].ToString());
                memoryInMB.Buffers = Convert.ToInt32(memLines[3].ToString());
                memoryInMB.Available = Convert.ToInt32(memLines[4].ToString());
            }
            else
            {
                _logger.Log(LogLevel.Warning, $"WARNING: Memory Metrics cmd results failed --> Output: {memLines}");
            }
            machineMetricsProcessStartInfo = manageProcess.BuildProcessStartInfo("/usr/bin/cat /proc/uptime|/usr/bin/awk '{print int($1)}'");
            string[] uptimeLines = manageProcess.Execute(machineMetricsProcessStartInfo).Split("\n");
            if (uptimeLines.Length == 1)
            {
                uptimeInSeconds.Uptime = Convert.ToInt32(uptimeLines[0].ToString());
            }
            else
            {
                _logger.Log(LogLevel.Warning, $"WARNING: Uptime Metrics cmd results failed --> Output: {uptimeLines}");
            }
            machineMetrics.CPU = cPU;
            machineMetrics.MemoryInMB = memoryInMB;
            machineMetrics.UptimeInSeconds = uptimeInSeconds;
            return machineMetrics;
        }
    }
}
