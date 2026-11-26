using UnityEngine;

namespace BBang.RetroToon
{
    /// <summary>Keeps a depth-tested TextMesh material bound to Unity's live dynamic font atlas.</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMesh), typeof(MeshRenderer))]
    public sealed class RetroWorldText : MonoBehaviour
    {
        static readonly int MainTex = Shader.PropertyToID("_MainTex");
        TextMesh textMesh;
        MeshRenderer textRenderer;
        MaterialPropertyBlock properties;
        Texture previousAtlas;

        void OnEnable()
        {
            Font.textureRebuilt += OnFontAtlasRebuilt;
            RefreshAtlas();
        }

        void OnDisable() => Font.textureRebuilt -= OnFontAtlasRebuilt;

        void LateUpdate() => SynchronizeAtlas();

        void OnWillRenderObject() => SynchronizeAtlas();

        void OnFontAtlasRebuilt(Font rebuiltFont)
        {
            if (!textMesh) textMesh = GetComponent<TextMesh>();
            if (textMesh && textMesh.font == rebuiltFont) RefreshAtlas();
        }

        public void RefreshAtlas()
        {
            previousAtlas = null;
            SynchronizeAtlas();
        }

        void SynchronizeAtlas()
        {
            if (!textMesh) textMesh = GetComponent<TextMesh>();
            if (!textRenderer) textRenderer = GetComponent<MeshRenderer>();
            if (!textMesh || !textRenderer || !textMesh.font || !textMesh.font.material) return;
            Texture atlas = textMesh.font.material.mainTexture;
            if (!atlas || atlas == previousAtlas) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            textRenderer.GetPropertyBlock(properties);
            properties.SetTexture(MainTex, atlas);
            textRenderer.SetPropertyBlock(properties);
            previousAtlas = atlas;
        }
    }
}
