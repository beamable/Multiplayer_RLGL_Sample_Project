using Beamable.UI.Scripts;
using ListView;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace _Game.UI.Account.Scripts
{
    public class AchievementCard : ListCard
    {
        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private float incompleteAlpha = 0.2f;
        [SerializeField]
        private float completedAlpha = 1f;

        [SerializeField] private string secretTitle = "SECRET ACHIEVEMENT";
        [SerializeField] private string secretDescription = "Keep playing to unlock!";
        [SerializeField] private Image normalIcon;
        [SerializeField] private Image secretIcon;

        public override async void SetUp(ListItem item)
        {
            var description = (string)item.PropertyBag["description"];
            var complete = (bool)item.PropertyBag["complete"];
            var secret = (bool) item.PropertyBag["secret"];
            
            normalIcon.sprite = await (item.PropertyBag["icon"] as AssetReferenceSprite).LoadSprite();

            descriptionText.text = description;
            SetCompletedStatus(complete);
            if(!complete && secret) SetSecretStatus();
        }

        private void SetCompletedStatus(bool complete)
        {
            canvasGroup.alpha = complete ? completedAlpha : incompleteAlpha;
        }

        private void SetSecretStatus()
        {
            title.text = secretTitle;
            descriptionText.text = secretDescription;
            normalIcon.enabled = false;
            secretIcon.enabled = true;
        }
    }
}