namespace Beamable.UI
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(UiViewComponent))]
    [CanEditMultipleObjects]
    public class UiViewComponentEditor: Editor
    {
        private IUiViewComponent view;
        
        private void OnEnable()
        {
            view = (IUiViewComponent) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!Application.isPlaying) return;
            
            if (GUILayout.Button("Show"))
            {
                UiStateController.Show(view.ViewName, view.NameSpace);
            }

            if (GUILayout.Button("Hide"))
            {
                UiStateController.Hide(view.ViewName, view.NameSpace);
            }
        }
    }
}