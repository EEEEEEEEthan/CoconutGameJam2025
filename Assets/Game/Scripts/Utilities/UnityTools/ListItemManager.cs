using System.Collections.Generic;
using Game.Utilities.UnityTools.Attributes;
using UnityEngine;
namespace Game.Utilities.UnityTools
{
	public sealed class ListItemManager : MonoBehaviour
	{
		[SerializeField] [PrefabSourceOnly] GameObject template;
		[SerializeField] [PrefabSourceOnly] GameObject showOnEmpty;
		readonly List<GameObject> list = new();
		readonly List<GameObject> inactiveList = new();
		public IReadOnlyList<GameObject> ActiveItems => list;
		void Awake()
		{
			template.SetActive(false);
			RefreshEmptyItem();
		}
		public GameObject Append()
		{
			if (inactiveList.TryPopLast(out var obj))
				obj.transform.SetAsLastSibling();
			else
				obj = Instantiate(template, transform);
			obj.SetActive(true);
			list.Add(obj);
			RefreshEmptyItem();
			return obj;
		}
		public void Clear()
		{
			foreach (var obj in list)
			{
				obj.SetActive(false);
				inactiveList.Add(obj);
			}
			list.Clear();
			RefreshEmptyItem();
		}
		void RefreshEmptyItem()
		{
			if (showOnEmpty) showOnEmpty.SetActive(list.Count <= 0);
		}
	}
}
