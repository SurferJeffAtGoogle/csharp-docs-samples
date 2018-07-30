﻿/*
 * Copyright (c) 2018 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Monitoring.V3;
using CommandLine;
using Google.Api.Gax;
using Google.Api;
using Newtonsoft.Json.Linq;
using static Google.Api.MetricDescriptor.Types;
using Google.Protobuf.WellKnownTypes;
using System.Net;

namespace GoogleCloudSamples
{
    class OptionsWithProjectId
    {
        [Option('p', HelpText = "The project ID of the project to use for monitoring operations.", Required = true)]
        public string ProjectId { get; set; }
    }

    [Verb("create", HelpText = "Create an uptime check.")]
    class CreateOptions :OptionsWithProjectId
    {
        [Option('h', HelpText = "Host name.")]
        public string HostName { get; set; } = "example.com";
        [Option('d', HelpText = "Display name.")]
        public string DisplayName { get; set; } = "New uptime check";
    }

    [Verb("delete", HelpText = "Delete an uptime check.")]
    class DeleteOptions
    {
        [Value('0', HelpText = "The full name of the config to delete. " +
            "Example: projects/my-project/uptimeCheckConfigs/my-config-name",
            Required = true)]
        public string ConfigName { get; set; }
    }

    [Verb("list", HelpText = "List metric descriptors for this project.")]
    class ListOptions : OptionsWithProjectId
    {
    }

    [Verb("list-ips", HelpText = "List IP addresses of uptime-check servers.")]
    class ListIpsOptions {}
    
    public class UptimeCheck
    {
        private static readonly DateTime s_unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // [START monitoring_uptime_check_create]
        public static object CreateUptimeCheck(string projectId, string hostName,
            string displayName)            
        {
            // Define a new config.
            var config = new UptimeCheckConfig()
            {
                DisplayName = displayName,
                MonitoredResource = new MonitoredResource()
                {
                    Type = "uptime_url",
                    Labels = {{"host", hostName}}
                },
                HttpCheck = new UptimeCheckConfig.Types.HttpCheck()
                {
                    Path = "/",
                    Port = 80,
                },
                Timeout = TimeSpan.FromSeconds(10).ToDuration(),
                Period = TimeSpan.FromMinutes(5).ToDuration()
            };
            // Create a client.
            var client = UptimeCheckServiceClient.Create();
            string projectName = new ProjectName(projectId).ToString();
            // Create the config.
            var newConfig = client.CreateUptimeCheckConfig(projectName, config);
            Console.WriteLine(newConfig.Name);
            return 0;
        }
        // [END monitoring_uptime_check_create]

        // [START monitoring_uptime_check_delete]
        public static object DeleteUptimeCheckConfig(string configName)
        {
            var client = UptimeCheckServiceClient.Create();
            client.DeleteUptimeCheckConfig(configName);
            Console.WriteLine($"Deleted {configName}");
            return 0;
        }
        // [END monitoring_uptime_check_delete]


        // [START monitoring_uptime_check_list_configs]
        public static object ListUptimeCheckConfigs(string projectId)
        {
            var client = UptimeCheckServiceClient.Create();
            var configs = client.ListUptimeCheckConfigs(
                new ProjectName(projectId).ToString());
            foreach (UptimeCheckConfig config in configs)
            {
                Console.WriteLine(config.Name);
            }
            return 0;
        }
        // [END monitoring_uptime_check_list_configs]

        // [START monitoring_uptime_check_list_ips]
        public static object ListUptimeCheckIps()
        {
            var client = UptimeCheckServiceClient.Create();
            var ips = client.ListUptimeCheckIps(new ListUptimeCheckIpsRequest());
            foreach (UptimeCheckIp ip in ips)
            {
                Console.WriteLine("{0,20} {1}", ip.IpAddress, ip.Location);
            }
            return 0;
        }
        // [END monitoring_uptime_check_list_ips]

        public static void Main(string[] args)
        { 
            var verbMap = new VerbMap<int>();
            verbMap
                .Add((CreateOptions opts) => CreateUptimeCheck(opts.ProjectId,
                        opts.HostName, opts.DisplayName))
                .Add((DeleteOptions opts) => DeleteUptimeCheckConfig(opts.ConfigName))
                .Add((ListOptions opts) => ListUptimeCheckConfigs(opts.ProjectId))
                .Add((ListIpsOptions opts) => ListUptimeCheckIps())
                .NotParsedFunc = (err) => 255;
            verbMap.Run(args);
        }
    }
}
