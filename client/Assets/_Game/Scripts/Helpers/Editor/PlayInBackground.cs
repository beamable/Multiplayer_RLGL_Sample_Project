 using UnityEditor;
 using UnityEngine;
 
 [InitializeOnLoad]
 public class PlayInBackground : MonoBehaviour {
 
     const string ITEM = "Edit/Play In Background";
 
     static PlayInBackground () {
         EditorApplication.delayCall += InitMenu;
     }
 
     static void InitMenu () {
         Menu.SetChecked(ITEM, Application.runInBackground);
     }
 
     [MenuItem(ITEM, priority = 300)]
     public static void TogglePlayInBackground () {
         Menu.SetChecked(ITEM, !Menu.GetChecked(ITEM));
         Application.runInBackground = Menu.GetChecked(ITEM);
     }
 
 }