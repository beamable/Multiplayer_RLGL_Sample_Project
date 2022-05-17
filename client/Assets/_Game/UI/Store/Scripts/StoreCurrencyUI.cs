using System.Collections;
using System.Collections.Generic;
using Beamable.Common.Inventory;
using Beamable.UI.Scripts;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreCurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentCurrency;
    [SerializeField] private Image currencyIcon;
    [SerializeField] private CurrencyRef currencyRef;
    
    [UsedImplicitly]
    public void SetCurrency(int amount)
    {
        if (currentCurrency == null) return;
        currentCurrency.text = $"{amount:n0}";
    }

    private void Start()
    {
        SetCurrencyIcon();
    }

    private async void SetCurrencyIcon()
    {
        if (currencyRef == null || currencyIcon == null) return;
        var currencyContent = await currencyRef.Resolve();
        currencyIcon.sprite = await currencyContent.icon.LoadSprite();
    }

}
