using System.Runtime.Versioning;
using System.ServiceProcess;

namespace EBCEYS.WindowsServiceHelper
{
    [SupportedOSPlatform("windows")]
    public static class ServiceControllerExtensions
    {
        public static void WaitForStatus(this ServiceController sc, ServiceControllerStatus status, WaitForStatusInfo waitForStatusInfo = default)
        {
            if (waitForStatusInfo.shouldWaitForStatus)
            {
                if (!waitForStatusInfo.waitTime.HasValue)
                {
                    sc.WaitForStatus(status);
                }
                sc.WaitForStatus(status, waitForStatusInfo.waitTime!.Value);
                return;
            }
        }
    }
}
