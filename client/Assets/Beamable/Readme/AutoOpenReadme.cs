using Beamable.Samples.Core;
using Beamable.Samples.SampleProjectBase;
using UnityEditor;

namespace Beamable.Samples.TBF
{
	/// <summary>
	/// Automatically focus on the sample project readme file when Unity first opens.
	/// </summary>
	[CustomEditor(typeof(Readme))]
	[InitializeOnLoad]
	public class AutoOpenReadme : ReadmeEditor
	{
		private const string FindAssetsFilter = "Readme t:Readme";
		private const string SessionStateKeyWasAlreadyShown = "Beamable.Samples.TBF.AutoOpenReadme.wasAlreadyShown";
		private static string[] FindAssetsFolders = new string[] { "Assets" };

		static AutoOpenReadme()
		{
			EditorApplication.delayCall += SelectReadmeAutomatically;
		}

		private static void SelectReadmeAutomatically()
		{
			if (!SessionState.GetBool(SessionStateKeyWasAlreadyShown, false))
			{
				SelectSpecificReadmeMenuItem();
				SessionState.SetBool(SessionStateKeyWasAlreadyShown, true);
			}
		}

		[MenuItem("Window/Beamable/Samples/Multiplayer/Open RLGL Readme", priority = 60)]
		private static Readme SelectSpecificReadmeMenuItem()
		{
			SessionState.SetBool(SessionStateKeyWasAlreadyShown, false);
			return SelectReadme(FindAssetsFilter, FindAssetsFolders);
		}
	}
}
