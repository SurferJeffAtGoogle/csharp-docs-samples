﻿// Copyright(c) 2017 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations under
// the License.

using System.IO;
using Xunit;
using System;
using System.Linq;
using System.Net;

namespace GoogleCloudSamples
{
    // <summary>
    /// Runs the sample app's methods and tests the outputs
    // </summary>
    public class CommonTests
    {
        private static readonly string s_projectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");

        readonly CommandLineRunner _cloudMonitoring = new CommandLineRunner()
        {
            VoidMain = Monitoring.Main,
            Command = "Monitoring"
        };

        protected ConsoleOutput Run(params string[] args)
        {
            return _cloudMonitoring.Run(args);
        }

        private readonly RetryRobot _retryRobot = new RetryRobot()
        {
            RetryWhenExceptions = new[]
            {
                typeof(Xunit.Sdk.XunitException)
            }
        };

        /// <summary>
        /// Retry action.
        /// For tests that create an entity and then query it afterward, 
        /// but may not find it immediately.
        /// </summary>
        /// <param name="action"></param>
        private void Eventually(Action action) => _retryRobot.Eventually(action);

        [Fact]
        public void TestListMetricDescriptors()
        {
            string testMetricName = "compute.googleapis.com/instance/cpu/utilization";
            var output = _cloudMonitoring.Run("list", s_projectId);
            Assert.Equal(0, output.ExitCode);
            Assert.Contains(testMetricName, output.Stdout);
        }

        [Fact]
        public void TestCreateCustomMetric()
        {
            var output = _cloudMonitoring.Run("create", s_projectId);
            Assert.Equal(0, output.ExitCode);
            Assert.Contains("metricKind", output.Stdout);
        }

        [Fact]
        public void TestGetMetricDescriptor()
        {
            _cloudMonitoring.Run("create", s_projectId);
            var output = _cloudMonitoring.Run("get", s_projectId,
                "custom.googleapis.com/stores/daily_sales");
            Assert.Equal(0, output.ExitCode);
            Assert.Contains("metricKind", output.Stdout);
        }

        [Fact]
        public void TestWriteTimeSeriesData()
        {
            var output = _cloudMonitoring.Run("write", s_projectId);
            Assert.Equal(0, output.ExitCode);
            Assert.Contains("Pittsburgh", output.Stdout);
        }

        [Fact]
        public void TestListMonitoredResourceDescriptors()
        {
            string testResourceName = "monitoredResourceDescriptors/api";
            var output = _cloudMonitoring.Run("listResources", s_projectId);
            Assert.Equal(0, output.ExitCode);
            Assert.Contains(testResourceName, output.Stdout);
        }

        [Fact]
        public void TestGetMonitoredResourceDescriptor()
        {
            string testResourceDisplayName = "Produced API";
            var output = _cloudMonitoring.Run("getResource", s_projectId, "api");
            Assert.Equal(0, output.ExitCode);
            Assert.Contains(testResourceDisplayName, output.Stdout);
        }

        [Fact]
        public void TestReadTimeSeriesData()
        {
            _cloudMonitoring.Run("write", s_projectId);
            var output = _cloudMonitoring.Run("read", s_projectId,
                "custom.googleapis.com/stores/daily_sales");
            Assert.Equal(0, output.ExitCode);
            Assert.Contains("123.45", output.Stdout);
        }

        [Fact]
        public void TestReadTimeSeriesDataFields()
        {
            _cloudMonitoring.Run("write", s_projectId);
            var output = _cloudMonitoring.Run("readFields", s_projectId,
                "custom.googleapis.com/stores/daily_sales");
            Assert.Equal(0, output.ExitCode);
            Assert.DoesNotContain("123.45", output.Stdout);
            Assert.Contains("Pittsburgh", output.Stdout);
        }

        [Fact]
        public void TestReadTimeSeriesDataAggregated()
        {
            var output = _cloudMonitoring.Run("readAggregate", s_projectId);
            Assert.Equal(0, output.ExitCode);
            Assert.Contains("Now:", output.Stdout);
            Assert.Contains("10 min ago:", output.Stdout);
        }

        [Fact]
        public void TestReadTimeSeriesDataReduced()
        {
            var output = _cloudMonitoring.Run("readReduce", s_projectId);
            Assert.Equal(0, output.ExitCode);
            Assert.Contains("Last 10 min:", output.Stdout);
            Assert.Contains("Last 10-20 min ago:", output.Stdout);
        }

        [Fact]
        public void TestDeleteMetricDescriptor()
        {
            string randomNameSuffix = TestUtil.RandomName();
            string metricType = "custom.googleapis.com/stores/daily_sales" + randomNameSuffix;
            // Create Metric Descriptor.
            var output = _cloudMonitoring.Run("create", s_projectId, metricType);
            // Confirm Metric Descriptor is created.
            Eventually(() =>
            {
                Assert.Equal(0, output.ExitCode);
            });
            // Get Metric Descriptor.
            var outputFromGet = _cloudMonitoring.Run("get", s_projectId, metricType);
            Eventually(() =>
            {
                Assert.Equal(0, output.ExitCode);
            });
            // Delete Metric Descriptor.
            var outputFromDelete = _cloudMonitoring.Run("delete", s_projectId, metricType);
            Assert.Equal(0, outputFromDelete.ExitCode);
        }
    }

    public class QuickStartTests
    {
        readonly CommandLineRunner _quickStart = new CommandLineRunner()
        {
            VoidMain = QuickStart.Main,
            Command = "dotnet run"
        };

        [Fact]
        public void TestRun()
        {
            var output = _quickStart.Run();
            Assert.Equal(0, output.ExitCode);
        }
    }
}
