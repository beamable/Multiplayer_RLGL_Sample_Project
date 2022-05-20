using System.Collections;
using System.Collections.Generic;
using Beamable;
using UnityEngine;

public class AccountAchievement : MonoBehaviour
{
    protected const string CREATE_ACCOUNT_KEY = "CREATE_AN_ACCOUNT";
    
    private async void Awake()
    {
        var beamableAPI = await API.Instance;
        if (beamableAPI.User.HasAnyCredentials())
        {
            await BeamableStatsController.ChangeStat(CREATE_ACCOUNT_KEY, "True");
        }
    }
}
