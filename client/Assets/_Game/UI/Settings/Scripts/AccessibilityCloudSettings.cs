using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class AccessibilityCloudSettings : SettingsController
{
    [SerializeField] private SettingsCloudSave settingsCloudSave;
    private PlayerAccessibilitySettings _accessibilitySettings;

    [UsedImplicitly]
    public void SetAccessibilitySettings(PlayerAccessibilitySettings settings)
    {
        if (settings.settings.Count < 1)
        {
            _accessibilitySettings = new PlayerAccessibilitySettings();
            SetSettingsToDefault();
        }
        else
        {
            _accessibilitySettings = settings;
        }
        SetAccessibilityUI();
    }

    private void SetAccessibilityUI()
    {
        var settingConfigs = SettingConfigsInChildren();
        foreach (var config in settingConfigs)
        {
            var settingValue = "";
            foreach (var setting in _accessibilitySettings.settings)
            {
                if (setting.key == config.settingKey)
                {
                    settingValue = setting.value;
                    break;
                }
                settingValue = config.defaultValue;
            }
            SetSettingUIValue(settingValue, config);
        }
    }

    public void SetSettingsToCurrent()
    {
        var currentSettings = GetCurrentSettings();
        _accessibilitySettings.settings.Clear();
        foreach (var setting in currentSettings)
        {
            _accessibilitySettings.settings.Add(new PlayerAccessibilitySettings.Setting() {key = setting.Key, value = setting.Value});
        }
        SaveSettings();
    }

    public void SetSettingsToDefault()
    {
        var defaultSettings = GetDefaultSettings();
        _accessibilitySettings.settings.Clear();
        foreach (var setting in defaultSettings)
        {
            _accessibilitySettings.settings.Add(new PlayerAccessibilitySettings.Setting() {key = setting.Key, value = setting.Value});
        }
        SaveSettings();
        SetAccessibilityUI();
    }

    [UsedImplicitly]
    public void SetSetting(string key, string value)
    {
        var oldValue = new PlayerAccessibilitySettings.Setting();
        var newValue = new PlayerAccessibilitySettings.Setting();

        var query = _accessibilitySettings.settings.Where(setting => setting.key == key).ToList();
        if (!query.Any())
        {
            newValue = new PlayerAccessibilitySettings.Setting() {key = key, value = value};
        }
        else
        {
            oldValue = query.First();
            newValue = new PlayerAccessibilitySettings.Setting() {key = oldValue.key, value = value};
            if (oldValue.key == null) return;
            _accessibilitySettings.settings.Remove(oldValue);
        }
        _accessibilitySettings.settings.Add(newValue);
        SaveSettings();
        SetAccessibilityUI();
    }

    public void SaveSettings()
    {
        settingsCloudSave.SaveSettings(_accessibilitySettings, settingsCloudSave.AccessibilityFileName);
    }
}
