using Autofac;

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
                // IOC
                dependencyConfiguration(logger);

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
                logger.Error("Error starting IPAlert Application", e);
            }
        }

        private static void dependencyConfiguration(Logger logger)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<Logger>(logger).As<Logger>().SingleInstance();
            builder.RegisterType<IPRetriever>().As<IPRetriever>().SingleInstance();
            builder.RegisterType<IPAlert>().As<IPAlert>().SingleInstance();

            _container = builder.Build();
        }
    }
}