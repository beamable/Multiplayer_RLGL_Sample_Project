using System.Collections;
using System.Collections.Generic;
using Beamable.Avatars;
using ListView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountCard : ListCard
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI scoreText;

    public override void SetUp(ListItem item)
    {
        SetTitle(item.Title);
        var score = (string)item.PropertyBag["score"];
        var avatarId = (string) item.PropertyBag["avatar"];
        
        if (!string.IsNullOrWhiteSpace(score))
        {
            scoreText.text = "Position: " + score;
        }
        else
        {
            scoreText.enabled = false;
        }

        avatarImage.sprite = !string.IsNullOrWhiteSpace(avatarId) ? GetAvatar(avatarId) : 
            AvatarConfiguration.Instance.Default.Sprite;
    }
    
    private Sprite GetAvatar(string id)
    {
        List<AccountAvatar> accountAvatars = AvatarConfiguration.Instance.Avatars;
        AccountAvatar accountAvatar = accountAvatars.Find(avatar => avatar.Name == id);
        return accountAvatar.Sprite;
    }
}
