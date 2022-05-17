using System.Collections.Generic;
using ListView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCard : ListCard
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [Space]
    [SerializeField] private Image background;
    [SerializeField] private Sprite currentPlayerBackground;
    [SerializeField] private Sprite genericPlayerBackground;
    public override void SetUp(ListItem item)
    {
        SetTitle(item.Title);
        rankText.text = item.PropertyBag["rank"].ToString();
        scoreText.text = item.PropertyBag["score"].ToString();
        SetBackground((bool) item.PropertyBag["current_player"]);
    }

    private void SetBackground(bool isCurrentPlayer)
    {
        if (isCurrentPlayer)
        {
            if (currentPlayerBackground != null)
            {
                background.sprite = currentPlayerBackground;
            }
        }
        else
        {
            if (genericPlayerBackground != null)
            {
                background.sprite = genericPlayerBackground;
            }
        }
    }
}
