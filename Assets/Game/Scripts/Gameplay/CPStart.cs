using System.Collections;
using UnityEngine;
namespace Game.Gameplay
{
	public class CPStart : MonoBehaviour
	{
		[SerializeField] Animator a;
		[SerializeField] Animator b;
		IEnumerator Start()
		{
			a.SetTrigger("Fight");
			yield return new WaitForSeconds(0.5f);
			b.SetTrigger("Fight");
		}
	}
}
