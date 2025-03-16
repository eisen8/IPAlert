using Autofac;
using IPAlert.Services;
using IPAlert.Settings;
using IPAlert.Utils;
using System.Globalization;
using System.IO.Abstractions;

namespace IPAlert
{
    /// <summary>
    ///  The boilerplate entry point for the application.
    /// </summary>
    internal static class Program
    {
        private static IPAlert? _ipAlert;
        private static IContainer? _container;
        private static Mutex? _mutex; // Used to enforce single instance of app running

        /// <summary>
        ///  The boilerplate Main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Logger logger = new Logger();
            logger.Info("Starting IPAlert Application");
            try
            {
                // Enforce single instance
                if (!checkIsNewInstance(logger))
                {
                    logger.Info("Another instance of IPAlert is already running, exiting.");
                    return;
                }

                // Localization
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                // IOC
                _container = dependencyConfiguration(logger);

                // Application 
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                _ipAlert = _container.Resolve<IPAlert>();
                Application.ApplicationExit += (sender, e) =>
                {
                    if (_ipAlert != null)
                    {
                        _ipAlert.Dispose();
                        _ipAlert = null;
                    }
                };

                Application.Run();
            }
            catch(Exception e)
            {
                logger.Error("Exception starting IPAlert Application", e);
            }
        }

        /// <summary>
        /// Configures the dependency injection container
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <returns>The DI container</returns>
        private static IContainer dependencyConfiguration(Logger logger)
        {
            FileSystem fs = new FileSystem();
            AppSettings settings = AppSettings.LoadFromFile(Constants.SETTINGS_FILE_PATH, fs);

            var builder = new ContainerBuilder();
            builder.RegisterInstance<Logger>(logger).AsSelf().SingleInstance();
            builder.RegisterInstance<IFileSystem>(fs).As<IFileSystem>().SingleInstance();
            builder.RegisterInstance<AppSettings>(settings).AsSelf().SingleInstance();
            builder.RegisterType<IPRetriever>().AsSelf().SingleInstance();
            builder.RegisterType<IPAlert>().AsSelf().SingleInstance();

            return builder.Build();
        }


        /// <summary>
        /// Checks if the system is already running an instance of the application.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <returns>Returns true if this is a new instance. False if there is another instance running.</returns>
        private static bool checkIsNewInstance(Logger logger)
        {
            string mutexName = "Global\\IPAlertMutex"; // Global mutexs are system-wide, so it is held across applications
            bool isNewInstance;
            try
            {
                _mutex = new Mutex(true, mutexName, out isNewInstance);
                return isNewInstance;

            }
            catch (AbandonedMutexException) 
            {
                // This occurs when previous application was forcibly terminated and mutex was abandoned
                logger.Info("Abandoned mutex found, creating new instance");
                // create a new mutex
                _mutex = new Mutex(true, mutexName);
                return true;
            }
        }
    }
}