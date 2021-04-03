# FreshStart
Gives Windows 10 a fresh start, with better privacy and unwanted features disabled.

All of the features can be optionally disabled from [config.json](https://github.com/JokkeeZ/FreshStart/blob/main/FreshStart/config.json), just by commenting them out.

```diff
- NOTE: If user wants to disable some feature, it needs to be commented out from config.json before running the application!
```

User can also add registry keys to [config.json](https://github.com/JokkeeZ/FreshStart/blob/main/FreshStart/config.json#L69) and give them new values.

# Implemented features
All of these can be optionally disabled from [config.json](https://github.com/JokkeeZ/FreshStart/blob/main/FreshStart/config.json), just by commenting the objects out.

- Sets dark theme for system & apps
- Removes pre-installed packages. See [config.json](https://github.com/JokkeeZ/FreshStart/blob/main/FreshStart/config.json#L23)
  
- Disables:
  - Toggle keys
  - Sticky keys
  - "AllowTelemetry"
  - Cortana & Cortana consent
  - Web search
  - Silent installed apps
  - Pre-installed apps & Pre-installed apps ever
  - Oem Pre-installed apps
  - Slideshow
  - Suggestions
  - Bing search
  - Device history
  - App suggestions in future

- Modifies Windows 10 appearance:
  - Show file extensions in explorer
  - Hide Cortana button
  - Hide Task View button
  - Use small Taskbar buttons 

- Removes:
  - Suggested apps

- Denies apps to access:
  - Location
  - Camera
  - Account information
  - Contacts
  - Appointments
  - Phone calls & history
  - Cortana
  - Email
  - Tasks
  - Radios
  - Bluetooth sync
  - Gaze input
  - App diagnostics

- Disables services:
  - Connected User Experiences and Telemetry
  - Print Spooler
  - Fax
  - Xbox Live Networking
  - Xbox Accessory Management
  - Xbox Live Game Save
  - Xbox Live Auth Manager
  - WalletService
  - Telephony
  - Printer Extensions and Notifications
  - Downloaded Maps Manager
  - Net.Tcp Port Sharing
  - Parental Controls
  - Phone Service
  - Print Workflow
  - Radio Management
  - Remote Registry
  - Retail Demo
  - Routing and Remote Access
  - Shared PC Account Manager
  - Windows Mixed Reality OpenXR

- And probably something more, see [config.json](https://github.com/JokkeeZ/FreshStart/blob/main/FreshStart/config.json) for everything!

# Note
This application has been made to fit my own needs, so its up to the user to add or remove unwanted/wanted features.

# License
Licensed under MIT License. See more: [MIT License](https://github.com/JokkeeZ/FreshStart/blob/main/LICENSE)
