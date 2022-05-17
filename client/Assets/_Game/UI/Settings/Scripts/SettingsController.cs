using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    protected Dictionary<string, string> GetDefaultSettings()
    {
        var settingConfigs = SettingConfigsInChildren();

        return settingConfigs.ToDictionary(config => config.settingKey, config => config.defaultValue);
    }

    protected Dictionary<string, string> GetCurrentSettings()
    {
        var settingConfigs = SettingConfigsInChildren();

        return settingConfigs.ToDictionary(config => config.settingKey, config => config.settingValue);
    }

    protected List<SettingConfig> SettingConfigsInChildren()
    {
        var childrenWithConfig = new List<SettingConfig>();
        var children = (from Transform child in transform select child.gameObject).ToList();
        foreach (var child in children)
        {
            var settingConfigs = child.GetComponentsInChildren<SettingConfig>();
            if (settingConfigs == null) continue;
            childrenWithConfig.AddRange(settingConfigs);
        }

        return childrenWithConfig;
    }

    protected void SetSettingUIValue(string settingValue, SettingConfig config)
    {
       switch (config.settingType)
            {
                case SettingConfig.SettingType.Slider:
                {
                    if (config.gameObject.TryGetComponent<Slider>(out var slider))
                    {
                        if (float.TryParse(settingValue, out var value))
                        {
                            slider.value = value;
                        }
                    }
                    break;
                }
                case SettingConfig.SettingType.Toggle:
                {
                    if (config.gameObject.TryGetComponent<Toggle>(out var toggle))
                    {
                        if (bool.TryParse(settingValue, out var value))
                        {
                            toggle.isOn = value;
                        }
                    }

                    break;
                }
                case SettingConfig.SettingType.ListSelection:
                    if (config.gameObject.TryGetComponent<TextMeshProUGUI>(out var settingText))
                    {
                        settingText.text = settingValue;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
    }
}
