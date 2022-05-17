#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class SceneLayoutHelper : MonoBehaviour
{


    public class InstanceIndexMap
    {
        public string path;
        public List<int> indices;
    }

    public SourceToTarget[] sourceToTargets;

    [ContextMenu("Set to Target")]
    public void ReplaceSourceWithTarget()
    {
        if (sourceToTargets == null || sourceToTargets.Length == 0)
            return;

        GameObject[] gameObjectsInScene = GameObject.FindObjectsOfType<GameObject>();

        for (int j = 0; j < sourceToTargets.Length; j++)
        {
            if (!sourceToTargets[j].Replace) continue;
            GameObject[] gameObjectsToCheck = gameObjectsInScene;
            if (Selection.gameObjects != null && sourceToTargets[j].UseSelectionOnly)
            {
                gameObjectsToCheck = Selection.gameObjects;
            }

            List<InstanceIndexMap> instances = new List<InstanceIndexMap>();
            for (int i = 0; i < gameObjectsToCheck.Length; i++)
            {
                if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObjectsToCheck[i]) == sourceToTargets[j].source)
                {
                    if (PrefabUtility.IsOutermostPrefabInstanceRoot(gameObjectsToCheck[i]))
                    {
                        GameObject replacedObject = ReplaceObject(gameObjectsToCheck[i], sourceToTargets[j].target, sourceToTargets[j].IgnoreScale);
                    }
                    else if (PrefabUtility.IsPartOfPrefabInstance(gameObjectsToCheck[i]))
                    {
                        GameObject parent = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObjectsToCheck[i]);
                        Transform[] t = parent.GetComponentsInChildren<Transform>();
                        GameObject source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(parent);
                        string sourcePath = AssetDatabase.GetAssetPath(source);

                        int index = -1;
                        for (int x = 0; x < t.Length; x++)
                        {
                            if (t[x].gameObject == gameObjectsToCheck[i])
                            {
                                index = x;
                                break;
                            }
                        }

                        if (index == -1)
                            continue;

                        bool contains = false;
                        foreach (InstanceIndexMap map in instances)
                        {
                            if (map.path == sourcePath)
                            {
                                contains = true;
                                map.indices.Add(index);
                                break;
                            }
                        }
                        if (!contains)
                        {
                            InstanceIndexMap newMap = new InstanceIndexMap();
                            newMap.path = sourcePath;
                            newMap.indices = new List<int>();
                            newMap.indices.Add(index);
                            instances.Add(newMap);
                        }

                    }
                }
            }

            foreach (InstanceIndexMap map in instances)
            {
                using (var editingScope = new PrefabUtility.EditPrefabContentsScope(map.path))
                {

                    Transform[] t = editingScope.prefabContentsRoot.GetComponentsInChildren<Transform>();
                    for (int i = t.Length - 1; i > 0; i--)
                    {
                        if (t[i].gameObject == editingScope.prefabContentsRoot)
                            continue;
                        if (sourceToTargets[j].UseSelectionOnly && !map.indices.Contains(i))
                            continue;
                        GameObject obj = t[i].gameObject;
                        if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null)
                        {
                            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) == sourceToTargets[j].source)
                            {

                                GameObject replacedObject = ReplaceObject(obj, sourceToTargets[j].target, sourceToTargets[j].IgnoreScale);
                            }
                        }
                    }
                }
            }
        }
    }


    [ContextMenu("Set To Source")]
    public void ReplaceTargetWithSource()
    {
        if (sourceToTargets == null || sourceToTargets.Length == 0)
            return;

        GameObject[] gameObjectsInScene = GameObject.FindObjectsOfType<GameObject>();

        for (int j = 0; j < sourceToTargets.Length; j++)
        {
            if (!sourceToTargets[j].Replace) continue;
            GameObject[] gameObjectsToCheck = gameObjectsInScene;
            if (Selection.gameObjects != null && sourceToTargets[j].UseSelectionOnly)
            {
                gameObjectsToCheck = Selection.gameObjects;
            }

            List<InstanceIndexMap> instances = new List<InstanceIndexMap>();
            for (int i = 0; i < gameObjectsToCheck.Length; i++)
            {
                if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObjectsToCheck[i]) == sourceToTargets[j].target)
                {
                    if (PrefabUtility.IsOutermostPrefabInstanceRoot(gameObjectsToCheck[i]))
                    {
                        GameObject replacedObject = ReplaceObject(gameObjectsToCheck[i], sourceToTargets[j].source, sourceToTargets[j].IgnoreScale);
                    }
                    else if (PrefabUtility.IsPartOfPrefabInstance(gameObjectsToCheck[i]))
                    {
                        GameObject parent = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObjectsToCheck[i]);
                        Transform[] t = parent.GetComponentsInChildren<Transform>();
                        GameObject source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(parent);
                        string sourcePath = AssetDatabase.GetAssetPath(source);

                        int index = -1;
                        for (int x = 0; x < t.Length; x++)
                        {
                            if (t[x].gameObject == gameObjectsToCheck[i])
                            {
                                index = x;
                                break;
                            }
                        }

                        if (index == -1)
                            continue;

                        bool contains = false;
                        foreach (InstanceIndexMap map in instances)
                        {
                            if (map.path == sourcePath)
                            {
                                contains = true;
                                map.indices.Add(index);
                                break;
                            }
                        }
                        if (!contains)
                        {
                            InstanceIndexMap newMap = new InstanceIndexMap();
                            newMap.path = sourcePath;
                            newMap.indices = new List<int>();
                            newMap.indices.Add(index);
                            instances.Add(newMap);
                        }

                    }
                }
            }

            foreach (InstanceIndexMap map in instances)
            {
                using (var editingScope = new PrefabUtility.EditPrefabContentsScope(map.path))
                {

                    Transform[] t = editingScope.prefabContentsRoot.GetComponentsInChildren<Transform>();
                    for (int i = t.Length - 1; i > 0; i--)
                    {
                        if (t[i].gameObject == editingScope.prefabContentsRoot)
                            continue;
                        if (!map.indices.Contains(i))
                            continue;
                        GameObject obj = t[i].gameObject;
                        if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null)
                        {
                            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) == sourceToTargets[j].target)
                            {

                                GameObject replacedObject = ReplaceObject(obj, sourceToTargets[j].source, sourceToTargets[j].IgnoreScale);
                            }
                        }
                    }
                }
            }
        }
    }

    public GameObject ReplaceObject(GameObject objectToReplace, GameObject replacementObject, bool ignoreScale)
    {

        GameObject replacedPrefab = PrefabUtility.InstantiatePrefab(replacementObject) as GameObject;

        replacedPrefab.transform.parent = objectToReplace.transform.parent;
        replacedPrefab.transform.SetSiblingIndex(objectToReplace.transform.GetSiblingIndex() + 1);

        replacedPrefab.transform.position = objectToReplace.transform.position;
        replacedPrefab.transform.rotation = objectToReplace.transform.rotation;
        if (!ignoreScale)
            replacedPrefab.transform.localScale = objectToReplace.transform.localScale;

        replacedPrefab.gameObject.layer = objectToReplace.layer;
        replacedPrefab.gameObject.tag = objectToReplace.tag;

        GameObjectUtility.SetStaticEditorFlags(replacedPrefab, GameObjectUtility.GetStaticEditorFlags(objectToReplace));

        Undo.RegisterCreatedObjectUndo(replacedPrefab, "Create Replacement Object");
        Undo.DestroyObjectImmediate(objectToReplace);


        return replacedPrefab;



    }

}


[System.Serializable]
public class SourceToTarget
{
    public bool Replace = true;
    public GameObject source;
    public GameObject target;
    public bool IgnoreScale = false;
    public bool UseSelectionOnly = false;
}
#endif