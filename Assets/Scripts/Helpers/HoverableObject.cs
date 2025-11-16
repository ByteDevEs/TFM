using System.Collections;
using Mirror;
using UnityEngine;
namespace Helpers
{
    public class HoverableObject : NetworkBehaviour
    {
        [SerializeField] Material outlineHoverMaterial;
        [SerializeField] Material outlineClickMaterial;

        new MeshRenderer renderer;

        public bool IsHovering { get; private set; }
        public bool IsClicking { get; private set; }

        float clickTime;

        void Start()
        {
            renderer = GetComponent<MeshRenderer>();
        }

        public virtual void SetHoverEffect()
        {
            if (IsHovering)
            {
                return;
            }

            IsHovering = true;

            if (renderer.sharedMaterials.Length >= 1)
            {
                renderer.sharedMaterials = new[]
                {
                    renderer.sharedMaterials[0], outlineHoverMaterial
                };
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

            if (renderer.sharedMaterials.Length >= 1)
            {
                renderer.sharedMaterials = new[]
                {
                    renderer.sharedMaterials[0], outlineClickMaterial
                };
            }

            while (Time.time - clickTime < 0.2f)
            {
                yield return null;
            }

            IsClicking = false;
            RemoveHoverEffect();
        }

        public virtual void RemoveHoverEffect()
        {
            if (!IsHovering)
            {
                return;
            }

            IsHovering = false;
            renderer.sharedMaterials = new[]
            {
                renderer.sharedMaterials[0]
            };
        }
    }
}
