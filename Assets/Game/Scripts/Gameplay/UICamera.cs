using System.Collections.Generic;
using Game.Gameplay.Hints;
using Game.ResourceManagement;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay
{
	public class UICamera : GameBehaviour
	{
		[SerializeField] Transform bar;
		[SerializeField] float spacing;
		readonly Dictionary<KeyCode, Hint> hints = new();
		public IReadOnlyDictionary<KeyCode, Hint> Hints => hints;
		void Awake()
		{
			var x = 0f;
			instantiateHint(ResourceTable.hintZPrefab.Main, KeyCode.Z, x += spacing);
			instantiateHint(ResourceTable.hintXPrefab.Main, KeyCode.X, x += spacing);
			instantiateHint(ResourceTable.hintAPrefab.Main, KeyCode.A, x += spacing);
			instantiateHint(ResourceTable.hintSPrefab.Main, KeyCode.S, x += spacing);
			instantiateHint(ResourceTable.hintWPrefab.Main, KeyCode.W, x += spacing);
			instantiateHint(ResourceTable.hintQPrefab.Main, KeyCode.Q, x += spacing);
			instantiateHint(ResourceTable.hint1Prefab.Main, KeyCode.Alpha1, x += spacing);
			instantiateHint(ResourceTable.hint2Prefab.Main, KeyCode.Alpha2, x += spacing);
			instantiateHint(ResourceTable.hint3Prefab.Main, KeyCode.Alpha3, x += spacing);
			instantiateHint(ResourceTable.hint4Prefab.Main, KeyCode.Alpha4, x += spacing);
			return;
			void instantiateHint(GameObject prefab, KeyCode key, float x)
			{
				var hint = prefab.Instantiate(bar);
				hint.transform.localPosition = new(x, 0, 0);
				var h = hint.GetComponent<Hint>();
				h.Initialize(key);
				h.enabled = true;
				hints[key] = h;
			}
		}
	}
}
