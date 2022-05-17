namespace ListView
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class ListViewComponent : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private List<GameObject> prefabs;

        public virtual void Build(ListViewData data, Action postRefreshAction = null)
        {
            ClearList();

            foreach (var item in data)
            {
                var spawnedCard = SpawnListCard(item.ListPrefabIndex);
                
                spawnedCard.listItem = item;
                
                spawnedCard.SetTitle(item.Title);
                spawnedCard.AddButtonListener(item.ViewAction);
                spawnedCard.SetUp(item);
            }

            postRefreshAction?.Invoke();
        }

        private void ClearList()
        {
            var childListCards = GetComponentsInChildren<ListCard>();
            foreach (var child in childListCards)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        private ListCard SpawnListCard(int prefabIndex)
        {
            var go = Instantiate(prefabs[prefabIndex], content.transform);
            var listCard = go.GetComponent<ListCard>();
            if (listCard == null)
            {
                Debug.LogError("ListCard component not found on " + gameObject.name + "!"
                               + " Ensure the ListView's assigned prefab inherits from ListCard.");
            }
            return listCard;
        }
    }
}