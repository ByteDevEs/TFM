using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;
namespace Helpers
{
    public class HoverableObject : NetworkBehaviour
    {
        GameObject[] children;
        
        public bool IsHovering { get; private set; }
        public bool IsClicking { get; private set; }

        float clickTime;


        protected void Start()
        {
            children = transform.GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray();
        }

        public virtual void SetHoverEffect()
        {
            if (IsHovering)
            {
                return;
            }

            IsHovering = true;

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

            if (!IsHovering || IsClicking)
            {
                yield break;
            }

            IsClicking = true;

            foreach (var child in children)
            {
                child.layer = LayerMask.NameToLayer("Clicked");
            }

            while (Time.time - clickTime < 0.2f)
            {
                yield return null;
            }

            IsClicking = false;
            RemoveEffect();
        }

        public virtual void RemoveEffect()
        {
            if (!IsHovering)
            {
                return;
            }

            IsHovering = false;
            foreach (var child in children)
            {
                child.layer = 0;
            }
        }
    }
}
