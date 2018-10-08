// [START monitoring_alert_enable_channel]
using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using System;

partial class AlertSnippets
{
    public NotificationChannel EnableNotificationChannel(
        string channelName = "projects/your-project-id/notificationChannels/123")
    {
        var client = NotificationChannelServiceClient.Create();
        NotificationChannel channel = new NotificationChannel();
        channel.Enabled = true;
        channel.Name = channelName;
        var fieldMask = new FieldMask { Paths = { "enabled" } };
        channel = client.UpdateNotificationChannel(
            updateMask:fieldMask,
            notificationChannel:channel);
        Console.WriteLine("Enabled {0}.", channel.Name);
        return channel;
    }
}
// [END monitoring_alert_enable_channel]
