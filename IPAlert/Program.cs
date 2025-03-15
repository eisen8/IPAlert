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
    }
}