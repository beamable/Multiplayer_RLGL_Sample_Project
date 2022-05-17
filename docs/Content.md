# Content

### In-Game Usages

- **Store** - Holds the listings for what items to sell in each store, as well as the amount and type of currency needed to purchase them. The Head Store contains head items, the Body Store contains body items, and the Gem Store contains the gem currency to purchase for real world money. These can be seen in the store page in the top right of the lobby screen.
- **Items** - Items that can be obtained and added to the player's inventory. These are put into the store listings (with the exception of in the Gem Store, where SKUs are used instead). These can be seen in the inventory and store pages.
- **Currency** - Currencies the player can earn and spend at the store. These are part of the player's inventory, and can be seen in the store page.
- **Leaderboards** - Used to rank the players in a list based on scores from each match they play. The global leaderboard is displayed at the end of each match. The Leaderboard screen can also be accessed via the lobby.
- **Achievements** - Custom contents that store the requirements to be met by the player in order to earn an achievement. These can be seen in the player's profile page.

### Assembly Definitions

Assembly definitions are used to share content between client scripts and microservices. The definition for the content can be found in the Assets/_Game/CustomContent folder, which is referenced by the microservices definition (located in Assets/Beamable/Microservices/AchievementsService). To learn more about class sharing with microservices, read more on the [Microservice Class Sharing](https://docs.beamable.com/docs/microservices-class-sharing) documentation.

### Custom Content Types

- **achievement_group** - Holds a list of achievement content objects. This is referenced by the Achievement microservice to identify all of the current "active" achievements.
- **achievements** - Holds the name, description, icon, and requirements for an achievement and whether or not that achievement is secret. Requirements are either a string requirement that can be marked as true or false if met, or a count requirement for a number to be met. These requirements also have a stat key, indicating which player stat should be tracked to validate that the requirement has been met. Marking an achievement as secret is a way to signal to the UI that the name, description, and icon for the achievement should be masked until the player has earned it.
- **currency_listing** - A type of listing content. Adds a sprite to the normal listing content in order to allow listings to have their own image to show instead of the items' or currencies' image to be shown. This is used so that different amounts of gems being sold can have different icons.


> To learn more about basic usage of the Content feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/content-feature-overview).
