using System.Net.NetworkInformation;


namespace IPAlert
{
    /// <summary>
    /// The IP Alert Tray Icon and Notification
    /// </summary>
    public class IPAlert : IDisposable
    {
        private NotifyIcon _trayIcon;
        private string _lastPublicIp = "";
        private NetworkAddressChangedEventHandler _networkChangedHandler;
        private IPRetriever _IPRetriever;
        private Logger _logger;
        private bool _isUpdating = false;
        private object _lock = new object();


        public IPAlert()
        {
            _logger = Logger.Instance;
            _IPRetriever = new IPRetriever();

            _trayIcon = new NotifyIcon
            {
                Icon = new Icon("./resources/IPAlert_Icon.ico"),
                Text = "Checking IP...",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };

            _trayIcon.ContextMenuStrip.Items.Add("Copy IP", null, (s, e) => copyPublicIPToClipboard());
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => Application.Exit());

            _networkChangedHandler = async (sender, e) =>
            {
                onNetworkChanged(sender, e);
            };


            NetworkChange.NetworkAddressChanged += _networkChangedHandler;

            // Initial check
            UpdateIPAddress(false);
        }

        public void Dispose()
        {
            if (_networkChangedHandler != null)
            {
                NetworkChange.NetworkAddressChanged -= _networkChangedHandler;
            }

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
        }

        /// <summary>
        /// Copies the current Public IP to the clipboard
        /// </summary>
        private void copyPublicIPToClipboard()
        {
            if (_lastPublicIp != "Error" && _lastPublicIp != "")
            {
                Clipboard.SetText(_lastPublicIp);
            }
        }

        /// <summary>
        /// Event that occurs on network events
        /// </summary>
        /// <param name="sender">sender arg</param>
        /// <param name="e">Event args</param>
        private async void onNetworkChanged(object? sender, EventArgs e)
        {
            _logger.Info("onNetworkChanged event");
            UpdateIPAddress(true);
        }

        /// <summary>
        /// Updates the IP Address
        /// </summary>
        /// <param name="shouldNotify">Whether we should trigger a notification (balloon tip) on changes</param>
        private async void UpdateIPAddress(bool shouldNotify = true)
        {
            // This lock/isUpdating is to ensure only one check happens if multiple network events occur at once
            lock(_lock)
            {
                if(_isUpdating)
                {
                    return;
                }

                _isUpdating = true;
            }
            try
            {
                Thread.Sleep(2000); // Wait before trying so that the network can settle down a bit
                string publicIp = await _IPRetriever.GetPublicIPAddress();

                if (publicIp != _lastPublicIp)
                {
                    _lastPublicIp = publicIp;

                    // Update the tray text
                    string trayText = $"IP: {publicIp}";
                    _trayIcon.Text = trayText;

                    // Trigger the notification
                    if (shouldNotify)
                    {
                        _trayIcon.BalloonTipTitle = "IP Address Updated";
                        _trayIcon.BalloonTipText = trayText;
                        _trayIcon.ShowBalloonTip(5000);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.Error("Error on UpdateIPAddress", e);
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}
