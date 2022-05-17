using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class AudioCloudSettings : SettingsController
{
    [SerializeField] private SettingsCloudSave settingsCloudSave;
    private PlayerAudioSettings _audioSettings;

    [UsedImplicitly]
    public void SetAudioSettings(PlayerAudioSettings settings)
    {
        if (settings.settings.Count < 1)
        {
            _audioSettings = new PlayerAudioSettings();
            SetSettingsToDefault();
        }
        else
        {
            _audioSettings = settings;
        }
        SetAudioUI();
    }

    private void SetAudioUI()
    {
        var settingConfigs = SettingConfigsInChildren();
        foreach (var config in settingConfigs)
        {
            var settingValue = "";
            foreach (var setting in _audioSettings.settings)
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
        _audioSettings.settings.Clear();
        foreach (var setting in currentSettings)
        {
            _audioSettings.settings.Add(new PlayerAudioSettings.Setting() {key = setting.Key, value = setting.Value});
        }
        SaveSettings();
    }

    public void SetSettingsToDefault()
    {
        var defaultSettings = GetDefaultSettings();
        _audioSettings.settings.Clear();
        foreach (var setting in defaultSettings)
        {
            _audioSettings.settings.Add(new PlayerAudioSettings.Setting() {key = setting.Key, value = setting.Value});
        }
        SaveSettings();
        SetAudioUI();
    }

    [UsedImplicitly]
    public void SetSetting(string key, string value)
    {
        var oldValue = new PlayerAudioSettings.Setting();
        var newValue = new PlayerAudioSettings.Setting();

        var query = _audioSettings.settings.Where(setting => setting.key == key).ToList();
        if (!query.Any())
        {
            newValue = new PlayerAudioSettings.Setting() {key = key, value = value};
        }
        else
        {
            oldValue = query.First();
            newValue = new PlayerAudioSettings.Setting() {key = oldValue.key, value = value};
            if (oldValue.key == null) return;
            _audioSettings.settings.Remove(oldValue);
        }
        _audioSettings.settings.Add(newValue);
        SaveSettings();
        SetAudioUI();
    }

    public void SaveSettings()
    {
        settingsCloudSave.SaveSettings(_audioSettings, settingsCloudSave.AudioFileName);
    }
}
