using System.Diagnostics;
using System.Runtime.Versioning;
using EBCEYS.DayOfAllLoversService.Extensions;
using EBCEYS.DayOfAllLoversService.Middle;
using EBCEYS.WindowsServiceHelper;

namespace EBCEYS.DayOfAllLoversService
{
    [SupportedOSPlatform("windows")]
    public class Program
    {
        private static readonly string serviceName = "EBCEYS.DayOfAllLoversService";
        private static readonly string serviceDescription = "EBCEYS.DayOfAllLoversService is a service for the best STAR in the world!" +
            "But you should know that she is a queen of greatest country in the world also!";
        private static readonly string defaultServicePath = @"C:\Program Files\EBCEYS-DayOfAllLoversService";
        public static string? BasePath => GetBasePath();
        [SupportedOSPlatform("windows")]
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "help")
                {
                    Console.WriteLine(serviceDescription);
                    Console.WriteLine("Supported commands:");
                    Console.WriteLine(" * help - it's just a help command. Displays a help.");
                    Console.WriteLine(" * install - it's service install command. After it should be path to install [optional]. Example: EBCEYS.DayOfAllLoversService.exe install");
                    Console.WriteLine(" * start - starts the service.");
                    Console.WriteLine(" * delete - uninstalls the service.");
                    Console.WriteLine(" * stop - stops the service.");
                    Console.WriteLine(" * status - shows service status");
                    return;
                }
                WindowsServiceHelperClient service = new(serviceName);
                if (args[0] == "install")
                {
                    string? path = args.ElementAtOrDefault(1);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        Console.WriteLine("Use default path? (y/n)");
                        for (; ; )
                        {
                            string command = (Console.ReadLine() ?? "").ToLowerInvariant();
                            if (command == "y" || command == "yes")
                            {
                                Console.WriteLine($"Default path is {defaultServicePath}");
                                path = defaultServicePath;
                                break;
                            }
                            if (command == string.Empty || command == "n" || command == "no")
                            {
                                Console.WriteLine("So you need to enter path with command \"install\"...");
                                return;
                            }
                        }
                    }
                    if (!path.IsValidPath())
                    {
                        Console.WriteLine($"You entered invalid path {path}! Path should be absolute!");
                        return;
                    }
                    DirectoryInfo installDirInfo = new(path);
                    Console.WriteLine($"Use {path} to install service");
                    installDirInfo.Create();
                    string currentDirectoryPath = Directory.GetCurrentDirectory();
                    Console.WriteLine($"Current directory path is {currentDirectoryPath}");
                    DirectoryInfo currentDirInfo = new(currentDirectoryPath);
                    string exeFilePath = string.Empty;
                    currentDirInfo.GetFiles().ToList().ForEach(file =>
                    {
                        string newPath = Path.Combine(installDirInfo.FullName, file.Name);
                        if (file.Extension.Contains(".exe"))
                        {
                            exeFilePath = newPath;
                        }
                        Console.WriteLine($"Copy file {file.FullName} to {newPath}");
                        file.CopyTo(newPath, true);
                    });
                    service.InstallService(serviceDescription, exeFilePath);
                    service.StartService();
                    Console.WriteLine($"Service {serviceName} was installed sucessully!");
                    return;
                }
                if (args[0] == "start")
                {
                    service.StartService();
                    Console.WriteLine($"Current service status is {service.GetServiceStatus()}");
                    return;
                }
                if (args[0] == "delete")
                {
                    if (service.DeleteService())
                    {
                        Console.WriteLine("Service deleted sucessfully!");
                    }
                    else
                    {
                        Console.WriteLine("Error on deleting service! Please do it manualy!");
                    }
                    return;
                }
                if (args[0] == "stop")
                {
                    service.StopService();
                    Console.WriteLine($"Current service status is {service.GetServiceStatus()}");
                    return;
                }
                if (args[0] == "status")
                {
                    System.ServiceProcess.ServiceControllerStatus? status = service.GetServiceStatus();
                    Console.WriteLine($"Service status is {status}");
                    return;
                }
                Console.WriteLine("Unknown command! Enter \"help\" key.");
                return;
            }

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            ConfigureConfiguration(builder.Configuration);

            ConfigureServices(builder.Services);

            ConfigureLogging(builder.Logging);

            IHost host = builder.Build();
            host.Run();
        }
        
        private static IConfigurationBuilder ConfigureConfiguration(IConfigurationBuilder config)
        {
            config.SetBasePath(BasePath ?? Directory.GetCurrentDirectory());
            config.AddEnvironmentVariables();
            config.AddJsonFile("appsettings.json", false, true);
            return config;
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddWindowsService(opt =>
            {
                opt.ServiceName = serviceName;
            });
            return services.AddHostedService<Worker>();
        }

        private static ILoggingBuilder ConfigureLogging(ILoggingBuilder logging)
        {
            return logging;
        }

        private static string? GetBasePath()
        {
            using ProcessModule? mainModule = Process.GetCurrentProcess()?.MainModule;
            return Path.GetDirectoryName(mainModule?.FileName);
        }
    }
}