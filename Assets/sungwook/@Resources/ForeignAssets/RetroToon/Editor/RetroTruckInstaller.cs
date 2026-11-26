using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBang.RetroToon.Editor
{
    public static class RetroTruckInstaller
    {
        public const string TruckName = "BBANG Delivery Truck";

        public static GameObject CreatePlacedTruck()
        {
            var truck = RetroTruckGeometry.Build();
            truck.name = TruckName;
            truck.transform.SetPositionAndRotation(new Vector3(-6.3f, 0.04f, -6.0f), Quaternion.Euler(0, 90, 0));
            if (PrefabUtility.IsPartOfPrefabInstance(truck))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(truck);
                PrefabUtility.RecordPrefabInstancePropertyModifications(truck.transform);
            }
            return truck;
        }

        [MenuItem("Tools/BBang/Add Delivery Truck to Street")]
        public static void AddToStreet()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play mode before placing the persistent truck.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != RetroDemoBuilder.ScenePath)
                throw new InvalidOperationException("Open RetroBurgerStreet before adding the truck.");
            var existing = GameObject.Find(TruckName);
            if (existing) { Selection.activeGameObject = existing; return; }
            Directory.CreateDirectory(RetroDemoBuilder.Output);
            string backup = RetroDemoBuilder.Output + "/RetroBurgerStreet-before-truck.unity";
            if (!File.Exists(backup)) EditorSceneManager.SaveScene(scene, backup, true);
            var truck = CreatePlacedTruck();
            Undo.RegisterCreatedObjectUndo(truck, "Add BBang delivery truck");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = truck;
            var renderers = truck.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            File.WriteAllText(RetroDemoBuilder.Output + "/truck-result.txt",
                "Truck placed in " + scene.path + "\nPosition: " + truck.transform.position
                + "\nRenderers: " + renderers.Length + "\nWorld bounds: " + bounds);
            Debug.Log("RETRO_TRUCK_OK: " + truck.name);
        }

        [MenuItem("Tools/BBang/Capture Delivery Truck")]
        public static void CaptureTruck()
        {
            var truck = GameObject.Find(TruckName);
            var controller = UnityEngine.Object.FindFirstObjectByType<RetroDemoController>();
            if (!truck || !controller || !controller.viewCamera)
                throw new InvalidOperationException("Open the street scene with its delivery truck first.");
            var camera = controller.viewCamera;
            Vector3 position = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            Vector3 orbit = controller.orbitTarget;
            float fov = camera.fieldOfView;
            bool print = controller.printEnabled;
            try
            {
                controller.printEnabled = true;
                controller.ApplyStyle();
                controller.SetView(0);
                RetroDemoBuilder.Render(camera, RetroDemoBuilder.Output + "/Truck-Street.png");
                Vector3 target = truck.transform.position + Vector3.up * 1.5f;
                camera.fieldOfView = 46;
                camera.transform.position = truck.transform.position + new Vector3(5.8f, 3.6f, -6.7f);
                camera.transform.LookAt(target);
                RetroDemoBuilder.Render(camera, RetroDemoBuilder.Output + "/Truck-Front.png");
                camera.transform.position = truck.transform.position + new Vector3(-5.7f, 3.4f, -6.5f);
                camera.transform.LookAt(target);
                RetroDemoBuilder.Render(camera, RetroDemoBuilder.Output + "/Truck-Rear.png");
            }
            finally
            {
                camera.transform.SetPositionAndRotation(position, rotation);
                camera.fieldOfView = fov;
                controller.orbitTarget = orbit;
                controller.printEnabled = print;
                controller.ApplyStyle();
            }
            RetroDemoBuilder.Validate();
        }

        [MenuItem("Tools/BBang/Refresh Delivery Truck Prefab")]
        public static void RefreshTruck()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play mode before refreshing the truck.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != RetroDemoBuilder.ScenePath)
                throw new InvalidOperationException("Open RetroBurgerStreet first.");
            // Updating the shared prefab preserves the placed instance's transform.
            var temporary = RetroTruckGeometry.Build();
            UnityEngine.Object.DestroyImmediate(temporary);
            var textMaterial = RetroTruckGeometry.GetDepthFontMaterial();
            foreach (var root in scene.GetRootGameObjects())
                foreach (var text in root.GetComponentsInChildren<TextMesh>(true))
                {
                    var renderer = text.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = textMaterial;
                    if (!text.GetComponent<RetroWorldText>()) text.gameObject.AddComponent<RetroWorldText>();
                    EditorUtility.SetDirty(renderer);
                    if (PrefabUtility.IsPartOfPrefabInstance(renderer))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
