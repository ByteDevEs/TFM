using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;
namespace Helpers
{
    public class HoverableObject : NetworkBehaviour
    {
        GameObject[] children;

        bool isHovering;
        bool isClicking;

        float clickTime;


        protected void Start()
        {
            children = transform.GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray();
        }

        public virtual void SetHoverEffect()
        {
            if (isHovering)
            {
                return;
            }

            isHovering = true;

            foreach (var child in children)
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

            foreach (var child in children)
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
            foreach (var child in children)
            {
                child.layer = 0;
            }
        }
    }
}
