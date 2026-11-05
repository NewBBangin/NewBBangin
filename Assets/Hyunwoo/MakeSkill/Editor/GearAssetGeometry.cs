using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hyunwoo.MakeSkill.Editor
{
    /// <summary>Builds the original prototype's reusable, horizontal sixteen-tooth blade.</summary>
    public static class GearAssetGeometry
    {
        public const string AssetRoot = "Assets/Hyunwoo/MakeSkill/GearAssets";
        const string MeshFolder = AssetRoot + "/Meshes";
        const string MaterialFolder = AssetRoot + "/Materials";

        public static Material Steel => EnsureMaterial("Muted ivory steel", new Color(.77f, .78f, .69f));
        public static Material Ink => EnsureMaterial("Aubergine mechanism", new Color(.18f, .14f, .20f));
        public static Material Orange => EnsureMaterial("Hazard orange enamel", new Color(1f, .43f, .12f));
        public static Material Mint => EnsureMaterial("Mint upgrade detail", new Color(.28f, .88f, .68f));

        public static void EnsureAssets()
        {
            EnsureFolder(MeshFolder);
            EnsureFolder(MaterialFolder);
            EnsureBladeMesh();
            _ = Steel;
            _ = Ink;
            _ = Orange;
            _ = Mint;
            AssetDatabase.SaveAssets();
        }

        /// <summary>Returns an unsaved visual root; the pivot is the disc centre and local Y is its axle.</summary>
        public static GameObject BuildSingleGear()
        {
            EnsureAssets();
            var root = new GameObject("Gear");
            var blade = new GameObject("Blade");
            blade.transform.SetParent(root.transform, false);
            blade.AddComponent<MeshFilter>().sharedMesh = EnsureBladeMesh();
            blade.AddComponent<MeshRenderer>().sharedMaterial = Steel;

            Part("Dark blade inset", PrimitiveType.Cylinder, root.transform,
                new Vector3(0f, .055f, 0f), new Vector3(.84f, .025f, .84f), Ink);
            Part("Mint spindle", PrimitiveType.Cylinder, root.transform,
                new Vector3(0f, .10f, 0f), new Vector3(.29f, .06f, .29f), Mint);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * .5f;
                Part("Orange rotating blade bolt " + (i + 1), PrimitiveType.Cube, root.transform,
                    new Vector3(Mathf.Cos(angle) * .30f, .083f, Mathf.Sin(angle) * .30f),
                    Vector3.one * .075f, Orange);
            }
            return root;
        }

        public static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            if (slash < 0) throw new System.ArgumentException("Use an Assets-relative folder path.", nameof(path));
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }

        static Material EnsureMaterial(string name, Color color)
        {
            EnsureFolder(MaterialFolder);
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material) return material;
            Shader shader = Shader.Find("BBang/Retro Toon") ?? Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) throw new System.InvalidOperationException("No compatible gear shader is installed.");
            material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_ShadowColor")) material.SetColor("_ShadowColor", new Color(.43f, .31f, .47f));
            if (material.HasProperty("_ShadowStrength")) material.SetFloat("_ShadowStrength", .65f);
            if (material.HasProperty("_ShadowThreshold")) material.SetFloat("_ShadowThreshold", .3f);
            if (material.HasProperty("_TextureStrength")) material.SetFloat("_TextureStrength", 0f);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", Color.black);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static Mesh EnsureBladeMesh()
        {
            EnsureFolder(MeshFolder);
            string path = MeshFolder + "/Hooked 16-tooth gear.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh) return mesh;

            const int teeth = 16, segments = teeth * 4;
            var vertices = new List<Vector3>(segments * 12);
            var triangles = new List<int>(segments * 12);
            float[] radii = { .73f, .92f, .93f, .72f };
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                float b = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 va = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radii[i % 4];
                Vector3 vb = new Vector3(Mathf.Cos(b), 0f, Mathf.Sin(b)) * radii[(i + 1) % 4];
                Vector3 up = Vector3.up * .045f;
                AddTriangle(vertices, triangles, up, vb + up, va + up);
                AddTriangle(vertices, triangles, -up, va - up, vb - up);
                AddTriangle(vertices, triangles, va + up, vb + up, va - up);
                AddTriangle(vertices, triangles, vb + up, vb - up, va - up);
            }
            mesh = new Mesh { name = "Original hooked 16-tooth gear" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            int index = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
        }

        static void Part(string name, PrimitiveType shape, Transform parent, Vector3 position,
            Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(shape);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            var renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
        }
    }
}
