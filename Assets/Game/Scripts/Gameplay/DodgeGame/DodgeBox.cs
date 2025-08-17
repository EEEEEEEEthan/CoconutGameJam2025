using System.Collections;
using UnityEngine;
using Game.Gameplay;
namespace Game.Gameplay.DodgeGame
{
	public class DodgeBox : MonoBehaviour
	{
		[Header("飞行配置")]
		public float speed = 10f;
		public Vector3 targetPosition;
		[Header("溶解效果")]
		public float dissolveTime = 2f;
		public Material dissolveMaterial;
		[Header("碰撞检测")]
		public LayerMask playerLayer;
		private bool hasPassedTarget = false;
		private bool isDissolving = false;
		private Vector3 flyDirection;
		private Renderer boxRenderer;
		private Material originalMaterial;
		private Rigidbody boxRigidbody;
		public static System.Action<DodgeBox> OnBoxHitPlayer;
		public static System.Action<DodgeBox> OnBoxDodged;
        
		void Awake()
		{
			boxRenderer = GetComponent<Renderer>();
			boxRigidbody = GetComponent<Rigidbody>();
			if (boxRenderer != null)
			{
				originalMaterial = boxRenderer.material;
			}
		}
		void Start()
		{
			flyDirection = (targetPosition - transform.position).normalized;
			if (boxRigidbody != null)
			{
				boxRigidbody.velocity = flyDirection * speed;
			}
		}
		void Update()
		{
			if (!hasPassedTarget && transform.position.z < 0)
			{
				hasPassedTarget = true;
				OnBoxDodged?.Invoke(this);
				StartDissolve();
			}
			if (boxRigidbody == null)
			{
				transform.position += flyDirection * speed * Time.deltaTime;
			}
		}
        
		public void Initialize(Vector3 startPos, Vector3 targetPos, float flySpeed)
		{
			transform.position = startPos;
			targetPosition = targetPos;
			speed = flySpeed;
			hasPassedTarget = false;
			isDissolving = false;
			if (boxRenderer != null && originalMaterial != null)
			{
				boxRenderer.material = originalMaterial;
			}
		}
        
		void StartDissolve()
		{
			if (isDissolving) return;
			isDissolving = true;
			StartCoroutine(DissolveCoroutine());
		}
        
		IEnumerator DissolveCoroutine()
		{
			float elapsedTime = 0f;
			Color originalColor = Color.white;
			if (boxRenderer != null)
			{
				if (dissolveMaterial != null)
				{
					boxRenderer.material = dissolveMaterial;
				}
				originalColor = boxRenderer.material.color;
			}
			while (elapsedTime < dissolveTime)
			{
				elapsedTime += Time.deltaTime;
				float alpha = 1f - (elapsedTime / dissolveTime);
				if (boxRenderer != null)
				{
					Color newColor = originalColor;
					newColor.a = alpha;
					boxRenderer.material.color = newColor;
				}
				yield return null;
			}
			Destroy(gameObject);
		}
        
		void OnTriggerEnter(Collider other)
		{
			if (other.GetComponentInParent<Player>())
			{
				OnBoxHitPlayer?.Invoke(this);
				Destroy(gameObject);
			}
		}
        

	}
}
