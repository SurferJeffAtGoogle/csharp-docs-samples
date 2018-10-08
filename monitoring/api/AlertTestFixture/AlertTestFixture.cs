﻿using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using System;
using static Google.Cloud.Monitoring.V3.Aggregation.Types;
using static Google.Cloud.Monitoring.V3.AlertPolicy.Types.Condition.Types;
using static Google.Cloud.Monitoring.V3.AlertPolicy.Types;

/// <summary>
/// Creates an AlertPolicy and NotificationChannel for the duration
/// if the tests.
/// </summary>
public class AlertTestFixture : IDisposable
{
    public AlertTestFixture()
    {
        var channel = new NotificationChannel()
        {
            Type = "email",
            DisplayName = "Email joe.",
            Description = "AlertTest.cs",
            Labels = { { "email_address", "joe@example.com" } },
            UserLabels =
                {
                    { "role", "operations" },
                    { "level", "5" },
                    { "office", "california_westcoast_usa" },
                    { "division", "fulfillment"}
                }
        };
        Channel = NotificationChannelClient.CreateNotificationChannel(
            new ProjectName(ProjectId), channel);

        Alert = AlertPolicyClient.CreateAlertPolicy(
            new ProjectName(ProjectId), new AlertPolicy()
            {
                DisplayName = "AlertTest.cs",
                Enabled = false,
                Combiner = ConditionCombinerType.Or,
                Conditions =
            {
                    new AlertPolicy.Types.Condition()
                    {
                        ConditionThreshold = new MetricThreshold()
                        {
                            Filter = "metric.label.state=\"blocked\" AND metric.type=\"agent.googleapis.com/processes/count_by_state\"  AND resource.type=\"gce_instance\"",
                            Aggregations = {
                                new Aggregation() {
                                    AlignmentPeriod = Duration.FromTimeSpan(
                                        TimeSpan.FromSeconds(60)),
                                    PerSeriesAligner = Aligner.AlignMean,
                                    CrossSeriesReducer = Reducer.ReduceMean,
                                    GroupByFields = {
                                        "project",
                                        "resource.label.instance_id",
                                        "resource.label.zone"
                                    }
                                }
                            },
                            DenominatorFilter = "",
                            DenominatorAggregations = {},
                            Comparison = ComparisonType.ComparisonGt,
                            ThresholdValue = 100.0,
                            Duration = Duration.FromTimeSpan(
                                TimeSpan.FromSeconds(900)),
                            Trigger = new Trigger() {
                                Count = 1,
                                Percent = 0.0,
                            }
                        },
                        DisplayName = "AlertTest.cs",
                    }
            },
            });
    }

    public NotificationChannel Channel { get; private set; }
    public AlertPolicy Alert { get; private set; }
    public string ProjectId { get; private set; } =
        Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");

    public NotificationChannelServiceClient NotificationChannelClient
    { get; private set; } = NotificationChannelServiceClient.Create();
    public AlertPolicyServiceClient AlertPolicyClient
    { get; private set; } = AlertPolicyServiceClient.Create();


    public void Dispose()
    {
        NotificationChannelClient.DeleteNotificationChannel(
            NotificationChannelName.Parse(Channel.Name), true);
        AlertPolicyClient.DeleteAlertPolicy(
            AlertPolicyName.Parse(Alert.Name));
    }
}
