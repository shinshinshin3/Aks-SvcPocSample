using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace CommonLibrary.Logging
{
    public class PodTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null) { return; }

            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                string podName = Environment.GetEnvironmentVariable("HOSTNAME");
                telemetry.Context.Cloud.RoleName = podName.Substring(1, podName.Length - 17);
            }
        }
    }
}
