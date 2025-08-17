using System.Collections;
using UnityEngine;
using Game.Gameplay;

namespace Game.Gameplay.DodgeGame
{
    /// <summary>
    /// 躲避Box组件，包含飞行逻辑、轨迹计算和溶解效果
    /// </summary>
    public class DodgeBox : MonoBehaviour
    {
        [Header("飞行配置")]
        public float speed = 10f;              // 飞行速度
        public Vector3 targetPosition;         // 目标位置
        
        [Header("溶解效果")]
        public float dissolveTime = 2f;        // 溶解持续时间
        public Material dissolveMaterial;      // 溶解材质
        
        [Header("碰撞检测")]
        public LayerMask playerLayer;          // 玩家层级掩码
        
        private bool hasPassedTarget = false;  // 是否已穿过目标区域
        private bool isDissolving = false;     // 是否正在溶解
        private Vector3 flyDirection;          // 飞行方向
        private Renderer boxRenderer;          // 渲染器组件
        private Material originalMaterial;     // 原始材质
        private Rigidbody boxRigidbody;       // 刚体组件
        
        // 事件：Box与玩家碰撞
        public static System.Action<DodgeBox> OnBoxHitPlayer;
        // 事件：Box成功被躲避（穿过z=0平面）
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
            // 计算飞行方向
            flyDirection = (targetPosition - transform.position).normalized;
            
            // 设置刚体速度
            if (boxRigidbody != null)
            {
                boxRigidbody.velocity = flyDirection * speed;
            }
        }
        
        void Update()
        {
            // 检查是否穿过z=0平面
            if (!hasPassedTarget && transform.position.z > 0)
            {
                hasPassedTarget = true;
                OnBoxDodged?.Invoke(this);
                StartDissolve();
            }
            
            // 如果没有刚体，手动移动
            if (boxRigidbody == null)
            {
                transform.position += flyDirection * speed * Time.deltaTime;
            }
        }
        
        /// <summary>
        /// 初始化Box的飞行参数
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="flySpeed">飞行速度</param>
        public void Initialize(Vector3 startPos, Vector3 targetPos, float flySpeed)
        {
            transform.position = startPos;
            targetPosition = targetPos;
            speed = flySpeed;
            
            // 重置状态
            hasPassedTarget = false;
            isDissolving = false;
            
            // 恢复原始材质
            if (boxRenderer != null && originalMaterial != null)
            {
                boxRenderer.material = originalMaterial;
            }
        }
        
        /// <summary>
        /// 开始溶解效果
        /// </summary>
        void StartDissolve()
        {
            if (isDissolving) return;
            
            isDissolving = true;
            StartCoroutine(DissolveCoroutine());
        }
        
        /// <summary>
        /// 溶解效果协程
        /// </summary>
        IEnumerator DissolveCoroutine()
        {
            float elapsedTime = 0f;
            Color originalColor = Color.white;
            
            if (boxRenderer != null)
            {
                // 如果有溶解材质，使用溶解材质
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
            
            // 溶解完成，销毁对象
            Destroy(gameObject);
        }
        
        /// <summary>
        /// 碰撞检测
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            // 检查是否与玩家碰撞
            if (((1 << other.gameObject.layer) & playerLayer) != 0)
            {
                // 触发碰撞事件
                OnBoxHitPlayer?.Invoke(this);
                
                // 立即销毁Box
                Destroy(gameObject);
            }
        }
        
        void OnDrawGizmos()
        {
            // 绘制飞行轨迹
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPosition);
            
            // 绘制目标位置
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
    }
}