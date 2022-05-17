# Cloud Save

### In-Game Usage

Cloud save is used for saving the player's settings on their device and for keeping a record of the player's achievements. Files for cloud saving the player's settings and achievements are stored in the user's local low data. The settings cloud save updates whenever a setting is changed, and the achievement cloud save updates when the achievements are loaded on the player's profile page.

### Class Breakdown

- **SettingsCloudSave**
    - Has classes for each setting category that contain a list of structs with two string values; one that contains the setting key and the other that contains the setting value.
    - Uses Beamable's cloud saving to load and save the player's settings values to the device.
    - Uses Unity events when the settings are loaded to send a signal to update the setting UI.
- **SettingsController** - Base class for getting the current settings, default settings, and applying the setting UI values.
    - **AccessibilityCloudSettings** - Gets and applies the settings for the accessibility setting category.
    - **AudioCloudSettings** - Gets and applies the settings for the audio setting category.
    - **GraphicsCloudSettings** - Gets and applies the settings for the graphics setting category.
- **SettingConfig** - Holds the information about each setting.
    - Has the setting key that the setting is identified by, the setting's value, the setting's default value, a Unity event for when the value changes, and the type of setting for the kind of value the setting has.
    - The setting types used are slider, toggle, and a list selection; these update the values from the UI.
    - If a value cannot be found for the setting, it will be set to its default value.
- **AchievementCloudSave**
    - Uses Beamable's cloud saving service to load and save the player's achievement data to the device.
    - Uses Beamable's microservice feature to check whether the player has earned any achievements.
    - Uses Beamable's content service to load the achievements contents from a specified achievement_group content.
    - Saves the achievement name and whether or not it has been achieved in a list of a struct that has a string value and a bool value.


> To learn more about the basics of the Cloud Save feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/cloud-save-feature-overview).
