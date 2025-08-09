using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    public class PlayerAnimationEvent : GameBehaviour
    {
        public void AnimEvt_SpecialAnimEnd()
        {
            GameRoot.Player.SetSpecialAnimEnd();
        }
    }
}
