using System.Collections.Generic;
using Beamable.Common.Api.Auth;

namespace _Game.UI.Login.Scripts
{
    public class UserAccount
    {
        public string userId;
        public string alias;
        public string avatar;
        public string email;
        public Dictionary<string, string> customData;
        public AuthThirdParty[] thirdPartyAssociations;
    }
}