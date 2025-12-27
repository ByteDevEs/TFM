using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;
namespace Helpers
{
    public class HoverableObject : NetworkBehaviour
    {
        protected GameObject[] Children;

        protected bool isHovering;
        bool isClicking;

        float clickTime;


        protected virtual void Start()
        {
            Children = transform.GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray();
        }

        public virtual void SetHoverEffect()
        {
            if (isHovering)
            {
                return;
            }

            isHovering = true;

            foreach (var child in Children)
            {
                child.layer = LayerMask.NameToLayer("Hovered");
            }
        }

        public virtual void SetClickEffect()
        {
            StartCoroutine(SetClickEffectCoroutine());
        }

        IEnumerator SetClickEffectCoroutine()
        {
            clickTime = Time.time;

            if (!isHovering || isClicking)
            {
                yield break;
            }

            isClicking = true;

            foreach (var child in Children)
            {
                child.layer = LayerMask.NameToLayer("Clicked");
            }

            while (Time.time - clickTime < 0.2f)
            {
                yield return null;
            }

            isClicking = false;
            RemoveEffect();
        }

        public virtual void RemoveEffect()
        {
            if (!isHovering)
            {
                return;
            }

            isHovering = false;
            foreach (var child in Children)
            {
                child.layer = 0;
            }
        }
    }
}
