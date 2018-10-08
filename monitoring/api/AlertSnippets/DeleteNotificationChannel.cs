// [START monitoring_alert_delete_channel]
using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using System;

partial class AlertSnippets
{
    public void DeleteNotificationChannel(
        string channelName = "projects/your-project-id/notificationChannels/123")
    {
        var client = NotificationChannelServiceClient.Create();
        client.DeleteNotificationChannel(
            name:NotificationChannelName.Parse(channelName),
            force:true);
        Console.WriteLine("Deleted {0}.", channelName);
    }
}
// [END monitoring_alert_delete_channel]
