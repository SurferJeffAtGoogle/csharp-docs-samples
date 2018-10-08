using System;
using Xunit;

public class AlertSnippetsTest : IClassFixture<AlertTestFixture>
{
    private readonly AlertTestFixture _fixture;
    private readonly AlertSnippets _snippets;

    public AlertSnippetsTest(AlertTestFixture fixture)
    {
        _fixture = fixture;
        _snippets = new AlertSnippets();
    }

    [Fact]
    public void TestEnableNotificationChannel()
    {
        var channel = _snippets.EnableNotificationChannel(
            _fixture.Channel.Name);
        Assert.True(channel.Enabled);
    }

    [Fact]
    public void TestDeleteNotificationChannel()
    {
        var channel = _fixture.CreateChannel();
        _snippets.DeleteNotificationChannel(channel.Name);
    }

}
