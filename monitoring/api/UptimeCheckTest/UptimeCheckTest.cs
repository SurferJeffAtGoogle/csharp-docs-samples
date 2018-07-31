using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GoogleCloudSamples
{
    public class UptimeCheckTest : IClassFixture<UptimeCheckTestFixture>
    {
        private readonly UptimeCheckTestFixture _fixture;

        public UptimeCheckTest(UptimeCheckTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestList()
        {
            var output = _fixture.Cmd.Run("list", "-p", _fixture.ProjectId);
            // Confirm it contains the two configs we just created.
            foreach (string configName in _fixture.UptimeCheckConfigNames)
            {
                Assert.Contains(configName, output.Stdout);
            }
        }

        [Fact]
        public void TestListIps()
        {
            var output = _fixture.Cmd.Run("list-ips");
            Assert.Contains("Oregon", output.Stdout);
        }
 
        [Fact]
        public void TestUpdateAndGet()
        {
            _fixture.Cmd.Run("update", _fixture.UptimeCheckConfigNames.First(), "-d", "whippletree");
            _fixture.Cmd.Run("update", _fixture.UptimeCheckConfigNames.First(), "-h", "http://tachistoscope.com");
            var output = _fixture.Cmd.Run("get", _fixture.UptimeCheckConfigNames.First());
            Assert.Contains("whippletree", output.Stdout);
            Assert.Contains("tachistoscope.com", output.Stdout);
        } 
    }

    public class UptimeCheckTestFixture : IDisposable
    {
        public UptimeCheckTestFixture()
        {
            // Create two uptime checks to work with.
            var output = Cmd.Run("create", "-p", ProjectId);
            UptimeCheckConfigNames.Add(output.Stdout.Trim());
            output = Cmd.Run("create", "-p", ProjectId);
            UptimeCheckConfigNames.Add(output.Stdout.Trim());
        }

        public IList<string> UptimeCheckConfigNames { get; private set; } =
            new List<string>();

        public CommandLineRunner Cmd {get; private set;} = new CommandLineRunner()
        {
            Main = UptimeCheck.Main,
            Command = "UptimeCheck"
        };

        public string ProjectId { get; private set; } = 
            Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");

        public void Dispose()
        {
            // Clean up the uptime checks we created:
            foreach (string configName in UptimeCheckConfigNames)
            {
                Cmd.Run("delete", configName);
            }
        }
    }
}
