using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Hyunwoo.MakeSkill.Editor
{
    public static class HyunGearInstaller
    {
        public const string ScenePath = "Assets/Hyunwoo/MakeSkill/HyunScene.unity";
        public const string PrefabFolder = "Assets/Hyunwoo/MakeSkill/GearAssets/Prefabs";
        public const string SinglePath = PrefabFolder + "/Gear.prefab";
        public const string PairPath = PrefabFolder + "/CarGearPair.prefab";
        public const string Output = "Artifacts/HyunGear";
        const string PairName = "CarGearPair";

        [MenuItem("Tools/Hyunwoo/Gears/Create Assets and Place in HyunScene")]
        public static void Install()
        {
            RequireScene();
            GearAssetGeometry.EnsureAssets();
            GearAssetGeometry.EnsureFolder(PrefabFolder);
            var single = GearAssetGeometry.BuildSingleGear();
            single.AddComponent<GearRotor>();
            var singlePrefab = PrefabUtility.SaveAsPrefabAsset(single, SinglePath);
            Object.DestroyImmediate(single);

            var pair = new GameObject(PairName);
            for (int side = -1; side <= 1; side += 2)
            {
                var mount = new GameObject(side < 0 ? "Left Gear Mount" : "Right Gear Mount").transform;
                mount.SetParent(pair.transform, false);
                mount.localPosition = new Vector3(side * 1.70f, .48f, -.10f);
                Part("Chassis support arm", PrimitiveType.Cube, mount,
                    new Vector3(-side * .40f, -.15f, 0), new Vector3(.90f, .16f, .26f), GearAssetGeometry.Ink);
                Part("Chassis clamp", PrimitiveType.Cube, mount,
                    new Vector3(-side * .73f, .01f, 0), new Vector3(.18f, .38f, .32f), GearAssetGeometry.Ink);
                Part("Orange motor", PrimitiveType.Cylinder, mount,
                    new Vector3(0, -.12f, 0), new Vector3(.48f, .10f, .48f), GearAssetGeometry.Orange);
                var rotor = (GameObject)PrefabUtility.InstantiatePrefab(singlePrefab);
                rotor.name = side < 0 ? "Left Gear" : "Right Gear";
                rotor.transform.SetParent(mount, false);
                rotor.GetComponent<GearRotor>().degreesPerSecond = side * 1250f;
                PrefabUtility.RecordPrefabInstancePropertyModifications(rotor.GetComponent<GearRotor>());

                var hitbox = new GameObject("GrindHitbox", typeof(BoxCollider), typeof(GearGrinder), typeof(AudioSource));
                hitbox.transform.SetParent(mount, false);
                var trigger = hitbox.GetComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(1.86f, .3f, 1.86f);
                var sound = hitbox.GetComponent<AudioSource>();
                sound.playOnAwake = false;
                sound.spatialBlend = .3f;
                sound.volume = .25f;
                sound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Hyunwoo/PushObjects/Grind.wav");
                var grinder = new SerializedObject(hitbox.GetComponent<GearGrinder>());
                grinder.FindProperty("grindSound").objectReferenceValue = sound;
                grinder.ApplyModifiedPropertiesWithoutUndo();
            }
            var pairPrefab = PrefabUtility.SaveAsPrefabAsset(pair, PairPath);
            Object.DestroyImmediate(pair);

            var car = FindCar();
            var visual = car.Find("Visual");
            if (!visual) throw new InvalidOperationException("Car/Visual is missing.");
            var existing = car.Find(PairName);
            if (existing) Object.DestroyImmediate(existing.gameObject);
            var placed = (GameObject)PrefabUtility.InstantiatePrefab(pairPrefab, car);
            placed.name = PairName;
            placed.transform.localPosition = visual.localPosition;
            placed.transform.localRotation = visual.localRotation;
            placed.transform.localScale = visual.localScale;
            PrefabUtility.RecordPrefabInstancePropertyModifications(placed.transform);
            EditorSceneManager.MarkSceneDirty(car.gameObject.scene);
            EditorSceneManager.SaveScene(car.gameObject.scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = placed;
            SceneView.lastActiveSceneView?.Frame(new Bounds(visual.position + Vector3.up, new Vector3(6, 4, 7)), false);
            Validate();
            Capture();
            Debug.Log("HYUN_GEARS_INSTALLED: " + PairPath);
        }

        static void Part(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        static void RequireScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode first.");
            if (SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Open HyunScene first; other scenes are never replaced automatically.");
        }

        static Transform FindCar()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects().First(g => g.name == "Car").transform;
        }

        [MenuItem("Tools/Hyunwoo/Gears/Validate Assets and Placement")]
        public static void Validate()
        {
            var report = new StringBuilder();
            int passed = 0, failed = 0;
            void Check(string label, bool condition)
            {
                if (condition) passed++; else failed++;
                report.AppendLine((condition ? "PASS " : "FAIL ") + label);
            }
            var car = FindCar();
            var pair = car.Find(PairName);
            var visual = car.Find("Visual");
            Check("Single gear is a saved prefab", AssetDatabase.LoadAssetAtPath<GameObject>(SinglePath));
            Check("Pair is a saved prefab", AssetDatabase.LoadAssetAtPath<GameObject>(PairPath));
            Check("Pair follows Car", pair && pair.parent == car);
            Check("Pair aligned to offset Visual", pair && visual && Vector3.Distance(pair.localPosition, visual.localPosition) < .001f);
            var rotors = pair ? pair.GetComponentsInChildren<GearRotor>() : Array.Empty<GearRotor>();
            Check("Exactly two spinning gears", rotors.Length == 2);
            Check("Opposite rotation directions", rotors.Length == 2 && rotors[0].degreesPerSecond * rotors[1].degreesPerSecond < 0);
            var hitboxes = pair ? pair.GetComponentsInChildren<Collider>(true) : Array.Empty<Collider>();
            Check("Two grinding triggers", hitboxes.Length == 2 && hitboxes.All(c => c.isTrigger && c.GetComponent<GearGrinder>()));
            Check("All mesh references are persistent", pair && pair.GetComponentsInChildren<MeshFilter>().All(f => f.sharedMesh && AssetDatabase.Contains(f.sharedMesh)));
            Check("All materials have supported shaders", pair && pair.GetComponentsInChildren<MeshRenderer>().All(r => r.sharedMaterials.All(m => m && m.shader && m.shader.isSupported)));
            if (pair)
            {
                foreach (var rotor in rotors)
                {
                    var mesh = rotor.transform.Find("Blade")?.GetComponent<MeshFilter>()?.sharedMesh;
                    Check(rotor.name + " has a 16-tooth blade", mesh && Mathf.Abs(mesh.bounds.size.x - 1.86f) < .04f && mesh.vertexCount >= 256);
                    var bounds = new Bounds(rotor.transform.position, Vector3.zero);
                    foreach (var renderer in rotor.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(renderer.bounds);
                    Check(rotor.name + " clears ground", bounds.min.y > .05f);
                }
            }
            report.Insert(0, "HyunScene gear assets: " + passed + " passed, " + failed + " failed\n");
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "/validation.txt", report.ToString());
            if (failed > 0) throw new InvalidOperationException(report.ToString());
            Debug.Log(report.ToString());
        }

        [MenuItem("Tools/Hyunwoo/Gears/Capture Preview")]
        public static void Capture()
        {
            Directory.CreateDirectory(Output);
            var car = FindCar();
            var visual = car.Find("Visual");
            var center = visual.position;
            var preview = new GameObject("Temporary gear preview camera", typeof(Camera));
            var camera = preview.GetComponent<Camera>();
            camera.fieldOfView = 40;
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 200;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.14f, .17f, .20f);
            try
            {
                preview.transform.position = center + new Vector3(6.8f, 5.5f, 8.7f);
                preview.transform.LookAt(center + Vector3.up * 1.15f);
                Render(camera, Output + "/HyunScene-Gears.png");
                preview.transform.position = center + new Vector3(0, 8.5f, 7.8f);
                preview.transform.LookAt(center + Vector3.up * 1f);
                Render(camera, Output + "/HyunScene-Gears-Front.png");
                var pair = car.Find(PairName);
                preview.transform.position = pair.position + new Vector3(4.1f, 3.7f, .5f);
                preview.transform.LookAt(pair.position + new Vector3(1.70f, .48f, -.1f));
                Render(camera, Output + "/Gear-Detail.png");
                var main = Camera.main;
                if (main) Render(main, Output + "/HyunScene-Game.png");
            }
            finally { Object.DestroyImmediate(preview); }
        }

        static void Render(Camera camera, string path)
        {
            var active = RenderTexture.active;
            var target = RenderTexture.GetTemporary(1600, 1000, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
            try
            {
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
                if (!RenderPipeline.SupportsRenderRequest(camera, request))
                    throw new InvalidOperationException("URP render request is not ready.");
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(pixels);
            }
        }
    }

    [InitializeOnLoad]
    internal static class HyunGearCommands
    {
        const string Inbox = "Library/HyunGearCommand.txt";
        static double nextPoll;
        static bool checkingPlay;
        static double playStart;
        static Vector3 carStart;
        static Quaternion[] initialRotations;
        static HyunGearCommands()
        {
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (EditorApplication.isPlaying && SessionState.GetBool("HyunGear.PlayRequested", false))
            {
                if (!checkingPlay)
                {
                    var car = GameObject.Find("Car");
                    if (!car) return;
                    var rotors = car.GetComponentsInChildren<GearRotor>();
                    initialRotations = rotors.Select(r => r.transform.localRotation).ToArray();
                    carStart = car.transform.position;
                    playStart = EditorApplication.timeSinceStartup;
                    checkingPlay = true;
                }
                else if (EditorApplication.timeSinceStartup - playStart > .73)
                {
                    SessionState.SetBool("HyunGear.PlayRequested", false);
                    var car = GameObject.Find("Car");
                    var rotors = car.GetComponentsInChildren<GearRotor>();
                    bool spins = rotors.Length == 2 && initialRotations.Length == 2 && rotors.Select((r, i) => Quaternion.Angle(initialRotations[i], r.transform.localRotation) > 1).All(x => x);
                    bool follows = car.transform.Find("CarGearPair") && rotors.All(r => r.transform.IsChildOf(car.transform));
                    Directory.CreateDirectory(HyunGearInstaller.Output);
                    File.WriteAllText(HyunGearInstaller.Output + "/play-validation.txt", "Both blades rotate in Play Mode: " + spins + "\nGears remain children of moving Car: " + follows + "\nCar movement distance: " + Vector3.Distance(carStart, car.transform.position).ToString("F3") + "\n");
                    if (Camera.main) ScreenCapture.CaptureScreenshot(HyunGearInstaller.Output + "/HyunScene-Play.png");
                    checkingPlay = false;
                    EditorApplication.delayCall += () => EditorApplication.isPlaying = false;
                }
            }
            if (EditorApplication.timeSinceStartup < nextPoll || !File.Exists(Inbox)) return;
            nextPoll = EditorApplication.timeSinceStartup + .5;
            string command = File.ReadAllText(Inbox).Trim();
            File.Delete(Inbox);
            try
            {
                switch (command)
                {
                    case "install": HyunGearInstaller.Install(); break;
                    case "validate": HyunGearInstaller.Validate(); break;
                    case "capture": HyunGearInstaller.Capture(); break;
                    case "play-check":
                        SessionState.SetBool("HyunGear.PlayRequested", true);
                        EditorApplication.isPlaying = true;
                        break;
                    default: throw new InvalidOperationException("Unknown gear command: " + command);
                }
                Directory.CreateDirectory(HyunGearInstaller.Output);
                File.WriteAllText(HyunGearInstaller.Output + "/command-result.txt", command + " OK");
            }
            catch (Exception error)
            {
                Directory.CreateDirectory(HyunGearInstaller.Output);
                File.WriteAllText(HyunGearInstaller.Output + "/command-result.txt", command + " FAILED\n" + error);
                Debug.LogException(error);
            }
        }
    }
}
