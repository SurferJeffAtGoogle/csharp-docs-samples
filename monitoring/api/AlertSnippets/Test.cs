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
        _snippets.EnableNotificationChannel(_fixture.Alert.Name);
    }
}
