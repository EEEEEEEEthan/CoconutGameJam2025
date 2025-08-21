using UnityEngine;
namespace Game.Gameplay.DodgeGame
{
	public class DodgeBox : MonoBehaviour
	{
		public static System.Action<DodgeBox> OnBoxHitPlayer;
		public static System.Action<DodgeBox> OnBoxDodged;
		[Header("飞行配置")] public float speed = 10f;
		public Vector3 targetPosition;
		[Header("碰撞检测")] public LayerMask playerLayer;
		bool hasPassedTarget;
		bool isDissolving;
		Vector3 flyDirection;
		Renderer boxRenderer;
		Rigidbody boxRigidbody;
		MeshRenderer meshRenderer;
		void Awake()
		{
			meshRenderer = GetComponentInChildren<MeshRenderer>();
			meshRenderer.sharedMaterial = new(meshRenderer.sharedMaterial);
		}
		void Start()
		{
			flyDirection = (targetPosition - transform.position).normalized;
			if (boxRigidbody != null) boxRigidbody.velocity = flyDirection * speed;
		}
		void Update()
		{
			if (!hasPassedTarget && transform.position.z < 0)
			{
				hasPassedTarget = true;
				OnBoxDodged?.Invoke(this);
			}
			if (boxRigidbody == null) transform.position += flyDirection * speed * Time.deltaTime;
		}
		void OnTriggerEnter(Collider other)
		{
			if (other.GetComponentInParent<Player>())
			{
				OnBoxHitPlayer?.Invoke(this);
				Destroy(gameObject);
			}
		}
		public void Initialize(Vector3 startPos, Vector3 targetPos, float flySpeed)
		{
			transform.position = startPos;
			targetPosition = targetPos;
			speed = flySpeed;
			hasPassedTarget = false;
			isDissolving = false;
		}
	}
}
