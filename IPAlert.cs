using System.Net.NetworkInformation;
using System.Timers;
using IPAlert.Resources;
using IPAlert.Services;
using IPAlert.Settings;
using IPAlert.Utils;
using Timer = System.Timers.Timer;


namespace IPAlert
{
    /// <summary>
    /// The IP Alert Tray Icon and Notification
    /// </summary>
    public class IPAlert : IDisposable
    {
        private AppSettings _settings;
        private IPRetriever _ipRetriever;
        private Logger _logger;


        private NotifyIcon _trayIcon;
        private string _lastPublicIp = IPRetriever.NO_CONNECTION_STRING;
        private readonly NetworkAddressChangedEventHandler _networkChangedHandler;
        private readonly Timer _timer;

        private bool _isUpdating = false;
        private object _lock = new object();


        public IPAlert(AppSettings settings, Logger logger, IPRetriever ipRetriever)
        {
            _settings = settings;
            _logger = logger;
            _ipRetriever = ipRetriever;

            _trayIcon = new NotifyIcon
            {
                Icon = new Icon(Constants.ICON_PATH),
                Text = Resource.CheckingIP,
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };

            _trayIcon.ContextMenuStrip.Items.Add(Resource.CopyIP, null, (s, e) => copyPublicIPToClipboard());
            _trayIcon.ContextMenuStrip.Items.Add(Resource.Exit, null, (s, e) => Application.Exit());

            if (_settings.Mode == IPAlertMode.OnNetworkChanges)
            {
                _networkChangedHandler = async (sender, e) =>
                {
                    onNetworkChanged(sender, e);
                };

                NetworkChange.NetworkAddressChanged += _networkChangedHandler;
            }
            else // Timed mode
            {
                _timer = new Timer(_settings.PollingTimeMs);
                _timer.Elapsed += onPollingTimer;
                _timer.AutoReset = true;
                _timer.Start();
            }
                // Initial check
                UpdateIPAddress(false);
        }

        public void Dispose()
        {
            if (_networkChangedHandler != null)
            {
                NetworkChange.NetworkAddressChanged -= _networkChangedHandler;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
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
            if (_lastPublicIp != IPRetriever.NO_CONNECTION_STRING)
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
            UpdateIPAddress(_settings.NotificationsEnabled);
        }

        /// <summary>
        /// Event that occurs on a polling timer
        /// </summary>
        /// <param name="sender">sender arg</param>
        /// <param name="e">Event args</param>
        private async void onPollingTimer(object? sender, ElapsedEventArgs e)
        {
            _logger.Info("onPollingTimer event");
            UpdateIPAddress(_settings.NotificationsEnabled);
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
                if (_settings.Mode == IPAlertMode.OnNetworkChanges)
                    Thread.Sleep(_settings.PollingTimeMs); // Wait before trying so that the network can settle down a bit


                string publicIp = await _ipRetriever.GetPublicIPAddress();

                if (publicIp != _lastPublicIp)
                {
                    _lastPublicIp = publicIp;

                    // Update the tray text
                    string trayText = string.Format(Resource.IPDisplay, _lastPublicIp);
                    _trayIcon.Text = trayText;

                    // Trigger the notification
                    if (shouldNotify)
                    {
                        if (_lastPublicIp == IPRetriever.NO_CONNECTION_STRING)
                        {
                            _trayIcon.BalloonTipTitle = Resource.ConnectionLost;
                        } else
                        {
                            _trayIcon.BalloonTipTitle = Resource.IPAddressChanged;
                            _trayIcon.BalloonTipText = trayText;
                        }

                        _trayIcon.ShowBalloonTip(_settings.NotificationTimeMs);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.Error("Exception on UpdateIPAddress", e);
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}
