# IPAlert

A lightweight Windows system tray application that displays your **Public IP address** in the notification area. It will trigger application notifications if your IP Address changes or if you lose connection.

## Features
- **Shows Public IP Address** in the system tray.
- **Real-time Notifications** when your connection changes
- **Quickly Copy** your IP Address to the clipboard 


## Images
IP Address on hover:

![IP Address on hover](Images/IPAddress.png)

IP Address Changed notification:

![IP Address Changed notification](Images/IPAddressChanged.png)

Connection Lost notification:

![Connection Lost notification](Images/ConnectionLost.png)

Right click options:

![Right click options](Images/RightClick.png)

## Configuration
The Settings/appsettings.json file contains configuration settings. You must restart the app to refresh changes to the settings file.
- **NotificationsEnabled**: If true will enable notifications on IP address changes. If false, it will not show notifications.
- **NotificationTimeMs**: The time in milliseconds that the notification will display.
- **Mode**: Either "Timed" or "OnNetworkChanges". Timed mode polls the IP Address at regular interval. OnNetworkChanges mode reacts to network change events.
- **PollingTimeMs**: The time in milliseconds between polls in Timed mode.

## Dependencies
- Windows 10/11
- [.NET 6.0+](https://dotnet.microsoft.com/en-us/download)
