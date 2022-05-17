using Beamable.Avatars;
using ListView;
using UnityEngine;
using UnityEngine.UI;

public class IconItemCard : ListCard
{
    [SerializeField] private Image image;
    [SerializeField] private Toggle toggle;
    private AccountAvatar _avatar;
    private string _currentAvatarId;
    private IconChoiceMenu _menu;

    public override void SetUp(ListItem item)
    {
        _avatar = item.PropertyBag["account avatar"] as AccountAvatar;
        image.sprite = _avatar.Sprite;
        toggle.isOn = false;
        toggle.group = item.PropertyBag["toggle group"] as ToggleGroup;
        _currentAvatarId = item.PropertyBag["current avatar id"] as string;
        _menu = item.PropertyBag["menu"] as IconChoiceMenu;
        SelectAtSetUp();
    }

    public void SelectThis(bool isOn)
    {
        if (_menu != null && isOn)
        {
            _menu.SetSelectedAvatar(_avatar);
        }
    }

    public void SelectAtSetUp()
    {
        if (_avatar.Name == _currentAvatarId)
        {
            toggle.isOn = true;
        }
    }
}
