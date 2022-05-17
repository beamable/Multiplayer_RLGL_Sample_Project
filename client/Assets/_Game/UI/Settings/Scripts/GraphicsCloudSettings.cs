using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class GraphicsCloudSettings : SettingsController
{
    [SerializeField] private SettingsCloudSave settingsCloudSave;
    private PlayerGraphicsSettings _graphicsSettings;

    [UsedImplicitly]
    public void SetGraphicsSettings(PlayerGraphicsSettings settings)
    {
        if (settings.settings.Count < 1)
        {
            _graphicsSettings = new PlayerGraphicsSettings();
            SetSettingsToDefault();
        }
        else
        {
            _graphicsSettings = settings;
        }
        SetGraphicsUI();
    }

    private void SetGraphicsUI()
    {
        var settingConfigs = SettingConfigsInChildren();
        foreach (var config in settingConfigs)
        {
            var settingValue = "";
            foreach (var setting in _graphicsSettings.settings)
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
        _graphicsSettings.settings.Clear();
        foreach (var setting in currentSettings)
        {
            _graphicsSettings.settings.Add(new PlayerGraphicsSettings.Setting() {key = setting.Key, value = setting.Value});
        }
        SaveSettings();
    }

    public void SetSettingsToDefault()
    {
        var defaultSettings = GetDefaultSettings();
        _graphicsSettings.settings.Clear();
        foreach (var setting in defaultSettings)
        {
            _graphicsSettings.settings.Add(new PlayerGraphicsSettings.Setting() {key = setting.Key, value = setting.Value});
        }
        SaveSettings();
        SetGraphicsUI();
    }

    [UsedImplicitly]
    public void SetSetting(string key, string value)
    {
        var oldValue = new PlayerGraphicsSettings.Setting();
        var newValue = new PlayerGraphicsSettings.Setting();

        var query = _graphicsSettings.settings.Where(setting => setting.key == key).ToList();
        if (!query.Any())
        {
            newValue = new PlayerGraphicsSettings.Setting() {key = key, value = value};
        }
        else
        {
            oldValue = query.First();
            newValue = new PlayerGraphicsSettings.Setting() {key = oldValue.key, value = value};
            if (oldValue.key == null) return;
            _graphicsSettings.settings.Remove(oldValue);
        }
        _graphicsSettings.settings.Add(newValue);
        SaveSettings();
        SetGraphicsUI();
    }

    public void SaveSettings()
    {
        settingsCloudSave.SaveSettings(_graphicsSettings, settingsCloudSave.GraphicsFileName);
    }
}
