# Identity and Authentication

### In-Game Use

When first opening the game, we use authentication and identity to give the user choices of how to log in. In addition, we check if a user has previously logged in on the device and display any existing accounts as login options. If a user does not have an account, they can create one through email or by logging in with a third party service. The sign in options currently available are email, Facebook, Google, and Steam. The email option is on all devices. Facebook and Google are only available on mobile. Steam is only available on PC, but will only be active if the user has Steam and the game in their Steam library. The email login method includes a password recovery flow. After logging in either manually or automatically, the active account will be displayed as the current account and any other accounts on the device will be listed in the "other accounts" section. The user is able to switch from the currently logged in account to one of the other accounts by selecting the account from the list and then pressing the "Switch Account" button at the bottom of the screen. The lists of current and other accounts will update accordingly. Selecting the "Load Game" button will launch the game with whichever account is currently logged in and displayed in the current account section.

### Class Breakdown

**AuthenticationHandler** - Base class for logging users in.
- Beamable's Auth Service is used to check if third parties are available to login with, to login with those third parties, to create users, and to link third parties to users.
- Classes that inherit from this:
   - **EmailLoginHandler**
   - **FacebookLoginHandler**
   - **GoogleLoginHandler**
   - **SteamLoginHandler**
   - **FrictionlessLoginHandler**

**ForgotPassword** - Flow for getting a code to set up a new password.
- Uses Beamable's Auth Service to issue a password update code to the user's email and to confirm and change the user's password.

**AccountSelectionController** - Gets the users that have logged into the device and tracks which account is currently selected in order to switch to the selected account.
- Beamable Context is used to get any accounts that have logged into the game on the current device, and to updated the currently logged-in player.

**AccountMenu** - Base class for displaying the UI for one or more user accounts.
- Uses Beamable's stats service to get the user's stats to display on their account card.
- Classes that inherit from this:
    - **SingleAccountMenu**
    - **MultipleAccountMenu**


> To learn more about the basic usage of the Identity and Authentication features, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/identity).
