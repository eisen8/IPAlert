namespace IPAlert
{
    /// <summary>
    ///  The boilerplate entry point for the application.
    /// </summary>
    internal static class Program
    {
        private static IPAlert? _IPAlert;

        /// <summary>
        ///  The boilerplate Main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Logger logger = Logger.Instance;
            logger.Init();
            try
            {
                logger.Info("Starting IPAlert Application");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                _IPAlert = new IPAlert();
                Application.ApplicationExit += (sender, e) =>
                {
                    if (_IPAlert != null)
                    {
                        _IPAlert.Dispose();
                        _IPAlert = null;
                    }
                };

                Application.Run();
            }
            catch(Exception e)
            {
                logger.Error("Error starting IPAlert Application", e);
            }
        }
    }
}