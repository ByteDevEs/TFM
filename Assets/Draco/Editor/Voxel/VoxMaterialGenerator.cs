using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Draco.Editor.Voxel {
    internal static class VoxMaterialGenerator {
        // URP Property IDs
        private static readonly int baseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int baseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int smoothness = Shader.PropertyToID("_Smoothness");
        private static readonly int metallic = Shader.PropertyToID("_Metallic");
        private static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int emissionMap = Shader.PropertyToID("_EmissionMap");
        
        // URP Surface Options
        private static readonly int surface = Shader.PropertyToID("_Surface"); // 0: Opaque, 1: Transparent
        private static readonly int blend = Shader.PropertyToID("_Blend");     // 0: Alpha, 1: Premultiply, 2: Additive, 3: Multiply
        private static readonly int srcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int dstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int zWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int cull = Shader.PropertyToID("_Cull"); // 0: Off (Double Sided), 2: Back (Standard)

        private const string URP_LIT_SHADER = "Universal Render Pipeline/Lit";

        public static Material CreateMaterialFor(int id, MaterialData materialData, Texture palette) {
            var type = materialData?.MaterialType ?? MaterialType.Diffuse;
            return type switch {
                MaterialType.Diffuse => CreateDiffuseMaterial(palette),
                MaterialType.Emission => CreateEmissiveMaterial(id, materialData, palette),
                MaterialType.Glass => CreateGlassMaterial(id, materialData, palette),
                MaterialType.Metal => CreateMetalMaterial(id, materialData, palette),
                _ => throw new ArgumentOutOfRangeException($"Encountered unsupported material type \"{type}\" when attempting to generate material.")
            };
        }

        private static Material CreateBaseMaterial(string name, Texture palette) {
            var mat = new Material(Shader.Find(URP_LIT_SHADER)) {
                name = name,
                enableInstancing = true,
                doubleSidedGI = true 
            };
            
            // Set Base Texture
            mat.SetTexture(baseMap, palette);
            mat.SetColor(baseColor, Color.white);
            
            // Default to double-sided rendering (Cull Off)
            mat.SetFloat(cull, (float)CullMode.Off); 

            return mat;
        }

        private static Material CreateDiffuseMaterial(Texture palette) {
            var mat = CreateBaseMaterial("Default Material", palette);
            
            // Diffuse has no smoothness or metallic
            mat.SetFloat(smoothness, 0);
            mat.SetFloat(metallic, 0);
            mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mat.SetFloat("_SpecularHighlights", 0.0f); // URP equivalent to disable spec highlights

            return mat;
        }

        private static Material CreateEmissiveMaterial(int id, MaterialData materialData, Texture palette) {
            var mat = CreateBaseMaterial($"Emissive Material ({id})", palette);

            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            
            mat.SetFloat(metallic, 0.0f); // Usually emissive voxels aren't metallic
            mat.SetFloat(smoothness, 0);
            
            mat.SetTexture(emissionMap, palette);
            
            // Calculate emission intensity
            // MagicaVoxel flux is roughly power, we scale it for Unity URP
            var intensity = materialData.Intensity > 0 ? materialData.Intensity : 1.0f;
            // Use HDR color for emission
            mat.SetColor(emissionColor, Color.white * intensity);

            return mat;
        }

        private static Material CreateGlassMaterial(int id, MaterialData materialData, Texture palette) {
            var mat = CreateBaseMaterial($"Glass Material ({id})", palette);

            // Set URP Surface Type to Transparent
            mat.SetFloat(surface, 1.0f); 
            mat.SetFloat(blend, 0.0f); // Alpha blending
            
            mat.SetInt(srcBlend, (int)BlendMode.SrcAlpha);
            mat.SetInt(dstBlend, (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt(zWrite, 0); // Generally glass doesn't write to Z-buffer in Unity transparent queue
            
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;

            // Apply properties
            mat.SetFloat(smoothness, materialData.Smoothness);
            // Alpha controls transparency in URP Lit
            mat.SetColor(baseColor, new Color(1, 1, 1, 1 - materialData.Transparency));

            return mat;
        }

        private static Material CreateMetalMaterial(int id, MaterialData materialData, Texture palette) {
            var mat = CreateBaseMaterial($"Metal Material ({id})", palette);

            mat.SetFloat(smoothness, materialData.Smoothness);
            mat.SetFloat(metallic, materialData.Metallic);

            return mat;
        }
    }
}