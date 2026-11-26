using UnityEngine;

namespace BBang.RetroToon
{
    [CreateAssetMenu(menuName = "BBang/Retro Style Profile", fileName = "RetroStyle")]
    public sealed class RetroStyleProfile : ScriptableObject
    {
        [Header("Print / pixels")]
        [Range(180, 1080)] public int pixelHeight = 360;
        [Range(0, 3)] public float outlineThickness = 0.8f;
        public Color outlineColor = new Color(0.12f, 0.075f, 0.14f, 1);
        [Range(0.001f, 1)] public float depthThreshold = 0.035f;
        [Range(0.01f, 0.95f)] public float normalThreshold = 0.5f;
        [Range(0, 1)] public float ditherStrength = 0.3f;
        [Range(8, 128)] public int colorSteps = 32;
        [Range(0, 0.1f)] public float grainStrength = 0.012f;
        [Range(0, 2)] public float saturation = 1.05f;
        [Range(0, 1)] public float warmth = 0.12f;
        [Range(0, 1)] public float vignetteStrength = 0.12f;

        [Header("Atmosphere")]
        public Color fogColor = new Color(0.58f, 0.60f, 0.40f);
        [Min(0)] public float fogStart = 20;
        [Min(1)] public float fogEnd = 85;
        public Color sunColor = new Color(1, 0.95f, 0.87f);
        [Range(0, 3)] public float sunIntensity = 1;

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
#endif

        public void Apply(Material material, bool enabled)
        {
            if (!material) return;
            material.SetFloat("_EffectEnabled", enabled ? 1 : 0);
            material.SetFloat("_PixelHeight", pixelHeight);
            material.SetFloat("_OutlineThickness", outlineThickness);
            material.SetColor("_OutlineColor", outlineColor);
            material.SetFloat("_DepthThreshold", depthThreshold);
            material.SetFloat("_NormalThreshold", Mathf.Clamp(normalThreshold, 0.01f, 0.95f));
            material.SetFloat("_DitherStrength", ditherStrength);
            material.SetFloat("_ColorSteps", colorSteps);
            material.SetFloat("_GrainStrength", grainStrength);
            material.SetFloat("_Saturation", saturation);
            material.SetFloat("_Warmth", warmth);
            material.SetFloat("_VignetteStrength", vignetteStrength);
        }
    }
}
