using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BBang.RetroToon.Editor
{
    public static class RetroDemoBuilder
    {
        public const string Root = "Assets/RetroToon";
        public const string ScenePath = Root + "/Scenes/RetroBurgerStreet.unity";
        public const string Output = "Artifacts/RetroToon";

        [MenuItem("Tools/BBang/Build Retro Burger Street")]
        public static void Build()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path == ScenePath)
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Output + "/OriginalSettings");
            foreach (var settings in new[] { "GraphicsSettings.asset", "QualitySettings.asset", "EditorBuildSettings.asset" })
                if (!File.Exists(Output + "/OriginalSettings/" + settings))
                    File.Copy("ProjectSettings/" + settings, Output + "/OriginalSettings/" + settings);
            Directory.CreateDirectory(Root + "/Settings");
            Directory.CreateDirectory(Root + "/Scenes");
            AssetDatabase.Refresh();
            // NewScene unloads unused assets; create/load the profile afterwards.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var surface = Shader.Find("BBang/Retro Toon");
            var printShader = Shader.Find("Hidden/BBang/Retro Print");
            if (!surface || !printShader) throw new InvalidOperationException("Retro shaders have not imported.");
            var profile = GetOrCreate<RetroStyleProfile>(Root + "/Settings/RetroStyle.asset");
            var print = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Settings/RetroPrint.mat");
            if (!print) { print = new Material(printShader); AssetDatabase.CreateAsset(print, Root + "/Settings/RetroPrint.mat"); }
            profile.Apply(print, true);
            var sky = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Settings/RetroSunset.mat");
            if (!sky) { sky = new Material(Shader.Find("BBang/Retro Sunset Sky")); AssetDatabase.CreateAsset(sky, Root + "/Settings/RetroSunset.mat"); }

            var renderer = GetOrCreate<UniversalRendererData>(Root + "/Settings/RetroRenderer.asset");
            var rendererSerialized = new SerializedObject(renderer);
            rendererSerialized.FindProperty("m_RenderingMode").intValue = 0;
            rendererSerialized.ApplyModifiedPropertiesWithoutUndo();
            var feature = renderer.rendererFeatures.OfType<FullScreenPassRendererFeature>().FirstOrDefault();
            if (!feature)
            {
                feature = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
                feature.name = "Retro Ink + Pixel Print";
                AssetDatabase.AddObjectToAsset(feature, renderer);
                renderer.rendererFeatures.Add(feature);
            }
            feature.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.AfterRenderingPostProcessing;
            feature.requirements = ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal;
            feature.fetchColorBuffer = true;
            feature.passMaterial = print;
            feature.passIndex = 0;
            feature.SetActive(true);
            renderer.SetDirty();
            EditorUtility.SetDirty(feature);
            EditorUtility.SetDirty(renderer);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root + "/Settings/RetroPipeline.asset");
            if (!pipeline) { pipeline = UniversalRenderPipelineAsset.Create(renderer); AssetDatabase.CreateAsset(pipeline, Root + "/Settings/RetroPipeline.asset"); }
            pipeline.supportsHDR = false;
            pipeline.msaaSampleCount = 1;
            pipeline.renderScale = 1;
            pipeline.supportsCameraDepthTexture = true;
            pipeline.shadowDistance = 75;
            pipeline.shadowCascadeCount = 2;
            pipeline.mainLightShadowmapResolution = 2048;
            EditorUtility.SetDirty(pipeline);
            GraphicsSettings.defaultRenderPipeline = pipeline;
            // Use the same deliberately configured pipeline at either quality tier.
            int currentQuality = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(currentQuality, false);

            var geometry = RetroStreetGeometry.Build();
            RetroTruckInstaller.CreatePlacedTruck();
            var lightObject = new GameObject("Sun - warm afternoon / hard cel shadows");
            var sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Hard;
            sun.shadowStrength = 0.85f;
            sun.shadowBias = 0.05f;
            sun.shadowNormalBias = 0.35f;
            lightObject.transform.rotation = Quaternion.Euler(48, -35, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.48f, 0.52f);
            RenderSettings.skybox = sky;

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.15f;
            camera.farClipPlane = 150;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            var additional = camera.GetUniversalAdditionalCameraData();
            additional.renderPostProcessing = true;
            additional.antialiasing = AntialiasingMode.None;
            additional.requiresDepthTexture = true;
            var controller = new GameObject("Art Direction - select profile to tune the look").AddComponent<RetroDemoController>();
            controller.profile = profile;
            controller.printMaterial = print;
            controller.sun = sun;
            controller.viewCamera = camera;
            controller.SetView(0);
            controller.ApplyStyle();
            EditorUtility.SetDirty(print);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var existingScenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            existingScenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = existingScenes.ToArray();
            AssetDatabase.SaveAssets();
            Selection.activeObject = controller;
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.LookAt(controller.orbitTarget, camera.transform.rotation, 20);
            File.WriteAllText(Output + "/build-result.txt", "Scene generated: " + ScenePath + "\nRenderers: " + geometry.GetComponentsInChildren<Renderer>().Length);
            Debug.Log("RETRO_DEMO_BUILD_OK: " + ScenePath);
        }

        private static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var result = AssetDatabase.LoadAssetAtPath<T>(path);
            if (!result) { result = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(result, path); }
            return result;
        }

        [MenuItem("Tools/BBang/Open Retro Burger Street")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Tools/BBang/Capture Art Preview")]
        public static void Capture()
        {
            Directory.CreateDirectory(Output);
            var controller = UnityEngine.Object.FindFirstObjectByType<RetroDemoController>();
            if (!controller || !controller.viewCamera) throw new InvalidOperationException("Open RetroBurgerStreet first.");
            var camera = controller.viewCamera;
            var position = camera.transform.position;
            var rotation = camera.transform.rotation;
            var orbit = controller.orbitTarget;
            float fov = camera.fieldOfView;
            bool enabled = controller.printEnabled;
            try
            {
                controller.printEnabled = true;
                for (int i = 0; i < 3; i++)
                {
                    controller.SetView(i);
                    controller.ApplyStyle();
                    Render(camera, Output + "/RetroStreet-" + (i + 1) + ".png");
                }
                controller.SetView(0);
                controller.printEnabled = false;
                controller.ApplyStyle();
                Render(camera, Output + "/RetroStreet-PrintOff.png");
            }
            finally
            {
                camera.transform.SetPositionAndRotation(position, rotation);
                camera.fieldOfView = fov;
                controller.orbitTarget = orbit;
                controller.printEnabled = enabled;
                controller.ApplyStyle();
            }
            Validate();
        }

        internal static void Render(Camera camera, string path)
        {
            var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            rt.Create();
            var previous = RenderTexture.active;
            var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            try
            {
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = rt };
                if (!RenderPipeline.SupportsRenderRequest(camera, request)) throw new InvalidOperationException("URP render request is not ready.");
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = rt;
                texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(texture); }
        }

        [MenuItem("Tools/BBang/Validate Retro Scene")]
        public static void Validate()
        {
            Directory.CreateDirectory(Output);
            var report = new StringBuilder();
            report.AppendLine("Unity " + Application.unityVersion + "; graphics " + SystemInfo.graphicsDeviceType);
            report.AppendLine("Play mode: " + Application.isPlaying);
            int errors = 0;
            var controller = UnityEngine.Object.FindFirstObjectByType<RetroDemoController>();
            if (!controller || !controller.profile || !controller.printMaterial || !controller.viewCamera || !controller.sun)
            { errors++; report.AppendLine("ERROR: art-direction references missing or unloaded."); }
            if (controller) report.AppendLine("Print enabled: " + controller.printEnabled + "; HUD: " + controller.showHUD);
            report.AppendLine("Fog enabled: " + RenderSettings.fog + "; color: " + RenderSettings.fogColor);
            foreach (var shaderName in new[] { "BBang/Retro Toon", "Hidden/BBang/Retro Print", "BBang/Retro Sunset Sky", "BBang/Retro World Text" })
            {
                var shader = Shader.Find(shaderName);
                report.AppendLine(shaderName + ": found=" + (shader != null) + ", supported=" + (shader && shader.isSupported));
                if (!shader || !shader.isSupported) errors++;
                if (shader) foreach (var message in ShaderUtil.GetShaderMessages(shader))
                {
                    report.AppendLine(message.severity + ": " + message.message + " at " + message.file + ":" + message.line);
                    if (message.severity.ToString() == "Error") errors++;
                }
            }
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    errors += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            report.AppendLine("Scene renderers: " + renderers.Length);
            foreach (var renderer in renderers) foreach (var material in renderer.sharedMaterials)
                if (!material || !material.shader) { errors++; report.AppendLine("Missing material/shader: " + renderer.name); }
            report.AppendLine("Errors: " + errors);
            File.WriteAllText(Output + "/validation.txt", report.ToString());
            if (errors > 0) throw new InvalidOperationException(report.ToString());
            Debug.Log("RETRO_VALIDATION_OK\n" + report);
        }

        [MenuItem("Tools/BBang/Export Reusable Unity Package")]
        public static void Export()
        {
            Directory.CreateDirectory(Output);
            AssetDatabase.ExportPackage(Root, Output + "/BBang-RetroToon.unitypackage", ExportPackageOptions.Recurse);
        }
    }

    // A local command inbox for repeatable editor verification. It accepts only
    // the fixed actions below, and never evaluates source code or arbitrary paths.
    [InitializeOnLoad]
    internal static class RetroEditorCommands
    {
        private const string Inbox = "Library/RetroToonCommand.txt";
        private static double nextPoll;
        static RetroEditorCommands() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            if (EditorApplication.timeSinceStartup < nextPoll || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            nextPoll = EditorApplication.timeSinceStartup + 0.5;
            if (!File.Exists(Inbox)) return;
            string command = File.ReadAllText(Inbox).Trim();
            File.Delete(Inbox);
            try
            {
                switch (command)
                {
                    case "refresh": AssetDatabase.Refresh(); break;
                    case "build": RetroDemoBuilder.Build(); break;
                    case "capture": RetroDemoBuilder.Capture(); break;
                    case "truck": RetroTruckInstaller.AddToStreet(); break;
                    case "refresh-truck": RetroTruckInstaller.RefreshTruck(); break;
                    case "capture-truck": RetroTruckInstaller.CaptureTruck(); break;
                    case "validate": RetroDemoBuilder.Validate(); break;
                    case "export": RetroDemoBuilder.Export(); break;
                    case "play": EditorApplication.isPlaying = true; break;
                    case "stop": EditorApplication.isPlaying = false; break;
                    case "save":
                        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path == RetroDemoBuilder.ScenePath)
                            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                        AssetDatabase.SaveAssets();
                        break;
                    default: throw new InvalidOperationException("Unknown retro command: " + command);
                }
                Directory.CreateDirectory(RetroDemoBuilder.Output);
                File.WriteAllText(RetroDemoBuilder.Output + "/command-result.txt", command + " OK " + DateTime.UtcNow.ToString("O"));
            }
            catch (Exception exception)
            {
                Directory.CreateDirectory(RetroDemoBuilder.Output);
                File.WriteAllText(RetroDemoBuilder.Output + "/command-result.txt", command + " FAILED\n" + exception);
                Debug.LogException(exception);
            }
        }
    }
}
