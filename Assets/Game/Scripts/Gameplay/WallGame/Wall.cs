using System;
using System.Collections;
using System.Collections.Generic;
using Game.Utilities;
using UnityEngine;

namespace Game.Gameplay
{
    public class Wall : MonoBehaviour
    {
        void Update()
        {
            transform.position = transform.position.WithZ(transform.position.z - Time.time * 0.001f);
        }
    }
}
