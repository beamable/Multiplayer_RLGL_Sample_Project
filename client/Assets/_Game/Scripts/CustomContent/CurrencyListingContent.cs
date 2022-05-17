using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Shop;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Game.CustomContent
{
    [ContentType("currency_listing")]
    public class CurrencyListingContent : ListingContent
    {
        public AssetReferenceSprite icon;
    }
    
    [Serializable]
    public class CurrencyListingRef : ContentRef<CurrencyListingContent> { }
    [Serializable]
    public class CurrencyListingLink : ContentLink<CurrencyListingContent> { }
}
