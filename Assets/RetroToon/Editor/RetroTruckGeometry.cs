using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace BBang.RetroToon.Editor
{
    /// <summary>Builds an original, editable cab-over delivery truck. Front is +Z; tire contact is Y = 0.</summary>
    public static class RetroTruckGeometry
    {
        public const string PrefabPath = "Assets/RetroToon/Prefabs/BBangDeliveryTruck.prefab";
        const string AssetFolder = "Assets/RetroToon/Generated/Truck";
        static Transform root;
        static Mesh cylinder, sphere;
        static Font font;
        static Material mint, cream, ivory, cherry, orange, ink, glass, rubber, steel, gold, lettuce;
        const float AxleY = .525f;

        public static GameObject Build()
        {
            EnsureFolder(AssetFolder);
            EnsureFolder("Assets/RetroToon/Prefabs");
            root = new GameObject("BBANG Delivery Truck").transform;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mint = Material("Mint enamel", "53AFA0");
            cream = Material("Warm cream body", "E2CCA6");
            ivory = Material("Ivory paint", "F2DDB4");
            cherry = Material("Cherry delivery stripe", "CC3548");
            orange = Material("Toasted orange", "DC8438");
            ink = Material("Aubergine ink", "39353E");
            glass = Material("Petrol blue glass", "344C50");
            rubber = Material("Warm charcoal rubber", "37383D");
            steel = Material("Muted metal", "7D807A");
            gold = Material("Mustard details", "E9AD45");
            lettuce = Material("Lettuce green", "7F946B");
            cylinder = SaveMesh("Cylinder12", Lathe(new[] { new Vector2(0, -.5f), new Vector2(.5f, -.5f), new Vector2(.5f, .5f), new Vector2(0, .5f) }, 12));
            var rings = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
            {
                float angle = Mathf.PI * i / 8;
                rings.Add(new Vector2(Mathf.Sin(angle) * .5f, -Mathf.Cos(angle) * .5f));
            }
            sphere = SaveMesh("Sphere16", Lathe(rings.ToArray(), 16));

            BuildChassisAndWheels();
            BuildCab();
            BuildCargo();
            AssetDatabase.SaveAssets();
            PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject, PrefabPath, InteractionMode.AutomatedAction);
            return root.gameObject;
        }

        static void BuildChassisAndWheels()
        {
            var g = Group("01 Chassis and wheels");
            Box("Aubergine chassis", new Vector3(0, .72f, -.03f), new Vector3(1.96f, .31f, 5.1f), ink, g);
            Box("Underbody rail left", new Vector3(-.69f, .58f, -.17f), new Vector3(.15f, .16f, 4.75f), steel, g);
            Box("Underbody rail right", new Vector3(.69f, .58f, -.17f), new Vector3(.15f, .16f, 4.75f), steel, g);
            var tireMesh = SaveMesh("BeveledTire12", Lathe(new[] { new Vector2(0, -.21f), new Vector2(.37f, -.21f), new Vector2(.5f, -.13f), new Vector2(.5f, .13f), new Vector2(.37f, .21f), new Vector2(0, .21f) }, 12));
            var frontArch = SaveMesh("WheelArch", ArcBand(.60f, .525f, .13f, 12));
            float[] axles = { 1.78f, -1.72f };
            foreach (float z in axles)
            {
                Bar("Solid axle", new Vector3(-1.13f, AxleY, z), new Vector3(1.13f, AxleY, z), .14f, steel, g);
                for (int side = -1; side <= 1; side += 2)
                {
                    var wheel = Group((z > 0 ? "Front" : "Rear") + (side < 0 ? " left wheel" : " right wheel"), g);
                    Vector3 center = new Vector3(side * 1.08f, AxleY, z);
                    var tire = MeshObject("Twelve-sided beveled tire", tireMesh, center, Vector3.one, rubber, wheel);
                    tire.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.right);
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * 30 * Mathf.Deg2Rad;
                        var tread = Box("Chunky rubber tread", center + new Vector3(0, Mathf.Cos(a) * .495f, Mathf.Sin(a) * .495f), new Vector3(.43f, .06f, .17f), rubber, wheel);
                        tread.transform.localRotation = Quaternion.Euler(i * 30, 0, 0);
                    }
                    AxialCylinder("Ivory wheel rim", center + Vector3.right * side * .222f, .30f, .045f, Vector3.right, cream, wheel);
                    AxialCylinder("Red recessed wheel disc", center + Vector3.right * side * .249f, .225f, .026f, Vector3.right, cherry, wheel);
                    AxialCylinder("Metal hub cap", center + Vector3.right * side * .273f, .108f, .047f, Vector3.right, steel, wheel);
                    for (int i = 0; i < 4; i++)
                    {
                        float a = i * Mathf.PI * .5f;
                        AxialCylinder("Wheel lug", center + new Vector3(side * .278f, Mathf.Cos(a) * .164f, Mathf.Sin(a) * .164f), .024f, .022f, Vector3.right, ivory, wheel);
                    }
                    MeshObject("Painted wheel arch", frontArch, new Vector3(side * 1.19f, AxleY, z), Vector3.one, z > 0 ? mint : cherry, g);
                    Box("Rear mud flap", new Vector3(side * 1.09f, .41f, z - .52f), new Vector3(.42f, .43f, .065f), rubber, g);
                }
            }
            Box("Fuel tank", new Vector3(-1.02f, .68f, -.42f), new Vector3(.29f, .40f, 1.02f), steel, g);
            Box("Fuel tank strap", new Vector3(-1.18f, .68f, -.42f), new Vector3(.03f, .44f, .11f), ink, g);
            Box("Passenger access step", new Vector3(1.18f, .64f, 1.04f), new Vector3(.41f, .12f, .58f), steel, g);
            Box("Driver access step", new Vector3(-1.18f, .64f, 1.04f), new Vector3(.41f, .12f, .58f), steel, g);
            for (int side = -1; side <= 1; side += 2)
                Box("Step non-slip inset", new Vector3(side * 1.20f, .709f, 1.04f), new Vector3(.30f, .018f, .40f), ink, g);
        }

        static void BuildCab()
        {
            var g = Group("02 Beveled mint cab");
            var cabMesh = SaveMesh("ChamferedCab", RingBody(new[]
            {
                BodyRing(1.06f, .77f, .85f, 2.65f, .14f),
                BodyRing(1.15f, 1.54f, .85f, 2.72f, .14f),
                BodyRing(1.03f, 2.62f, .85f, 2.16f, .12f),
                BodyRing(.95f, 2.79f, .94f, 2.05f, .10f)
            }));
            var body = MeshObject("Chamfered cab shell", cabMesh, Vector3.zero, Vector3.one, mint, g);
            var cabCollider = body.AddComponent<BoxCollider>();
            cabCollider.center = new Vector3(0, 1.76f, 1.73f);
            cabCollider.size = new Vector3(2.12f, 2.02f, 1.76f);
            Box("Cream roof cap", new Vector3(0, 2.79f, 1.51f), new Vector3(1.89f, .075f, 1.10f), cream, g);
            Surface("Black windshield seal", new[]
            {
                new Vector3(-.96f, 1.63f, 2.69f), new Vector3(.96f, 1.63f, 2.69f),
                new Vector3(.865f, 2.57f, 2.215f), new Vector3(-.865f, 2.57f, 2.215f)
            }, Vector3.forward, ink, g);
            for (int side = -1; side <= 1; side += 2)
            {
                float left = side < 0 ? -.87f : .035f;
                float right = side < 0 ? -.035f : .87f;
                Surface("Split petrol-blue windshield", new[]
                {
                    new Vector3(left, 1.71f, 2.665f), new Vector3(right, 1.71f, 2.665f),
                    new Vector3(right * .91f, 2.49f, 2.271f), new Vector3(left * .91f, 2.49f, 2.271f)
                }, Vector3.forward, glass, g);
                Bar("Windshield wiper", new Vector3(side * .70f, 1.76f, 2.674f), new Vector3(side * .17f, 2.02f, 2.545f), .035f, ink, g);
                Surface("Side window rubber seal", new[]
                {
                    new Vector3(side * 1.146f, 1.64f, 1.02f), new Vector3(side * 1.146f, 1.64f, 2.44f),
                    new Vector3(side * 1.046f, 2.56f, 2.02f), new Vector3(side * 1.046f, 2.56f, 1.02f)
                }, Vector3.right * side, ink, g);
                Surface("Side window glass", new[]
                {
                    new Vector3(side * 1.154f, 1.73f, 1.10f), new Vector3(side * 1.154f, 1.73f, 2.30f),
                    new Vector3(side * 1.069f, 2.46f, 1.98f), new Vector3(side * 1.069f, 2.46f, 1.10f)
                }, Vector3.right * side, glass, g);
                Bar("Window division", new Vector3(side * 1.16f, 1.72f, 1.40f), new Vector3(side * 1.07f, 2.47f, 1.40f), .045f, ink, g);
                Box("Cream door stripe", new Vector3(side * 1.155f, 1.39f, 1.67f), new Vector3(.035f, .21f, 1.29f), cream, g);
                Box("Door handle recess", new Vector3(side * 1.167f, 1.53f, 1.17f), new Vector3(.035f, .10f, .25f), ink, g);
                Box("Door handle", new Vector3(side * 1.194f, 1.54f, 1.17f), new Vector3(.055f, .044f, .19f), steel, g);
                Bar("Mirror support upper", new Vector3(side * 1.06f, 2.20f, 2.15f), new Vector3(side * 1.48f, 2.12f, 2.19f), .047f, steel, g);
                Bar("Mirror support lower", new Vector3(side * 1.13f, 1.81f, 2.37f), new Vector3(side * 1.48f, 2.0f, 2.19f), .043f, steel, g);
                Box("Mirror housing", new Vector3(side * 1.49f, 2.11f, 2.18f), new Vector3(.16f, .36f, .24f), ink, g);
                Box("Mirror reflection", new Vector3(side * 1.49f, 2.11f, 2.047f), new Vector3(.12f, .28f, .025f), steel, g);
                AxialCylinder("Headlamp dark surround", new Vector3(side * .82f, 1.20f, 2.744f), .205f, .08f, Vector3.forward, ink, g);
                AxialCylinder("Warm round headlamp", new Vector3(side * .82f, 1.20f, 2.793f), .158f, .038f, Vector3.forward, ivory, g);
                Box("Amber indicator", new Vector3(side * 1.055f, 1.20f, 2.67f), new Vector3(.14f, .15f, .08f), orange, g);
                Box("Roof marker", new Vector3(side * .69f, 2.85f, 1.96f), new Vector3(.18f, .10f, .13f), orange, g);
                Box("Bumper rubber end", new Vector3(side * 1.12f, .75f, 2.79f), new Vector3(.28f, .25f, .25f), ink, g);
            }
            Box("Front cream band", new Vector3(0, 1.51f, 2.737f), new Vector3(1.96f, .17f, .07f), cream, g);
            Box("Deep grille opening", new Vector3(0, 1.14f, 2.763f), new Vector3(1.03f, .38f, .07f), ink, g);
            for (int i = 0; i < 3; i++)
                Box("Grille slat", new Vector3(0, 1.04f + i * .095f, 2.813f), new Vector3(.92f, .035f, .036f), steel, g);
            Box("Front bumper", new Vector3(0, .75f, 2.80f), new Vector3(2.42f, .20f, .25f), cream, g);
            Box("Front number plate", new Vector3(0, .825f, 2.936f), new Vector3(.68f, .18f, .025f), ink, g);
            Text("BB-0823", new Vector3(0, .827f, 2.957f), .098f, ivory.color, Quaternion.Euler(0, 180, 0), g);
            Text("B B A N G", new Vector3(0, 1.512f, 2.782f), .105f, cherry.color, Quaternion.Euler(0, 180, 0), g, FontStyle.Bold);
        }

        static void BuildCargo()
        {
            var g = Group("03 Cream cargo box and hand-painted branding");
            var cargo = SaveMesh("ChamferedCargoBox", RingBody(new[]
            {
                BodyRing(1.07f, 1.08f, -2.50f, .79f, .09f),
                BodyRing(1.15f, 1.17f, -2.58f, .87f, .10f),
                BodyRing(1.15f, 3.15f, -2.58f, .87f, .10f),
                BodyRing(1.07f, 3.24f, -2.50f, .79f, .09f)
            }));
            var cargoObj = MeshObject("Beveled cream cargo body", cargo, Vector3.zero, Vector3.one, cream, g);
            var collider = cargoObj.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 2.16f, -.855f);
            collider.size = new Vector3(2.30f, 2.16f, 3.45f);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 1.162f;
                Quaternion outward = Quaternion.Euler(0, side > 0 ? -90 : 90, 0);
                Box("Cherry lower delivery stripe", new Vector3(x, 1.38f, -.85f), new Vector3(.035f, .27f, 3.29f), cherry, g);
                Box("Thin mustard pinstripe", new Vector3(x + side * .005f, 1.57f, -.85f), new Vector3(.035f, .055f, 3.29f), gold, g);
                Box("Upper cherry accent", new Vector3(x, 3.03f, -.85f), new Vector3(.035f, .09f, 3.29f), cherry, g);
                Box("Rear vertical metal corner", new Vector3(x, 2.17f, -2.48f), new Vector3(.052f, 1.96f, .095f), steel, g);
                Box("Front vertical metal corner", new Vector3(x, 2.17f, .76f), new Vector3(.052f, 1.96f, .095f), steel, g);
                Text("BBANG", new Vector3(side * 1.193f, 2.57f, -1.02f), .47f, cherry.color, outward, g, FontStyle.Bold);
                Text("B U R G E R   D E L I V E R Y", new Vector3(side * 1.195f, 2.14f, -1.02f), .104f, ink.color, outward, g, FontStyle.Bold);
                Text("FRESH BUNS. FAST WHEELS.", new Vector3(side * 1.195f, 1.85f, -1.02f), .083f, ink.color, outward, g);
                BurgerBadge(side, new Vector3(side * 1.215f, 2.25f, .29f), g);
                Box("Side orange reflector", new Vector3(x + side * .02f, 1.37f, .57f), new Vector3(.065f, .14f, .24f), orange, g);
                Box("Side rear red reflector", new Vector3(x + side * .02f, 1.37f, -2.24f), new Vector3(.065f, .14f, .24f), cherry, g);
            }
            Box("Rear door frame", new Vector3(0, 2.13f, -2.598f), new Vector3(2.12f, 1.94f, .06f), ink, g);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Rear cargo door", new Vector3(side * .51f, 2.13f, -2.642f), new Vector3(.985f, 1.82f, .045f), ivory, g);
                Box("Rear door cherry band", new Vector3(side * .51f, 1.46f, -2.674f), new Vector3(.98f, .22f, .025f), cherry, g);
                Bar("Door locking bar", new Vector3(side * .29f, 1.35f, -2.704f), new Vector3(side * .29f, 2.78f, -2.704f), .045f, steel, g);
                Box("Locking bar grip", new Vector3(side * .22f, 1.90f, -2.736f), new Vector3(.24f, .055f, .08f), steel, g);
                for (int i = 0; i < 2; i++)
                    Box("Rear door hinge", new Vector3(side * .97f, 1.53f + i * 1.16f, -2.704f), new Vector3(.18f, .12f, .065f), steel, g);
                Box("Rear lamp housing", new Vector3(side * .93f, .98f, -2.65f), new Vector3(.35f, .25f, .14f), ink, g);
                Box("Red tail light", new Vector3(side * .99f, .98f, -2.735f), new Vector3(.19f, .17f, .035f), cherry, g);
                Box("Amber tail indicator", new Vector3(side * .80f, .98f, -2.735f), new Vector3(.13f, .17f, .035f), orange, g);
            }
            Text("BAKED DAILY", new Vector3(0, 2.92f, -2.678f), .174f, cherry.color, Quaternion.identity, g, FontStyle.Bold);
            Box("Rear bumper", new Vector3(0, .69f, -2.78f), new Vector3(2.42f, .19f, .25f), steel, g);
            Box("Rear number plate", new Vector3(0, .98f, -2.677f), new Vector3(.67f, .19f, .035f), ink, g);
            Text("BB-0823", new Vector3(0, .98f, -2.704f), .098f, ivory.color, Quaternion.identity, g);
        }

        static void BurgerBadge(int side, Vector3 center, Transform parent)
        {
            var g = Group(side > 0 ? "Right original burger badge" : "Left original burger badge", parent);
            g.localPosition = center;
            Ellipsoid("Badge toasted top bun", new Vector3(0, .21f, 0), new Vector3(.10f, .30f, .80f), orange, g);
            Ellipsoid("Badge mustard bun highlight", new Vector3(side * .02f, .29f, 0), new Vector3(.095f, .17f, .62f), gold, g);
            Box("Badge lettuce", new Vector3(side * .015f, .06f, 0), new Vector3(.09f, .09f, .83f), lettuce, g);
            Box("Badge tomato", new Vector3(side * .025f, -.035f, 0), new Vector3(.10f, .085f, .78f), cherry, g);
            Box("Badge burger patty", new Vector3(side * .02f, -.13f, 0), new Vector3(.11f, .11f, .79f), ink, g);
            Ellipsoid("Badge bottom bun", new Vector3(0, -.24f, 0), new Vector3(.10f, .17f, .76f), gold, g);
            for (int i = -1; i <= 1; i += 2)
            {
                Ellipsoid("Badge eye", new Vector3(side * .075f, .12f, i * .16f), new Vector3(.08f, .21f, .18f), ivory, g);
                Ellipsoid("Badge pupil", new Vector3(side * .121f, .13f, i * .16f), new Vector3(.035f, .11f, .085f), ink, g);
            }
            for (int i = -1; i <= 1; i++)
                Box("Badge sesame seed", new Vector3(side * .073f, .36f - Mathf.Abs(i) * .04f, i * .18f), new Vector3(.03f, .035f, .07f), ivory, g);
        }

        static Transform Group(string name, Transform parent = null)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent ? parent : root, false);
            return t;
        }

        static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
            o.name = name;
            Object.DestroyImmediate(o.GetComponent<Collider>());
            o.transform.SetParent(parent, false);
            o.transform.localPosition = position;
            o.transform.localScale = scale;
            o.GetComponent<Renderer>().sharedMaterial = material;
            return o;
        }

        static GameObject MeshObject(string name, Mesh mesh, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var o = new GameObject(name);
            o.transform.SetParent(parent, false);
            o.transform.localPosition = position;
            o.transform.localScale = scale;
            o.AddComponent<MeshFilter>().sharedMesh = mesh;
            o.AddComponent<MeshRenderer>().sharedMaterial = material;
            return o;
        }

        static void Ellipsoid(string name, Vector3 p, Vector3 scale, Material material, Transform parent) => MeshObject(name, sphere, p, scale, material, parent);

        static void AxialCylinder(string name, Vector3 p, float radius, float length, Vector3 axis, Material material, Transform parent)
        {
            var o = MeshObject(name, cylinder, p, new Vector3(radius * 2, length, radius * 2), material, parent);
            o.transform.localRotation = Quaternion.FromToRotation(Vector3.up, axis);
        }

        static void Bar(string name, Vector3 a, Vector3 b, float diameter, Material material, Transform parent) => AxialCylinder(name, (a + b) * .5f, diameter * .5f, (a - b).magnitude, b - a, material, parent);

        static void Surface(string name, Vector3[] corners, Vector3 outward, Material material, Transform parent)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            if (Vector3.Dot(Vector3.Cross(corners[1] - corners[0], corners[2] - corners[0]), outward) < 0)
                Quad(v, t, corners[0], corners[3], corners[2], corners[1]);
            else Quad(v, t, corners[0], corners[1], corners[2], corners[3]);
            // Distinct names include side so both outward windings survive asset reuse.
            string sideSuffix = outward.x < 0 ? "_Left" : outward.x > 0 ? "_Right" : corners[0].x < -.5f ? "_FrontLeft" : "_FrontRight";
            MeshObject(name, SaveMesh(name.Replace(" ", "_") + sideSuffix, Finish(v, t)), Vector3.zero, Vector3.one, material, parent);
        }

        static void Text(string text, Vector3 p, float size, Color color, Quaternion rotation, Transform parent, FontStyle style = FontStyle.Normal)
        {
            var o = new GameObject(text);
            o.transform.SetParent(parent, false);
            o.transform.localPosition = p;
            o.transform.localRotation = rotation;
            var tm = o.AddComponent<TextMesh>();
            tm.font = font; tm.text = text; tm.fontSize = 100; tm.characterSize = size * .1f;
            tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.color = color; tm.fontStyle = style;
            var renderer = o.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = GetDepthFontMaterial(); renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            o.AddComponent<RetroWorldText>().RefreshAtlas();
        }

        /// <summary>Shared persistent world-text material; add RetroWorldText to each TextMesh for live atlas binding.</summary>
        public static Material GetDepthFontMaterial()
        {
            EnsureFolder(AssetFolder);
            const string path = AssetFolder + "/WorldText_Depth.mat";
            var shader = Shader.Find("BBang/Retro World Text");
            if (!shader) throw new System.InvalidOperationException("Import Assets/RetroToon/Shaders/RetroWorldText.shader before building world text.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                material = new Material(shader) { name = "Retro world text - depth tested" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            // The dynamic atlas is not a serializable project asset. RetroWorldText supplies it per renderer.
            material.SetTexture("_MainTex", Texture2D.whiteTexture);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material Material(string name, string hex)
        {
            string path = AssetFolder + "/" + name.Replace(" ", "_") + ".mat";
            Shader shader = Shader.Find("BBang/Retro Toon") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            material.shader = shader;
            material.name = name;
            ColorUtility.TryParseHtmlString("#" + hex, out Color tint);
            material.SetColor("_BaseColor", tint);
            if (material.HasProperty("_ShadowColor")) material.SetColor("_ShadowColor", new Color(.43f, .33f, .43f, 1));
            if (material.HasProperty("_ShadowThreshold")) material.SetFloat("_ShadowThreshold", .47f);
            if (material.HasProperty("_ShadowSoftness")) material.SetFloat("_ShadowSoftness", .025f);
            if (material.HasProperty("_ShadowStrength")) material.SetFloat("_ShadowStrength", .66f);
            if (material.HasProperty("_TextureStrength")) material.SetFloat("_TextureStrength", 0);
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", Color.black);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Vector3[] BodyRing(float halfWidth, float y, float back, float front, float corner)
        {
            return new[]
            {
                new Vector3(-halfWidth + corner, y, back), new Vector3(halfWidth - corner, y, back),
                new Vector3(halfWidth, y, back + corner), new Vector3(halfWidth, y, front - corner),
                new Vector3(halfWidth - corner, y, front), new Vector3(-halfWidth + corner, y, front),
                new Vector3(-halfWidth, y, front - corner), new Vector3(-halfWidth, y, back + corner)
            };
        }

        static Mesh RingBody(Vector3[][] rings)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int r = 0; r < rings.Length - 1; r++)
            for (int i = 0; i < 8; i++)
                Quad(v, t, rings[r][i], rings[r + 1][i], rings[r + 1][(i + 1) % 8], rings[r][(i + 1) % 8]);
            Vector3 bottom = Vector3.zero, top = Vector3.zero;
            for (int i = 0; i < 8; i++) { bottom += rings[0][i] / 8; top += rings[rings.Length - 1][i] / 8; }
            for (int i = 0; i < 8; i++)
            {
                Triangle(v, t, bottom, rings[0][i], rings[0][(i + 1) % 8]);
                Triangle(v, t, top, rings[rings.Length - 1][(i + 1) % 8], rings[rings.Length - 1][i]);
            }
            return Finish(v, t);
        }

        static Mesh ArcBand(float outer, float inner, float thickness, int segments)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI / segments, b = (i + 1) * Mathf.PI / segments;
                Vector3 oa = new Vector3(thickness * .5f, Mathf.Sin(a) * outer, Mathf.Cos(a) * outer);
                Vector3 ob = new Vector3(thickness * .5f, Mathf.Sin(b) * outer, Mathf.Cos(b) * outer);
                Vector3 ia = new Vector3(thickness * .5f, Mathf.Sin(a) * inner, Mathf.Cos(a) * inner);
                Vector3 ib = new Vector3(thickness * .5f, Mathf.Sin(b) * inner, Mathf.Cos(b) * inner);
                Vector3 back = Vector3.left * thickness;
                Quad(v, t, oa, ia, ib, ob);
                Quad(v, t, oa + back, ob + back, ib + back, ia + back);
                Quad(v, t, oa, ob, ob + back, oa + back);
                Quad(v, t, ia, ia + back, ib + back, ib);
            }
            return Finish(v, t);
        }

        static Mesh Lathe(Vector2[] profile, int segments)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int r = 0; r < profile.Length - 1; r++)
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments, b = (i + 1) * Mathf.PI * 2 / segments;
                Vector3 p0 = new Vector3(Mathf.Cos(a) * profile[r].x, profile[r].y, Mathf.Sin(a) * profile[r].x);
                Vector3 p1 = new Vector3(Mathf.Cos(b) * profile[r].x, profile[r].y, Mathf.Sin(b) * profile[r].x);
                Vector3 p2 = new Vector3(Mathf.Cos(b) * profile[r + 1].x, profile[r + 1].y, Mathf.Sin(b) * profile[r + 1].x);
                Vector3 p3 = new Vector3(Mathf.Cos(a) * profile[r + 1].x, profile[r + 1].y, Mathf.Sin(a) * profile[r + 1].x);
                Quad(v, t, p0, p3, p2, p1);
            }
            return Finish(v, t);
        }

        static void Quad(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int n = v.Count; v.Add(a); v.Add(b); v.Add(c); v.Add(d);
            t.Add(n); t.Add(n + 1); t.Add(n + 2); t.Add(n); t.Add(n + 2); t.Add(n + 3);
        }

        static void Triangle(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c)
        {
            int n = v.Count; v.Add(a); v.Add(b); v.Add(c); t.Add(n); t.Add(n + 1); t.Add(n + 2);
        }

        static Mesh Finish(List<Vector3> v, List<int> t)
        {
            var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetTriangles(t, 0);
            var uv = new Vector2[v.Count];
            for (int i = 0; i < v.Count; i++) uv[i] = new Vector2(v[i].x, v[i].z);
            mesh.uv = uv; mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        static Mesh SaveMesh(string name, Mesh source)
        {
            string path = AssetFolder + "/" + name + ".asset";
            source.name = name;
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh) { AssetDatabase.CreateAsset(source, path); return source; }
            EditorUtility.CopySerialized(source, mesh); Object.DestroyImmediate(source); EditorUtility.SetDirty(mesh); return mesh;
        }

        static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/'); string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
