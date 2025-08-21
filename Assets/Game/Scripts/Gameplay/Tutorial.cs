using System.Collections;
using UnityEngine;
namespace Game.Gameplay
{
	public class Tutorial : GameBehaviour
	{
		[SerializeField] GameObject z;
		[SerializeField] GameObject x;
		[SerializeField] GameObject q;
		[SerializeField] GameObject w;
		[SerializeField] GameObject a;
		[SerializeField] GameObject s;
		[SerializeField] GameObject airWall;
		void Awake()
		{
			StartCoroutine(Unlock(KeyCode.Z, z));
			StartCoroutine(Unlock(KeyCode.X, x));
		}
		public void ShowQW()
		{
			Show(KeyCode.Q, q);
			Show(KeyCode.W, w);
		}
		public void ShowAS()
		{
			Show(KeyCode.A, a);
			Show(KeyCode.S, s);
		}
		void Show(KeyCode key, GameObject target)
		{
			GameRoot.Player.Unlock(key, false);
			target.gameObject.SetActive(true);
			StartCoroutine(Unlock(key, target));
		}
		IEnumerator Unlock(KeyCode key, GameObject target)
		{
			while (true)
			{
				if (Input.GetKeyDown(key))
				{
					target.GetComponent<BackgroundHint>().Hide();
					GameRoot.UiCamera.Hints[key].Show();
					break;
				}
				yield return null;
			}
			var locked = GameRoot.Player.Locked;
			if (locked is { leftBackward: false, leftForward: false, leftUp: false, rightBackward: false, rightForward: false, rightUp: false, })
				airWall.SetActive(false);
		}
	}
}
