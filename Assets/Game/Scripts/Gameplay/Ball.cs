using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    public class Ball : GameBehaviour
    {
        [SerializeField] float greetRadius = 0.1f; // 范围
        [SerializeField] float upVelocity = 0.1f; // 向上速度
        Rigidbody _rb;
        void Awake() => _rb = GetComponent<Rigidbody>();
        void OnEnable()
        {
            if (GameRoot && GameRoot.Player != null)
                GameRoot.Player.OnEmotionTriggered += OnPlayerEmotion;
        }
        void OnDisable()
        {
            if (GameRoot && GameRoot.Player != null)
                GameRoot.Player.OnEmotionTriggered -= OnPlayerEmotion;
        }
        void OnPlayerEmotion(Player.EmotionCode emotion)
        {
            if (emotion != Player.EmotionCode.Hi) return; // 只响应打招呼
            if (!GameRoot || GameRoot.Player == null) return;
            // 距离检测
            if ((transform.position - GameRoot.Player.transform.position).sqrMagnitude <= greetRadius * greetRadius)
            {
                if (_rb == null) _rb = GetComponent<Rigidbody>();
                if (_rb != null)
                    _rb.velocity = Vector3.up * upVelocity;
            }
        }
    }
}
