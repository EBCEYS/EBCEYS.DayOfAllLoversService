using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Speech.Synthesis;
using EBCEYS.DayOfAllLoversService.Extensions;
using EBCEYS.DayOfAllLoversService.Middle;
using EBCEYS.OSServiceHelper;
using EBCEYS.OSServiceHelper.Models;

namespace EBCEYS.DayOfAllLoversService
{
    [SupportedOSPlatform("windows")]
    public class Program
    {
        private const string serviceName = "EBCEYS.DayOfAllLoversService";
        private const string serviceDescription = "EBCEYS.DayOfAllLoversService is a service for the best STAR in the world!" +
            "But you should know that she is a queen of greatest country in the world also!";
        private const string defaultServicePath = @"C:\Program Files\EBCEYS-DayOfAllLoversService";

        private const string helpCommand = "help";
        private const string installCommand = "install";
        private const string startCommand = "start";
        private const string deleteCommand = "delete";
        private const string stopCommand = "stop";
        private const string restartCommand = "restart";
        private const string statusCommand = "status";

        public static string? BasePath => GetBasePath();
        [SupportedOSPlatform("windows")]
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                ProcessArgs(args);
                return;
            }

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            ConfigureConfiguration(builder.Configuration);

            ConfigureServices(builder.Services);

            ConfigureLogging(builder.Logging);

            IHost host = builder.Build();
            host.Run();
        }

        private static void ProcessArgs(string[] args)
        {
            string firstArg = args.First();
            if (firstArg == helpCommand)
            {
                ConsoleWriteHelp();
                return;
            }
            ILogger<WindowsServiceHelper> serviceLogger = CreateServiceHelperLogger();
            using WindowsServiceHelper service = new(serviceLogger, serviceName);

            if (firstArg == installCommand)
            {
                string? path = args.ElementAtOrDefault(1);
                if (string.IsNullOrWhiteSpace(path))
                {
                    if (AskUserYesOrNo("Use default path?"))
                    {
                        Console.WriteLine($"Default path is {defaultServicePath}");
                        path = defaultServicePath;
                    }
                    else
                    {
                        Console.WriteLine("So you need to enter path with command \"install\"...");
                        return;
                    }
                }
                if (!path.IsValidPath())
                {
                    Console.WriteLine($"You entered invalid path {path}! Path should be absolute!");
                    return;
                }

                if (service.IsServiceExists() && service.IsServiceRunning())
                {
                    service.StopService();
                    service.RefreshService();
                }

                List<FileInfo> copiedFiles = [];
                string exeFilePath = CopyToInstallingDir(path, copiedFiles, out bool isUpdate);
                try
                {
                    if (isUpdate)
                    {
                        Console.WriteLine("Selected update mode. Replacing new files is completed! Stoping service...");
                        if (service.IsServiceExists() && service.IsServiceRunning())
                        {
                            service.StopService();
                            service.RefreshService();
                        }
                        Console.WriteLine($"Service status is {service.GetServiceStatus()}");
                    }
                    else
                    {
                        service.InstallService(exeFilePath, InstallServiceStartMode.Auto);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to install service!");
                    copiedFiles.ForEach(file =>
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error on deleting file {file.FullName} from working directory {path}!");
                            Console.WriteLine(ex.ToString());
                        }
                    });
                    throw;
                }
                service.SetDescriptionForService(serviceDescription);
                service.StartService();

                service.RefreshService();

                Console.WriteLine($"Service status is {service.GetServiceStatus()}");
                string processDesc = isUpdate ? "updated" : "installed";
                Console.WriteLine($"Service {serviceName} was {processDesc} sucessully!");
                return;
            }
            if (firstArg == startCommand)
            {
                service.StartService();
                service.RefreshService();
                Console.WriteLine($"Current service status is {service.GetServiceStatus()}");
                return;
            }
            if (firstArg == deleteCommand)
            {
                if (service.DeleteService(TimeSpan.FromSeconds(15.0)))
                {
                    Console.WriteLine("Service was removed sucessfully!");
                }
                else
                {
                    Console.WriteLine("Error on deleting service! Please do it manualy!");
                }
                return;
            }
            if (firstArg == stopCommand)
            {
                service.StopService();
                service.RefreshService();
                Console.WriteLine($"Current service status is {service.GetServiceStatus()}");
                return;
            }
            if (firstArg == restartCommand)
            {
                Console.WriteLine("Try to restart service...");
                service.StopService();
                service.Service!.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
                Console.WriteLine("Service stoped...");
                service.RefreshService();
                service.StartService();
                service.Service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, TimeSpan.FromSeconds(15.0));
                Console.WriteLine("Service running...");
                return;
            }
            if (firstArg == statusCommand)
            {
                System.ServiceProcess.ServiceControllerStatus? status = service.GetServiceStatus();
                Console.WriteLine($"Service status is {status}");
                return;
            }

            Console.WriteLine($"Unknown command! Enter \"{helpCommand}\" key.");
            return;
        }

        private static string CopyToInstallingDir(string path, List<FileInfo> copiedFiles, out bool isUpdate)
        {
            isUpdate = false;
            DirectoryInfo installDirInfo = new(path);
            Console.WriteLine($"Use {path} to install service");
            FileInfo[] existingFiles = installDirInfo.GetFiles();
            if (installDirInfo.Exists && existingFiles.Length != 0)
            {
                if (AskUserYesOrNo("Selected directory is not empty. Do you want to clear it?"))
                {
                    isUpdate = existingFiles.FirstOrDefault(f => f.Name.StartsWith("EBCEYS") && f.Extension == ".exe") != null;
                    foreach (FileInfo existingFile in existingFiles)
                    {
                        Console.WriteLine($"Try to delete file {existingFile.FullName}");
                        existingFile.Delete();
                    }
                }
                else
                {
                    return "";
                }
            }
            installDirInfo.Create();
            string currentDirectoryPath = BasePath ?? Directory.GetCurrentDirectory();
            Console.WriteLine($"Current directory path is {currentDirectoryPath}");
            DirectoryInfo currentDirInfo = new(currentDirectoryPath);
            string exeFilePath = string.Empty;
            copiedFiles ??= [];
            foreach (FileInfo file in currentDirInfo.GetFiles())
            {
                if (file.Extension.Contains(".zip"))
                {
                    continue;
                }
                string newPath = Path.Combine(installDirInfo.FullName, file.Name);
                if (file.Extension.Contains(".exe"))
                {
                    exeFilePath = newPath;
                }
                Console.WriteLine($"Copy file {Environment.NewLine}{file.FullName}{Environment.NewLine}to{Environment.NewLine}{newPath}");
                file.CopyTo(newPath, true);
                copiedFiles.Add(file);
            }
            return exeFilePath;
        }

        private static void ConsoleWriteHelp()
        {
            Console.WriteLine(serviceDescription);
            Console.WriteLine("Supported commands:");
            Console.WriteLine($" * {helpCommand} - it's just a help command. Displays a help. You just used it...");
            Console.WriteLine($" * {installCommand} - it's service install or update command. After \"install\" key should be absulute path [optional]. Example:{Environment.NewLine} EBCEYS.DayOfAllLoversService.exe install{Environment.NewLine}or{Environment.NewLine} EBCEYS.DayOfAllLoversService.exe install \"{defaultServicePath}\"");
            Console.WriteLine($" * {deleteCommand} - uninstalls the service.");
            Console.WriteLine($" * {startCommand} - starts the service.");
            Console.WriteLine($" * {stopCommand} - stops the service.");
            Console.WriteLine($" * {restartCommand} - restarts the service.");
            Console.WriteLine($" * {statusCommand} - shows service status");
        }
        /// <summary>
        /// Asks user to enter agreement. " (y/n)" adds to question.
        /// </summary>
        /// <param name="question">The question. " (y/n)" adds to question.</param>
        /// <returns><c>true</c> if user answered "yes"<br/><c>false</c> if answer was "no" or <see cref="string.Empty"/></returns>
        private static bool AskUserYesOrNo(string question)
        {
            Console.WriteLine(question + " (y/n)");
            for (; ; )
            {
                string command = (Console.ReadLine() ?? "").ToLowerInvariant();
                if (command == "y" || command == "yes")
                {
                    return true;
                }
                if (command == string.Empty || command == "n" || command == "no")
                {
                    return false;
                }
                Console.WriteLine("Please enter correct answer (\"yes\" or \"no\") or just press \"enter\":");
            }
        }

        private static ILogger<WindowsServiceHelper> CreateServiceHelperLogger()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(conf =>
            {
                conf.ClearProviders();
                conf.SetMinimumLevel(LogLevel.Trace);
                conf.AddConsole();
            });
            return loggerFactory.CreateLogger<WindowsServiceHelper>();
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
            services.AddSingleton<TextSpeekerService>(sp =>
            {
                string voiceToSelect = sp.GetService<IConfiguration>()?.GetValue<string?>("VoiceName") ?? "ru";
                using SpeechSynthesizer ss = new();
                List<InstalledVoice> voices = ss.GetInstalledVoices().Where(v => v.VoiceInfo.Culture.Name.Contains(voiceToSelect)).ToList();
                return new(sp.GetService<ILogger<TextSpeekerService>>()!, voices.GetRandomElement()!);
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