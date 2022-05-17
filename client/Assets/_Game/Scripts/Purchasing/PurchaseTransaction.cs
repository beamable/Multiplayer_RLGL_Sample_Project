namespace Beamable.IAP
{
    using UnityEngine;
    using System;
    using UnityEngine.Events;
    using Api.Payments;    
   
    public class PurchaseTransaction
    {
        public UnityEvent<Exception> OnPurchaseFailed;
        public UnityEvent<CompletedTransaction> OnPurchaseSuccess;
        
        private readonly string _storeId;
        private readonly string _listingId;
        private readonly BeamContext _context;
        
        public PurchaseTransaction(string storeId, string listingId, BeamContext context)
        {
            _storeId = storeId;
            _listingId = listingId;
            _context = context;

            OnPurchaseFailed = new UnityEvent<Exception>();
            OnPurchaseSuccess = new UnityEvent<CompletedTransaction>();
        }

        public async void MakePurchase()
        {
            var skuId = _listingId.Replace("skus.", "");
            var skusResponse = await _context.Api.PaymentService.GetSKUs();
            var sku = skusResponse.skus.definitions.Find(i => i.name == skuId);
            if (sku == null)
            {
                OnPurchaseFailed.Invoke(new Exception("Sku not found."));
                return;
            };
         
            var purchaser = await _context.Api.BeamableIAP;
            var purchaseResult = await purchaser.StartPurchase($"{_listingId}:{_storeId}", sku.name)
                .Error(OnPurchaseError);

            if (string.IsNullOrEmpty(purchaseResult.Receipt)) return;
            OnPurchaseSuccess.Invoke(purchaseResult);
        }

        private void OnPurchaseError(Exception error)
        {
            if (error == null) return;
            OnPurchaseFailed.Invoke(error);
        }
    }
}
