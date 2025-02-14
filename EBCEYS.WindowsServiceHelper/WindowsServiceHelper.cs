using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using System.ServiceProcess;

namespace EBCEYS.WindowsServiceHelper
{
    [SupportedOSPlatform("windows")]
    public class WindowsServiceHelperClient : IDisposable
    {
        public string ServiceName { get; }
        private ServiceController? service;
        private ServiceController? Service
        {
            get
            {
                return (service ?? GetService());
            }
        }
        public WindowsServiceHelperClient(string serviceName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(serviceName, nameof(serviceName));
            ServiceName = serviceName;
        }
        private ServiceController? GetService()
        {
            service = ServiceController.GetServices()?.FirstOrDefault(x => x.ServiceName == ServiceName);
            return service;
        }
        public bool IsServiceExists()
        {
            return Service != default;
        }
        public ServiceControllerStatus? GetServiceStatus()
        {
            return Service?.Status;
        }
        public bool IsServiceRunning()
        {
            return GetServiceStatus() == ServiceControllerStatus.Running;
        }
        public bool IsServiceStoped()
        {
            return GetServiceStatus() == ServiceControllerStatus.Stopped;
        }
        public void StartService(string[]? args = default, WaitForStatusInfo waitFor = default)
        {
            if (!IsServiceExists())
            {
                throw new InvalidOperationException("Service is not installed!");
            }
            if (args != default)
            {
                Service?.Start(args);
            }
            else
            {
                Service?.Start();
            }
            Service?.WaitForStatus(ServiceControllerStatus.Running, waitFor);
        }
        public void StopService(bool stopDependetServices = false, WaitForStatusInfo waitFor = default)
        {
            if (!IsServiceExists())
            {
                throw new InvalidOperationException("Service is not installed!");
            }
            Service?.Stop(stopDependetServices);
            Service?.WaitForStatus(ServiceControllerStatus.Stopped, waitFor);
            Console.WriteLine("Service stoped...");
        }
        public void PauseService(WaitForStatusInfo waitFor = default)
        {
            if (!IsServiceExists())
            {
                throw new InvalidOperationException("Service is not installed!");
            }
            Service?.Pause();
            Service?.WaitForStatus(ServiceControllerStatus.Paused, waitFor);
        }
        public bool DeleteService()
        {
            if (!IsServiceExists())
            {
                throw new InvalidOperationException("Service is not installed!");
            }
            if (!IsServiceStoped())
            {
                throw new InvalidOperationException("Service should be stoped");
            }
            ProcessStartInfo installInfo = new()
            {
                FileName = "sc.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                Arguments = $"delete {ServiceName}"
            };
            using Process deleteProcess = new()
            {
                StartInfo = installInfo,
            };
            Console.WriteLine($"Try to execute service uninstall process {installInfo.FileName} {installInfo.Arguments}");
            deleteProcess.Start();
            bool res = deleteProcess.WaitForExit(TimeSpan.FromSeconds(10.0));

            string output = deleteProcess.StandardOutput.ReadToEnd();
            Console.WriteLine($"Process result output: {output}");
            Console.WriteLine("Please remove files from service working directory");
            return res;

        }
        public void InstallService(string description, string path)
        {
            if (IsServiceExists())
            {
                throw new InvalidOperationException($"Service is already installed! Current service status is {GetServiceStatus()}");
            }
            ProcessStartInfo installInfo = new()
            {
                FileName = "sc.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            };
            List<string> argsList = [];
            argsList.Add($"create {ServiceName}");
            argsList.Add($"binPath= \"{path}\"");
            argsList.Add($"DisplayName=\"{ServiceName}\"");
            argsList.Add($"start= auto");
            string installArgs = string.Join(" ", argsList);
            installInfo.Arguments = installArgs;
            Console.WriteLine($"Start installing process: {installInfo.FileName} {installArgs}");
            using Process installProcess = new()
            {
                StartInfo = installInfo
            };
            installProcess.Start();
            Console.WriteLine("Start installation process...");
            installProcess.WaitForExit(TimeSpan.FromSeconds(10.0));

            string output = installProcess.StandardOutput.ReadToEnd();
            Console.WriteLine($"Installation result:");
            Console.WriteLine(output);

            try
            {
                Console.WriteLine("Try to set description for service");
                installInfo.Arguments = $"description {ServiceName} \"{description}\"";
                using Process setDescProcess = new()
                {
                    StartInfo = installInfo
                };
                setDescProcess.Start();
                setDescProcess.WaitForExit(TimeSpan.FromSeconds(10.0));

                output = setDescProcess.StandardOutput.ReadToEnd();
                Console.WriteLine("Set description output:");
                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on creating description for service!");
                Console.WriteLine(ex.ToString());
            }
        }

        public void Dispose()
        {
            service?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
