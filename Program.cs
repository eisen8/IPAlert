using Autofac;
using IPAlert.Settings;
using System.Globalization;

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
                AppSettings settings = AppSettings.LoadFromFile(Constants.SETTINGS_FILE_PATH);

                // Localization
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                // IOC
                dependencyConfiguration(logger, settings);

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

        private static void dependencyConfiguration(Logger logger, AppSettings settings)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<Logger>(logger).As<Logger>().SingleInstance();
            builder.RegisterInstance<AppSettings>(settings).As<AppSettings>().SingleInstance();
            builder.RegisterType<IPRetriever>().As<IPRetriever>().SingleInstance();
            builder.RegisterType<IPAlert>().As<IPAlert>().SingleInstance();

            _container = builder.Build();
        }
    }
}