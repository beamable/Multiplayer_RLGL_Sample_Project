using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.Avatars;
using ListView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IconChoiceMenu : MonoBehaviour
{
    [SerializeField] private ListViewComponent listViewComponent;
    [SerializeField] private ToggleGroup toggleGroup;
    private AccountAvatar _selectedAvatar;
    private string _currentAvatarId;

    public UnityEvent<string> OnSaveIcon;
    public UnityEvent OnUpdateList;

    public void UpdateList()
    {
        OnUpdateList?.Invoke();
        List<AccountAvatar> accountAvatars = AvatarConfiguration.Instance.Avatars;
        var cardData = new ListViewData();

        foreach (var avatar in accountAvatars)
        {
            cardData.Add((new ListItem
            {
                PropertyBag = new Dictionary<string, object>()
                {
                    {"account avatar", avatar},
                    {"toggle group", toggleGroup},
                    {"current avatar id", _currentAvatarId},
                    {"menu", this}
                },
                ListPrefabIndex = 0
            }));
        }
        listViewComponent.Build(cardData);
    }

    public void SaveIcon()
    {
        OnSaveIcon?.Invoke(_selectedAvatar.Name);
    }

    public void SetSelectedAvatar(AccountAvatar avatar)
    {
        _selectedAvatar = avatar;
    }

    public void GetCurrentAvatar(string avatarId)
    {
        _currentAvatarId = avatarId;
    }
}
