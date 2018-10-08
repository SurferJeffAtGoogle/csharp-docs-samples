using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using System;

partial class AlertSnippets
{
    public void EnableNotificationChannel(
        string policyName = "projects/your-project-id/alertPolicies/123242")
    {
        var client = AlertPolicyServiceClient.Create();
        AlertPolicy policy = new AlertPolicy();
        policy.Enabled = true;
        policy.Name = policyName;
        var fieldMask = new FieldMask { Paths = { "enabled" } };
        client.UpdateAlertPolicy(fieldMask, policy);
        Console.WriteLine("Enabled {0}.", policy.Name);
    }
}