//使用UTF-8
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public static class AnimatorExtension
    {
        public static bool HasParameter(this Animator animator, string paramName)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            };
            return false;
        }

        public static void SetExistedBool(this Animator animator, string paramName, bool value)
        {
            if (animator.HasParameter(paramName))
            {
                animator.SetBool(paramName, value);
            }
        }

        public static void SetExistedTrigger(this Animator animator, string paramName)
        {
            if (animator.HasParameter(paramName))
            {
                animator.SetTrigger(paramName);
            }
        }
    }
}