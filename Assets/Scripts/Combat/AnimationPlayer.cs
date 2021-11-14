using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationPlayer : MonoBehaviour
{
    public const string SLASH = "Slash", ARROW_FOCUS = "ArrowFocus",
        SPEAR_THRUST = "SpearThrust", SWORD_SLASH = "SwordSlash", ARROW_FIRE = "ArrowFire";
    public Dictionary<int, AnimationClip> Animations = new Dictionary<int, AnimationClip>();
    //public Dictionary<int, AnimationClip> animations = new Dictionary<int, string>();
    public Animator Animator;

    public void Play(string ID)
    {
        Animator.SetTrigger(ID);
    }
}
