﻿using System.Net.NetworkInformation;
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
        private readonly AppSettings _settings;
        private readonly IPRetriever _ipRetriever;
        private readonly Logger _logger;


        private readonly NotifyIcon _trayIcon;
        private readonly NetworkAddressChangedEventHandler? _networkChangedHandler;
        private readonly Timer? _timer;
        private readonly Lock _lock = new Lock();

        private string _lastPublicIp = IPRetriever.NO_CONNECTION_STRING;
        private bool _isUpdating = false;


        public IPAlert(AppSettings settings, Logger logger, IPRetriever ipRetriever)
        {
            _settings = settings;
            _logger = logger;
            _ipRetriever = ipRetriever;

            _trayIcon = new NotifyIcon
            {
                Icon = new Icon(Constants.ICON_PATH),
                Text = Resource.NoConnection,
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };

            _trayIcon.ContextMenuStrip.Items.Add(Resource.CopyIP, null, (s, e) => copyPublicIPToClipboard());
            _trayIcon.ContextMenuStrip.Items.Add(Resource.Exit, null, (s, e) => Application.Exit());

            if (_settings.Mode == IPAlertMode.OnNetworkChanges)
            {
                _networkChangedHandler = (sender, e) =>
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
                updateIPAddress(false);
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
        private void onNetworkChanged(object? sender, EventArgs e)
        {
            _logger.Info("onNetworkChanged event");
            updateIPAddress(_settings.NotificationsEnabled);
        }

        /// <summary>
        /// Event that occurs on a polling timer
        /// </summary>
        /// <param name="sender">sender arg</param>
        /// <param name="e">Event args</param>
        private void onPollingTimer(object? sender, ElapsedEventArgs e)
        {
            _logger.Info("onPollingTimer event");
            updateIPAddress(_settings.NotificationsEnabled);
        }

        /// <summary>
        /// Updates the IP Address
        /// </summary>
        /// <param name="shouldNotify">Whether we should trigger a notification (balloon tip) on changes</param>
        private async void updateIPAddress(bool shouldNotify = true)
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
                    string trayText = string.Format(Resource.IPDisplay, _lastPublicIp);

                    // Update the tray text
                    if (_lastPublicIp == IPRetriever.NO_CONNECTION_STRING)
                    {
                        _trayIcon.Text = Resource.NoConnection;
                    } 
                    else
                    {
                        _trayIcon.Text = trayText;
                    }

                    // Trigger the notification
                    if (shouldNotify)
                    {
                        if (_lastPublicIp == IPRetriever.NO_CONNECTION_STRING)
                        {

                            _trayIcon.BalloonTipTitle = Resource.ConnectionLost;
                            _trayIcon.BalloonTipText = " ";
                        }
                        else
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


        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose of manages resources here
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


            // Free any unmanaged resources here

            _disposed = true;
        }

        ~IPAlert()
        {
            Dispose(false);
        }
    }
}
