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
            if (animator == null || animator.parameters == null) return false;
            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].name == paramName)
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

        public static void SetExistedFloat(this Animator animator, string paramName, float value)
        {
            if (animator.HasParameter(paramName))
            {
                animator.SetFloat(paramName, value);
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