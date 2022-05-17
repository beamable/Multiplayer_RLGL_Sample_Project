using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class SettingConfig : MonoBehaviour
{
    public string settingKey;
    [HideInInspector] public string settingValue;
    public string defaultValue;
    public UnityEvent<string, string> OnValueChange;
    public enum SettingType {Slider, Toggle, ListSelection}

    public SettingType settingType;
    
    [Serializable]
    public class NamedColorValue
    {
        public string colorName;
        public Color colorValue;
    }

    [Serializable]
    public class NamedFloatValue
    {
        public string floatName;
        public float floatValue;
    }
    public List<NamedColorValue> colorValues = new List<NamedColorValue>();
    public List<NamedFloatValue> floatValues = new List<NamedFloatValue>();

    [UsedImplicitly]
    public void SetSettingValue(float value)
    {
        DefaultIfEmpty();
        settingValue = value.ToString();
        OnValueChange?.Invoke(settingKey, settingValue);
    }

    [UsedImplicitly]
    public void SetSettingValue(bool value)
    {
        DefaultIfEmpty();
        settingValue = value.ToString();
        OnValueChange?.Invoke(settingKey, settingValue);
    }

    [UsedImplicitly]
    public void NextSettingValue()
    {
        DefaultIfEmpty();
        var index = 0;
        if (colorValues != null && colorValues.Count > 1)
        {
            var color = colorValues.Where(color => color.colorName == settingValue).ToList();
            if (!color.Any())
            {
                settingValue = defaultValue;
            }
            else
            {
                index = colorValues.IndexOf(color.First());
                index = IncreaseAndWrap(index, colorValues.Count);
                settingValue = colorValues[index].colorName;
            }
        }
        else if (floatValues != null && floatValues.Count > 1)
        {
            var value = floatValues.Where(value => value.floatName == settingValue).ToList();
            if (!value.Any())
            {
                settingValue = defaultValue;
            }
            else
            {
                index = floatValues.IndexOf(value.First());
                index = IncreaseAndWrap(index, floatValues.Count);
                settingValue = floatValues[index].floatName;
            }
        }
        OnValueChange?.Invoke(settingKey, settingValue);
    }

    [UsedImplicitly]
    public void PreviousSettingValue()
    {
        DefaultIfEmpty();
        var index = 0;
        if (colorValues != null && colorValues.Count > 1)
        {
            var color = colorValues.Where(color => color.colorName == settingValue).ToList();
            if (!color.Any())
            {
                settingValue = defaultValue;
            }
            else
            {
                index = colorValues.IndexOf(color.First());
                index = DecreaseAndWrap(index, colorValues.Count);
                settingValue = colorValues[index].colorName;
            }
        }
        else if (floatValues != null && floatValues.Count > 1)
        {
            var value = floatValues.Where(value => value.floatName == settingValue).ToList();
            if (!value.Any())
            {
                settingValue = defaultValue;
            }
            else
            {
                index = floatValues.IndexOf(value.First());
                index = DecreaseAndWrap(index, floatValues.Count);
                settingValue = floatValues[index].floatName;
            }
        }
        OnValueChange?.Invoke(settingKey, settingValue);
    }
    
    private int IncreaseAndWrap(int index, int count)
    {
        if (index == count-1)
        {
            return 0;
        }
        return index + 1;
    }
    
    private int DecreaseAndWrap(int index, int count)
    {
        if (index == 0)
        {
            return count - 1;
        }
        return index - 1;
    }

    private void DefaultIfEmpty()
    {
        if (settingValue == string.Empty)
        {
            settingValue = defaultValue;
        }
    }
}
