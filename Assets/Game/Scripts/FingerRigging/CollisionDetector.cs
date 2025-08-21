using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FingerRigging
{
    public class CollisionDetector : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Finger"))
            {
                // 触发器进入时的逻辑
                Debug.Log("Finger entered the trigger: " + other.name);
            }
        }
    }
}
