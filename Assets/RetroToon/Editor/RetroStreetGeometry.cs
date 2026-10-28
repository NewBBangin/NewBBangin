using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace BBang.RetroToon.Editor
{
    /// <summary>Original, reusable low-poly street art. All generated mesh and material data is saved as assets.</summary>
    public static class RetroStreetGeometry
    {
        const string Folder = "Assets/RetroToon/Generated";
        static Transform root;
        static Mesh cylinder, sphere, cone, disk;
        static Font font;
        static Material ivory, gold, amber, coral, red, plum, ink, olive, moss, cream, taupe, concrete, glass, pink, teal, road, trim;
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        public static GameObject Build()
        {
            EnsureFolder(Folder);
            materials.Clear();
            root = new GameObject("RETRO STREET | Original 3D Diorama").transform;
            cylinder = SaveMesh("Cylinder_12", Lathe(new[] { new Vector2(0, -.5f), new Vector2(.5f, -.5f), new Vector2(.5f, .5f), new Vector2(0, .5f) }, 12));
            cone = SaveMesh("Cone_12", Lathe(new[] { new Vector2(0, 0), new Vector2(.5f, 0), new Vector2(.08f, 1), new Vector2(0, 1) }, 12));
            var sphereRings = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
            {
                float a = i * Mathf.PI / 8;
                sphereRings.Add(new Vector2(Mathf.Sin(a) * .5f, -Mathf.Cos(a) * .5f));
            }
            sphere = SaveMesh("Sphere_16x8", Lathe(sphereRings.ToArray(), 16));
            disk = SaveMesh("Disk_16", Lathe(new[] { new Vector2(0, -.5f), new Vector2(.5f, -.5f), new Vector2(.5f, .5f), new Vector2(0, .5f) }, 16));
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ivory = Mat("Vanilla", "F1D5A3");
            gold = Mat("Mustard", "ECAA39");
            amber = Mat("Bun / Toasted Orange", "DA7E32");
            coral = Mat("Coral", "E44B43");
            red = Mat("Cherry Red", "BF2D46");
            plum = Mat("Muted Aubergine", "554450");
            ink = Mat("Warm Ink", "312F3A");
            olive = Mat("Dusty Olive", "8E926E");
            moss = Mat("Sage", "727F6A");
            cream = Mat("Sandstone", "C3AA80");
            taupe = Mat("Dusty Terracotta", "A58C79");
            concrete = Mat("Pavement", "848271");
            glass = Mat("Diner Window", "30464A");
            pink = Mat("Bubblegum", "F7A8D4");
            teal = Mat("Mint", "27B594");
            road = Mat("Asphalt", "56575A");
            trim = Mat("Oxidised Metal", "65615C");

            BuildGround();
            BuildCity();
            BuildDiner();
            BuildMascot();
            BuildStreetLife();
            AssetDatabase.SaveAssets();
            return root.gameObject;
        }

        static void BuildGround()
        {
            var group = Group("01 • STREET / paving and road");
            Box("City ground", new Vector3(0, -.22f, 12), new Vector3(90, .3f, 90), concrete, group);
            Box("Asphalt avenue", new Vector3(0, -.035f, -13), new Vector3(85, .12f, 20), road, group);
            Box("Raised sidewalk", new Vector3(0, .08f, 3.5f), new Vector3(52, .3f, 13), cream, group);
            Box("Sidewalk upper surface", new Vector3(0, .235f, 3.5f), new Vector3(52, .035f, 12.6f), concrete, group);
            // Thin inset seams give large, readable slabs without hundreds of individually outlined cubes.
            for (int x = -25; x <= 25; x += 2)
                Box("Paving joint", new Vector3(x, .258f, 3.45f), new Vector3(.035f, .008f, 12.45f), trim, group);
            for (float z = -2.75f; z <= 9.7f; z += 2.1f)
                Box("Paving joint", new Vector3(0, .258f, z), new Vector3(51.8f, .008f, .035f), trim, group);
            for (int x = -25; x <= 25; x += 2)
                Box("Curb stone", new Vector3(x, .16f, -3.04f), new Vector3(1.98f, .31f, .29f), cream, group);
            for (int i = -8; i <= 8; i++)
                Box("Faded avenue dash", new Vector3(i * 4.5f, .034f, -9.2f), new Vector3(2.3f, .012f, .16f), gold, group);
            Box("Double yellow line", new Vector3(0, .036f, -8.8f), new Vector3(84, .013f, .075f), gold, group);
            for (int i = 0; i < 7; i++)
                Box("Pedestrian crossing", new Vector3(-13.5f, .035f, -5.25f - i * 1.18f), new Vector3(4.6f, .016f, .62f), cream, group);
            var checker = Checker("Cafe_Checker", new Color(.85f, .73f, .56f), new Color(.30f, .31f, .30f));
            var tiled = Mat("Cafe checker floor", "FFFFFF", checker);
            Box("Checkerboard cafe terrace", new Vector3(.4f, .28f, -.4f), new Vector3(12.6f, .06f, 4.9f), tiled, group);
            // Separate UV scale on the material keeps the square tile size consistent on the top face.
            tiled.SetTextureScale("_BaseMap", new Vector2(15, 6));
            Box("Terrace front brass edge", new Vector3(.4f, .33f, -2.89f), new Vector3(12.7f, .09f, .12f), gold, group);
            Box("Terrace left edging", new Vector3(-5.95f, .33f, -.43f), new Vector3(.12f, .09f, 4.92f), gold, group);
            Box("Terrace right edging", new Vector3(6.75f, .33f, -.43f), new Vector3(.12f, .09f, 4.92f), gold, group);
            Cyl("Manhole", new Vector3(7.5f, .042f, -8), new Vector3(1.6f, .035f, 1.6f), trim, group);
            for (int i = -3; i <= 3; i++)
                Box("Manhole grooves", new Vector3(7.5f + i * .18f, .064f, -8), new Vector3(.045f, .01f, 1.05f), ink, group);
        }

        static void BuildDiner()
        {
            var g = Group("02 • BBANG / corner diner");
            Box("Warm plaster main volume", new Vector3(0, 2.8f, 4.8f), new Vector3(9.7f, 5.1f, 6.2f), cream, g);
            Box("Lower dark brick plinth", new Vector3(0, .64f, 4.77f), new Vector3(9.86f, .76f, 6.35f), plum, g);
            Box("Roof fascia", new Vector3(0, 5.26f, 4.8f), new Vector3(10.1f, .34f, 6.55f), plum, g);
            Box("Roof golden trim", new Vector3(0, 5.49f, 4.8f), new Vector3(10.22f, .15f, 6.65f), gold, g);
            Box("Flat roof", new Vector3(0, 5.57f, 4.8f), new Vector3(9.8f, .12f, 6.2f), taupe, g);
            Box("Rear parapet", new Vector3(0, 5.86f, 7.75f), new Vector3(10.0f, .65f, .27f), cream, g);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Checkered corner pier", new Vector3(side * 4.44f, 2.55f, 1.55f), new Vector3(.88f, 4.25f, .28f), coral, g);
                for (int row = 0; row < 11; row++)
                for (int col = 0; col < 2; col++)
                    Box("Porcelain facade tile", new Vector3(side * 4.44f + (col - .5f) * .42f, .61f + row * .38f, 1.38f), new Vector3(.405f, .365f, .055f), (row + col) % 2 == 0 ? gold : ivory, g);
            }
            Box("Wide window surround", new Vector3(-1.22f, 2.24f, 1.51f), new Vector3(5.45f, 2.56f, .27f), plum, g);
            Box("Deep turquoise shop window", new Vector3(-1.22f, 2.27f, 1.35f), new Vector3(5.12f, 2.28f, .09f), glass, g);
            for (int i = 0; i < 4; i++)
                Box("Window vertical mullion", new Vector3(-3.75f + i * 1.7f, 2.27f, 1.24f), new Vector3(.095f, 2.32f, .12f), cream, g);
            Box("Window sill", new Vector3(-1.22f, 1.11f, 1.15f), new Vector3(5.62f, .2f, .43f), gold, g);
            Box("Window top rail", new Vector3(-1.22f, 3.35f, 1.22f), new Vector3(5.45f, .11f, .13f), gold, g);
            // Reflections deliberately stay broad and opaque to preserve the ink-and-paint silhouette.
            for (int i = 0; i < 3; i++)
            {
                var r = Box("Muted window reflection", new Vector3(-3.1f + i * 1.7f, 2.67f, 1.285f), new Vector3(.13f, 1.06f, .018f), moss, g);
                r.transform.localRotation = Quaternion.Euler(0, 0, -27);
                Box("Window lower panel", new Vector3(-3.0f + i * 1.7f, 1.48f, 1.255f), new Vector3(1.38f, .16f, .025f), trim, g);
            }
            Box("Door casing", new Vector3(2.82f, 2.03f, 1.47f), new Vector3(1.87f, 3.29f, .26f), red, g);
            Box("Door dark inset", new Vector3(2.82f, 2.03f, 1.3f), new Vector3(1.57f, 3.01f, .095f), glass, g);
            Box("Door glass border", new Vector3(2.82f, 2.43f, 1.23f), new Vector3(1.35f, 1.91f, .045f), cream, g);
            Box("Door glass", new Vector3(2.82f, 2.43f, 1.185f), new Vector3(1.18f, 1.73f, .035f), glass, g);
            Box("Door kickplate", new Vector3(2.82f, .82f, 1.22f), new Vector3(1.3f, .43f, .07f), gold, g);
            Box("Door brass pull", new Vector3(3.28f, 1.83f, 1.08f), new Vector3(.07f, .49f, .13f), gold, g);
            Box("Door threshold", new Vector3(2.82f, .37f, 1.10f), new Vector3(2.05f, .19f, .83f), ivory, g);
            Sign("PUSH", new Vector3(3.05f, 2.08f, 1.13f), .085f, ivory.color, g);
            Box("Main cream sign panel", new Vector3(0, 4.55f, 1.39f), new Vector3(9.31f, 1.18f, .29f), ivory, g);
            Box("Sign upper red edge", new Vector3(0, 5.14f, 1.18f), new Vector3(9.46f, .1f, .12f), red, g);
            Box("Sign lower red edge", new Vector3(0, 3.97f, 1.18f), new Vector3(9.46f, .1f, .12f), red, g);
            Sign("B B A N G", new Vector3(-.07f, 4.71f, 1.205f), .72f, red.color, g, FontStyle.Bold);
            Sign("B U R G E R   C L U B", new Vector3(0, 4.18f, 1.195f), .205f, plum.color, g, FontStyle.Bold);
            for (int i = 0; i < 15; i++)
            {
                float x = -3.95f + i * .565f;
                var mesh = SaveMesh("AwningStrip", Slab(new Vector3(-.282f, .21f, .65f), new Vector3(.282f, .21f, .65f), new Vector3(.282f, -.22f, -.65f), new Vector3(-.282f, -.22f, -.65f), .06f));
                MeshObject("Canvas awning stripe", mesh, new Vector3(x, 3.64f, .68f), Vector3.one, i % 3 == 0 ? ivory : red, g);
                Box("Canvas valance", new Vector3(x, 3.32f, .015f), new Vector3(.565f, .29f, .09f), i % 3 == 0 ? ivory : red, g);
            }
            for (int i = 0; i < 3; i++)
                Bar("Awning metal brace", new Vector3(-3.7f + i * 3.7f, 3.2f, 1.35f), new Vector3(-3.7f + i * 3.7f, 3.42f, .12f), .055f, trim, g);
            Box("OPEN sign back", new Vector3(-2.85f, 2.45f, 1.12f), new Vector3(1.25f, .62f, .07f), ink, g);
            Sign("OPEN", new Vector3(-2.85f, 2.48f, 1.06f), .255f, pink.color, g, FontStyle.Bold);
            Sign("TILL LATE", new Vector3(-2.85f, 2.20f, 1.055f), .075f, ivory.color, g);
            // Diner's visible east wall.
            for (int i = 0; i < 3; i++)
            {
                Box("East wall red pillar", new Vector3(4.91f, 2.6f, 2.3f + i * 2.23f), new Vector3(.13f, 4.04f, .17f), red, g);
                Box("East window casing", new Vector3(4.915f, 2.55f, 3.17f + i * 1.93f), new Vector3(.19f, 1.85f, 1.53f), plum, g);
                Box("East glass", new Vector3(5.02f, 2.55f, 3.17f + i * 1.93f), new Vector3(.035f, 1.58f, 1.27f), glass, g);
                Box("East sill", new Vector3(5.04f, 1.6f, 3.17f + i * 1.93f), new Vector3(.36f, .15f, 1.72f), gold, g);
            }
            Box("Roof ventilation unit", new Vector3(2.9f, 6.05f, 6.3f), new Vector3(2.0f, .85f, 1.45f), trim, g);
            for (int i = 0; i < 8; i++)
                Box("Vent louver", new Vector3(2.15f + i * .21f, 6.06f, 5.56f), new Vector3(.075f, .58f, .035f), ink, g);
            Cyl("Extraction duct", new Vector3(-3.5f, 6.15f, 6.5f), new Vector3(.68f, 1.2f, .68f), trim, g);
            Cyl("Extraction cap", new Vector3(-3.5f, 6.78f, 6.5f), new Vector3(.92f, .19f, .92f), cream, g);
        }

        static void BuildMascot()
        {
            var g = Group("03 • THE BIG BUN / original mascot sculpture");
            for (int i = -1; i <= 1; i += 2)
                Box("Mascot steel roof post", new Vector3(i * 1.15f, 6.05f, 3.15f), new Vector3(.2f, 1.1f, .2f), trim, g);
            var bottom = SaveMesh("Mascot_BottomBun", Lathe(new[] { new Vector2(0, -.36f), new Vector2(1.45f, -.36f), new Vector2(2.02f, -.15f), new Vector2(2.18f, .20f), new Vector2(2.1f, .37f), new Vector2(0, .37f) }, 20));
            MeshObject("Golden bottom bun", bottom, new Vector3(0, 6.72f, 3.0f), new Vector3(1, 1, .79f), gold, g);
            MeshObject("Chargrilled patty", cylinder, new Vector3(0, 7.2f, 3), new Vector3(4.35f, .46f, 3.28f), plum, g);
            MeshObject("Tomato slice", cylinder, new Vector3(0, 7.48f, 3), new Vector3(4.52f, .17f, 3.4f), red, g);
            var lettuce = SaveMesh("Mascot_Lettuce", WavyRing(2.18f, .17f, 24));
            MeshObject("Ruffled lettuce edge", lettuce, new Vector3(0, 7.66f, 3), new Vector3(1, 1, .79f), moss, g);
            var cheese = Box("Folded cheese layer", new Vector3(0, 7.01f, 3), new Vector3(4.02f, .10f, 3.5f), gold, g);
            cheese.transform.localRotation = Quaternion.Euler(0, -11, 0);
            var cheeseCorner = SaveMesh("Cheese_Drip", Slab(new Vector3(-.65f, .18f, 0), new Vector3(.57f, .18f, 0), new Vector3(.31f, -.42f, -.09f), new Vector3(-.4f, -.25f, -.08f), .045f));
            MeshObject("Cheese folded over patty", cheeseCorner, new Vector3(1.13f, 7.03f, 1.23f), Vector3.one, gold, g);
            var top = SaveMesh("Mascot_TopBun", Lathe(new[] { new Vector2(0, 0), new Vector2(2.17f, 0), new Vector2(2.30f, .23f), new Vector2(2.18f, .57f), new Vector2(1.81f, 1.02f), new Vector2(1.13f, 1.31f), new Vector2(.38f, 1.41f), new Vector2(0, 1.42f) }, 20));
            MeshObject("Toasted sesame crown", top, new Vector3(0, 7.82f, 3), new Vector3(1, 1, .79f), amber, g);
            var topShine = SaveMesh("Mascot_BunInset", Lathe(new[] { new Vector2(0, .02f), new Vector2(1.22f, .02f), new Vector2(1.68f, .12f), new Vector2(1.50f, .32f), new Vector2(.9f, .51f), new Vector2(0, .55f) }, 20));
            MeshObject("Sunlit bun patch", topShine, new Vector3(0, 8.65f, 3), new Vector3(1, 1, .79f), gold, g);
            for (int i = 0; i < 15; i++)
            {
                float a = (i * 137.5f) * Mathf.Deg2Rad;
                float r = Mathf.Sqrt((i + .5f) / 15f) * 1.62f;
                float x = Mathf.Cos(a) * r;
                float z = Mathf.Sin(a) * r * .79f;
                float y = 9.22f - .27f * r * r;
                var seed = Ellipsoid("Ivory sesame seed", new Vector3(x, y, 3 + z), new Vector3(.23f, .055f, .09f), ivory, g);
                seed.transform.localRotation = Quaternion.Euler(z * 15, i * 53f, -x * 19f);
            }
            Ellipsoid("Left giant eye rim", new Vector3(-.89f, 7.85f, 1.225f), new Vector3(1.82f, 1.94f, .39f), plum, g);
            Ellipsoid("Right giant eye rim", new Vector3(.93f, 7.88f, 1.225f), new Vector3(1.88f, 2.03f, .39f), plum, g);
            Ellipsoid("Left vanilla eye", new Vector3(-.89f, 7.85f, 1.035f), new Vector3(1.63f, 1.76f, .34f), ivory, g);
            Ellipsoid("Right vanilla eye", new Vector3(.93f, 7.88f, 1.035f), new Vector3(1.69f, 1.85f, .34f), ivory, g);
            Ellipsoid("Left ink pupil", new Vector3(-.61f, 7.95f, .852f), new Vector3(.83f, 1.07f, .19f), ink, g);
            Ellipsoid("Right ink pupil", new Vector3(1.18f, 8.0f, .852f), new Vector3(.88f, 1.13f, .19f), ink, g);
            Ellipsoid("Left pupil glint", new Vector3(-.78f, 8.25f, .744f), new Vector3(.14f, .18f, .045f), ivory, g);
            Ellipsoid("Right pupil glint", new Vector3(.99f, 8.32f, .744f), new Vector3(.14f, .18f, .045f), ivory, g);
            Ellipsoid("Happy mouth", new Vector3(.15f, 6.89f, 1.26f), new Vector3(1.65f, .48f, .18f), ink, g);
            Box("One silly front tooth", new Vector3(.02f, 7.01f, 1.15f), new Vector3(.35f, .19f, .055f), ivory, g);
            Ellipsoid("Coral cheek left", new Vector3(-1.77f, 7.12f, 1.65f), new Vector3(.53f, .31f, .13f), coral, g);
            Ellipsoid("Coral cheek right", new Vector3(1.82f, 7.15f, 1.65f), new Vector3(.53f, .31f, .13f), coral, g);
            Arm(g, -1, new Vector3(-3.18f, 7.92f, 2.0f));
            Arm(g, 1, new Vector3(3.21f, 8.45f, 2.2f));
            Sign("GOOD BUNS. GOOD TIMES.", new Vector3(0, 5.94f, 1.49f), .18f, ivory.color, g, FontStyle.Bold);
        }

        static void Arm(Transform g, int side, Vector3 hand)
        {
            Vector3 shoulder = new Vector3(side * 1.96f, 7.1f, 3.0f);
            Vector3 elbow = new Vector3(side * 2.74f, 6.91f, 2.7f);
            Bar("Mascot bendy arm", shoulder, elbow, .22f, plum, g);
            Bar("Mascot raised forearm", elbow, hand + new Vector3(0, -.32f, .05f), .22f, plum, g);
            Ellipsoid("Vanilla mitten palm", hand, new Vector3(.88f, .72f, .47f), ivory, g);
            for (int i = 0; i < 4; i++)
                Ellipsoid("Sculpted mitten knuckle", hand + new Vector3((i - 1.5f) * .235f, .32f + (i == 1 || i == 2 ? .045f : 0), -.05f), new Vector3(.24f, .48f, .45f), ivory, g);
            Ellipsoid("Mitten thumb", hand + new Vector3(-side * .38f, -.03f, -.25f), new Vector3(.38f, .35f, .36f), ivory, g);
            Box("Mitten cuff", hand + new Vector3(0, -.37f, .035f), new Vector3(.59f, .16f, .43f), pink, g);
        }

        static void BuildCity()
        {
            var g = Group("04 • CITY / layered silhouettes");
            Building("OLIVE COURT", new Vector3(-10.75f, .25f, 8.3f), new Vector3(7.8f, 13.2f, 10.6f), olive, g, 3, 4);
            Building("SUNSET HOTEL", new Vector3(12.2f, .25f, 10.5f), new Vector3(8.6f, 17.8f, 11.1f), cream, g, 3, 5);
            Building("Back lane apartments", new Vector3(0, .25f, 17), new Vector3(12.0f, 12.4f, 8), moss, g, 5, 4);
            Building("Left factory", new Vector3(-20.2f, .25f, 16), new Vector3(9.5f, 18.2f, 11), taupe, g, 4, 5);
            Building("Right print works", new Vector3(24, .25f, 19), new Vector3(11.2f, 12.0f, 13), olive, g, 4, 3);
            var distant = Mat("Haze / distant olive", "999E7D");
            var far = Mat("Haze / distant sandstone", "B0A88A");
            for (int i = 0; i < 9; i++)
            {
                float h = 10 + ((i * 7) % 11);
                float x = -37 + i * 9;
                Box("Distant skyline block", new Vector3(x, h * .5f, 35 + i % 2 * 5), new Vector3(7.2f, h, 10), i % 2 == 0 ? distant : far, g);
                Box("Distant roof crown", new Vector3(x, h + .2f, 35 + i % 2 * 5), new Vector3(7.6f, .42f, 10.4f), i % 2 == 0 ? distant : far, g);
                for (int j = 0; j < 3; j++)
                    Box("Distant window strip", new Vector3(x, 4.0f + j * 3.3f, 29.94f + i % 2 * 5), new Vector3(5.7f, .65f, .035f), moss, g);
            }
            // Fire escape: chunky, sparse rail work keeps the silhouette legible after pixelation.
            var f = Group("Olive court fire escape", g);
            for (int floor = 0; floor < 3; floor++)
            {
                float y = 3.1f + floor * 3.0f;
                Box("Fire escape landing", new Vector3(-10.0f, y, 2.43f), new Vector3(4.8f, .15f, 1.0f), trim, f);
                Bar("Landing handrail", new Vector3(-12.4f, y + .85f, 1.94f), new Vector3(-7.6f, y + .85f, 1.94f), .052f, trim, f);
                for (int i = 0; i <= 10; i++)
                    Bar("Fire escape baluster", new Vector3(-12.35f + i * .47f, y, 1.94f), new Vector3(-12.35f + i * .47f, y + .85f, 1.94f), .04f, trim, f);
                Vector3 start = new Vector3(-12.05f, y + .04f, 2.1f);
                Vector3 end = new Vector3(-8.1f, y + 3.0f, 2.1f);
                if (floor < 2)
                {
                    Bar("Stair outer stringer", start, end, .09f, trim, f);
                    Bar("Stair inner stringer", start + Vector3.forward * .65f, end + Vector3.forward * .65f, .09f, trim, f);
                    Bar("Stair handrail", start + Vector3.up * .8f, end + Vector3.up * .8f, .045f, trim, f);
                    for (int j = 0; j < 12; j++)
                        Box("Escape stair tread", Vector3.Lerp(start, end, j / 12f) + Vector3.forward * .33f, new Vector3(.38f, .055f, .77f), trim, f);
                }
            }
            // Old rooftop billboard seen through the haze.
            var sign = Group("Rooftop radio and billboard", g);
            for (int x = -1; x <= 1; x += 2)
            {
                Bar("Billboard post", new Vector3(-10.75f + x * 2.4f, 13.2f, 7), new Vector3(-10.75f + x * 2.4f, 16.4f, 7), .15f, trim, sign);
                Bar("Billboard diagonal support", new Vector3(-10.75f + x * 2.4f, 13.4f, 9.8f), new Vector3(-10.75f + x * 2.4f, 16.3f, 7), .13f, trim, sign);
            }
            Box("Weathered billboard", new Vector3(-10.75f, 16.05f, 7), new Vector3(6.1f, 2.05f, .18f), taupe, sign);
            for (int i = 0; i < 16; i++)
                Box("Billboard vertical rib", new Vector3(-13.65f + i * .385f, 16.05f, 6.88f), new Vector3(.055f, 2.18f, .07f), trim, sign);
            Sign("S T A Y   H U N G R Y", new Vector3(-10.75f, 16.03f, 6.79f), .255f, coral.color, sign, FontStyle.Bold);
            Box("Vertical hotel sign back", new Vector3(7.72f, 12.3f, 4.6f), new Vector3(1.04f, 5.5f, .43f), red, g);
            Sign("H\nO\nT\nE\nL", new Vector3(7.72f, 12.4f, 4.34f), .54f, ivory.color, g, FontStyle.Bold);
            Box("Hotel sign little star", new Vector3(7.72f, 15.24f, 4.6f), new Vector3(.63f, .43f, .45f), gold, g);
            Box("Side lane wall", new Vector3(7.1f, .92f, 9), new Vector3(.23f, 1.35f, 10), taupe, g);
            for (int i = 0; i < 5; i++)
                Box("Back alley step", new Vector3(6.35f, .31f + i * .22f, 5.7f + i * .45f), new Vector3(2.3f, .22f * (i + 1), .48f), cream, g);
            // Pipes on near buildings, and a sagging street cable.
            Bar("Hotel drainpipe", new Vector3(8.11f, .35f, 4.82f), new Vector3(8.11f, 17.4f, 4.82f), .13f, trim, g);
            for (int i = 0; i < 4; i++)
                Box("Pipe wall bracket", new Vector3(8.11f, 2.0f + i * 4, 4.89f), new Vector3(.23f, .09f, .27f), plum, g);
            var cable = Group("Overhead cables", g);
            Vector3 last = new Vector3(-16, 12.7f, 2.4f);
            for (int i = 1; i <= 18; i++)
            {
                float t = i / 18f;
                Vector3 p = new Vector3(Mathf.Lerp(-16, 17, t), 12.7f - Mathf.Sin(t * Mathf.PI) * 2.6f, 2.4f + t * 1.8f);
                Bar("Sagging power cable", last, p, .025f, ink, cable);
                last = p;
            }
            Bar("Aerial mast", new Vector3(13.7f, 18.1f, 10), new Vector3(13.7f, 21.0f, 10), .07f, trim, g);
            Bar("Aerial boom", new Vector3(12.6f, 20.15f, 10), new Vector3(14.8f, 20.15f, 10), .055f, trim, g);
            for (int i = -2; i <= 2; i++)
                Bar("Aerial element", new Vector3(13.7f + i * .48f, 20.15f, 9.4f), new Vector3(13.7f + i * .48f, 20.15f, 10.6f), .04f, trim, g);
        }

        static void Building(string name, Vector3 origin, Vector3 size, Material wall, Transform parent, int columns, int floors)
        {
            var g = Group(name, parent);
            float front = origin.z - size.z * .5f;
            Box("Masonry", origin + Vector3.up * size.y * .5f, size, wall, g);
            Box("Foundation course", origin + Vector3.up * .47f, new Vector3(size.x + .2f, .94f, size.z + .16f), taupe, g);
            Box("Roof cornice", origin + Vector3.up * (size.y - .13f), new Vector3(size.x + .46f, .31f, size.z + .46f), cream, g);
            Box("Roof dark line", origin + Vector3.up * (size.y + .08f), new Vector3(size.x + .19f, .12f, size.z + .19f), trim, g);
            for (int f = 0; f < floors; f++)
            {
                float y = origin.y + 2.2f + f * (size.y - 3.1f) / floors;
                for (int c = 0; c < columns; c++)
                {
                    float x = origin.x + (c - (columns - 1) * .5f) * size.x / (columns + .7f);
                    float w = size.x / (columns + .7f) * .61f;
                    Box("Inset window shadow", new Vector3(x, y, front - .025f), new Vector3(w + .19f, 1.65f, .11f), trim, g);
                    Box("Dusty blue-green glass", new Vector3(x, y, front - .09f), new Vector3(w, 1.43f, .04f), f % 3 == 1 && c % 2 == 0 ? olive : moss, g);
                    Box("Window dividing bar", new Vector3(x, y, front - .13f), new Vector3(.075f, 1.45f, .05f), wall, g);
                    Box("Window sill", new Vector3(x, y - .88f, front - .17f), new Vector3(w + .35f, .13f, .34f), cream, g);
                    if ((f + c) % 4 == 0)
                        Box("Sunlit window blind", new Vector3(x, y + .31f, front - .142f), new Vector3(w, .54f, .025f), taupe, g);
                }
                Box("Floor belt course", new Vector3(origin.x, y - 1.17f, front - .10f), new Vector3(size.x + .13f, .10f, .18f), wall == cream ? taupe : cream, g);
            }
            // Facing side gets broad relief and windows where the three-quarter camera can see it.
            for (int f = 0; f < floors; f++)
            for (int c = 0; c < 3; c++)
            {
                float x = origin.x + size.x * .5f + .04f;
                float y = origin.y + 2.2f + f * (size.y - 3.1f) / floors;
                float z = front + 1.7f + c * (size.z - 2.8f) / 3;
                Box("Side window inset", new Vector3(x, y, z), new Vector3(.09f, 1.49f, 1.1f), trim, g);
                Box("Side window glass", new Vector3(x + .055f, y, z), new Vector3(.035f, 1.26f, .91f), moss, g);
                Box("Side sill", new Vector3(x + .09f, y - .81f, z), new Vector3(.27f, .12f, 1.28f), cream, g);
            }
        }

        static void BuildStreetLife()
        {
            var g = Group("05 • STREET LIFE / cafe furniture and props");
            CafeSet(new Vector3(5.6f, .34f, -.15f), 1.87f, g);
            CafeSet(new Vector3(-4.15f, .34f, -.9f), 1.35f, g);
            TrafficCone(new Vector3(-6.6f, .26f, -2.0f), 1.05f, g);
            TrafficCone(new Vector3(7.35f, .05f, -4.2f), 1.38f, g);
            TrafficCone(new Vector3(8.85f, .05f, -5.4f), .90f, g);
            var board = Group("A-board / daily special", g);
            Box("A-board chalk panel", new Vector3(-.3f, 1.15f, -.78f), new Vector3(1.13f, 1.42f, .12f), ink, board);
            Box("A-board gold top", new Vector3(-.3f, 1.9f, -.78f), new Vector3(1.34f, .12f, .18f), gold, board);
            for (int s = -1; s <= 1; s += 2)
            {
                Bar("Sandwich board front leg", new Vector3(-.3f + s * .62f, .34f, -1.0f), new Vector3(-.3f + s * .62f, 1.96f, -.7f), .08f, gold, board);
                Bar("Sandwich board back leg", new Vector3(-.3f + s * .62f, .34f, .02f), new Vector3(-.3f + s * .62f, 1.96f, -.7f), .08f, gold, board);
            }
            Sign("TODAY", new Vector3(-.3f, 1.6f, -.86f), .20f, pink.color, board, FontStyle.Bold);
            Sign("BUN + FRIES", new Vector3(-.3f, 1.26f, -.86f), .105f, ivory.color, board);
            Sign("05.90", new Vector3(-.3f, .89f, -.86f), .265f, gold.color, board, FontStyle.Bold);
            Box("Sandwich board chalk underline", new Vector3(-.3f, .67f, -.855f), new Vector3(.71f, .023f, .012f), ivory, board);
            // A lamp, bin, delivery crate and hydrant provide familiar scale cues.
            StreetLamp(new Vector3(-7.9f, .27f, -.45f), g);
            StreetLamp(new Vector3(10.6f, .27f, 2.25f), g);
            Cyl("Litter bin body", new Vector3(-6.3f, .9f, 1.15f), new Vector3(.82f, 1.26f, .82f), moss, g);
            Cyl("Litter bin lid", new Vector3(-6.3f, 1.57f, 1.15f), new Vector3(.93f, .16f, .93f), trim, g);
            Box("Litter bin opening", new Vector3(-6.3f, 1.31f, .72f), new Vector3(.45f, .22f, .04f), ink, g);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * .2f;
                Bar("Bin vertical rib", new Vector3(-6.3f + Mathf.Cos(a) * .416f, .37f, 1.15f + Mathf.Sin(a) * .416f), new Vector3(-6.3f + Mathf.Cos(a) * .416f, 1.46f, 1.15f + Mathf.Sin(a) * .416f), .035f, trim, g);
            }
            Cyl("Hydrant main barrel", new Vector3(10.8f, .82f, -2.25f), new Vector3(.53f, 1.10f, .53f), coral, g);
            Ellipsoid("Hydrant cap", new Vector3(10.8f, 1.39f, -2.25f), new Vector3(.62f, .32f, .62f), red, g);
            Bar("Hydrant cross valve", new Vector3(10.35f, 1.0f, -2.25f), new Vector3(11.25f, 1.0f, -2.25f), .28f, red, g);
            Cyl("Hydrant footing", new Vector3(10.8f, .33f, -2.25f), new Vector3(.75f, .16f, .75f), trim, g);
            for (int i = 0; i < 3; i++)
            {
                Vector3 c = new Vector3(5.55f, .57f + i * .52f, 6.3f);
                Box("Delivery crate", c, new Vector3(.95f, .49f, .82f), amber, g);
                for (int j = 0; j < 3; j++)
                    Box("Crate vent", c + new Vector3((j - 1) * .26f, 0, -.42f), new Vector3(.16f, .21f, .025f), plum, g);
            }
            Planter(new Vector3(3.97f, .37f, .0f), g);
            Planter(new Vector3(-5.35f, .31f, 2.5f), g);
            // Small scattered paper details, deliberately restrained.
            var paper = Box("Folded street flyer", new Vector3(3.3f, .06f, -5.3f), new Vector3(.38f, .015f, .54f), cream, g);
            paper.transform.localRotation = Quaternion.Euler(0, -23, 0);
        }

        static void CafeSet(Vector3 position, float radius, Transform parent)
        {
            var g = Group("Red and mustard cafe parasol", parent);
            g.localPosition = position;
            float top = radius > 1.5f ? 3.55f : 2.96f;
            Cyl("Parasol pedestal", new Vector3(0, .05f, 0), new Vector3(.61f, .1f, .61f), plum, g);
            Bar("Parasol brass pole", new Vector3(0, .08f, 0), new Vector3(0, top + .08f, 0), .065f, gold, g);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * .25f;
                float b = (i + 1) * Mathf.PI * .25f;
                Vector3 pa = new Vector3(Mathf.Cos(a) * radius, top - .67f, Mathf.Sin(a) * radius);
                Vector3 pb = new Vector3(Mathf.Cos(b) * radius, top - .67f, Mathf.Sin(b) * radius);
                var canopy = SaveMesh("Parasol_" + radius.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "_" + i, TrianglePrism(new Vector3(0, top, 0), pa, pb, .035f));
                MeshObject("Alternating canvas panel", canopy, Vector3.zero, Vector3.one, i % 2 == 0 ? coral : gold, g);
                Bar("Canvas radial seam", new Vector3(0, top - .025f, 0), pa + Vector3.up * .018f, .018f, amber, g);
            }
            Ellipsoid("Umbrella finial", new Vector3(0, top + .06f, 0), new Vector3(.17f, .17f, .17f), gold, g);
            MeshObject("Round bistro tabletop", disk, new Vector3(0, 1.12f, 0), new Vector3(1.31f, .13f, 1.31f), cream, g);
            Cyl("Table center support", new Vector3(0, .58f, 0), new Vector3(.19f, 1.08f, .19f), plum, g);
            for (int i = 0; i < 3; i++)
            {
                float a = (i * 120 + 15) * Mathf.Deg2Rad;
                Vector3 stool = new Vector3(Mathf.Cos(a) * 1.12f, 0, Mathf.Sin(a) * 1.12f);
                Cyl("Cherry stool seat", stool + Vector3.up * .76f, new Vector3(.58f, .15f, .58f), red, g);
                Cyl("Stool column", stool + Vector3.up * .36f, new Vector3(.1f, .7f, .1f), plum, g);
                Cyl("Stool weighted foot", stool + Vector3.up * .04f, new Vector3(.49f, .075f, .49f), plum, g);
            }
            Cyl("Paper soda cup", new Vector3(.32f, 1.32f, .14f), new Vector3(.16f, .28f, .16f), ivory, g);
            Cyl("Soda cup red band", new Vector3(.32f, 1.31f, .14f), new Vector3(.165f, .10f, .165f), coral, g);
            Bar("Soda straw", new Vector3(.32f, 1.44f, .14f), new Vector3(.34f, 1.65f, .14f), .013f, red, g);
        }

        static void TrafficCone(Vector3 origin, float height, Transform parent)
        {
            var g = Group("Orange and pink traffic cone", parent);
            g.localPosition = origin;
            g.localScale = Vector3.one * height;
            Box("Square rubber foot", new Vector3(0, .055f, 0), new Vector3(.91f, .11f, .91f), plum, g);
            Box("Orange base", new Vector3(0, .12f, 0), new Vector3(.84f, .10f, .84f), amber, g);
            MeshObject("Traffic cone", cone, new Vector3(0, .15f, 0), new Vector3(.74f, 1.27f, .74f), amber, g);
            // Two tapered overlays sit just outside the base cone, leaving broad orange stripes.
            var band1 = SaveMesh("ConeBand_Lower", Lathe(new[] { new Vector2(.292f, .36f), new Vector2(.24f, .58f) }, 12));
            var band2 = SaveMesh("ConeBand_Upper", Lathe(new[] { new Vector2(.18f, .83f), new Vector2(.132f, 1.04f) }, 12));
            MeshObject("Pink reflective band lower", band1, new Vector3(0, .15f, 0), Vector3.one, pink, g);
            MeshObject("Pink reflective band upper", band2, new Vector3(0, .15f, 0), Vector3.one, pink, g);
        }

        static void StreetLamp(Vector3 origin, Transform parent)
        {
            var g = Group("Old street lamp", parent);
            g.localPosition = origin;
            Cyl("Lamp base", new Vector3(0, .14f, 0), new Vector3(.48f, .28f, .48f), trim, g);
            Cyl("Lamp post", new Vector3(0, 2.9f, 0), new Vector3(.14f, 5.55f, .14f), trim, g);
            Bar("Lamp curved arm inner", new Vector3(0, 5.6f, 0), new Vector3(.32f, 6.04f, 0), .12f, trim, g);
            Bar("Lamp curved arm outer", new Vector3(.32f, 6.04f, 0), new Vector3(1.05f, 6.04f, 0), .12f, trim, g);
            var shade = SaveMesh("Lamp_Shade", Lathe(new[] { new Vector2(0, -.08f), new Vector2(.39f, -.08f), new Vector2(.39f, 0), new Vector2(.16f, .22f), new Vector2(0, .22f) }, 12));
            MeshObject("Lamp bell shade", shade, new Vector3(1.08f, 5.89f, 0), Vector3.one, trim, g);
            Cyl("Warm lamp lens", new Vector3(1.08f, 5.80f, 0), new Vector3(.66f, .025f, .66f), ivory, g);
        }

        static void Planter(Vector3 origin, Transform parent)
        {
            var g = Group("Terracotta sidewalk planter", parent);
            g.localPosition = origin;
            Cyl("Planter pot", new Vector3(0, .26f, 0), new Vector3(.59f, .52f, .59f), amber, g);
            Cyl("Planter rim", new Vector3(0, .53f, 0), new Vector3(.68f, .12f, .68f), coral, g);
            Cyl("Dark soil", new Vector3(0, .59f, 0), new Vector3(.56f, .025f, .56f), plum, g);
            for (int i = 0; i < 7; i++)
            {
                float a = i * Mathf.PI * 2 / 7;
                var leaf = Ellipsoid("Broad stylized leaf", new Vector3(Mathf.Cos(a) * .22f, .87f + i % 2 * .12f, Mathf.Sin(a) * .22f), new Vector3(.17f, .77f, .15f), i % 2 == 0 ? moss : olive, g);
                leaf.transform.localRotation = Quaternion.Euler(Mathf.Sin(a) * 30, 0, -Mathf.Cos(a) * 30);
            }
        }

        static Transform Group(string name, Transform parent = null)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent ? parent : root, false);
            return t;
        }

        static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            Object.DestroyImmediate(obj.GetComponent<Collider>());
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            obj.isStatic = true;
            return obj;
        }

        static GameObject MeshObject(string name, Mesh mesh, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial = material;
            obj.isStatic = true;
            return obj;
        }

        static GameObject Cyl(string name, Vector3 p, Vector3 scale, Material mat, Transform parent) => MeshObject(name, cylinder, p, scale, mat, parent);
        static GameObject Ellipsoid(string name, Vector3 p, Vector3 scale, Material mat, Transform parent) => MeshObject(name, sphere, p, scale, mat, parent);

        static void Bar(string name, Vector3 a, Vector3 b, float diameter, Material mat, Transform parent)
        {
            var go = Cyl(name, (a + b) * .5f, new Vector3(diameter, Vector3.Distance(a, b), diameter), mat, parent);
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }

        static void Sign(string text, Vector3 p, float size, Color color, Transform parent, FontStyle style = FontStyle.Normal)
        {
            var o = new GameObject(text.Replace('\n', ' '));
            o.transform.SetParent(parent, false);
            o.transform.localPosition = p;
            var t = o.AddComponent<TextMesh>();
            t.font = font;
            t.text = text;
            t.fontSize = 100;
            t.characterSize = size * .1f;
            t.anchor = TextAnchor.MiddleCenter;
            t.alignment = TextAlignment.Center;
            t.color = color;
            t.fontStyle = style;
            t.lineSpacing = .85f;
            var renderer = o.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = RetroTruckGeometry.GetDepthFontMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            o.AddComponent<RetroWorldText>();
        }

        static Material Mat(string name, string hex, Texture2D texture = null)
        {
            if (materials.TryGetValue(name, out var cached)) return cached;
            string path = Folder + "/" + name.Replace(" / ", "_").Replace("/", "_") + ".mat";
            Shader shader = Shader.Find("BBang/Retro Toon") ?? Shader.Find("Universal Render Pipeline/Lit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
            else mat.shader = shader;
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            mat.name = name;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", new Color(.43f, .33f, .43f, 1));
            if (mat.HasProperty("_ShadowThreshold")) mat.SetFloat("_ShadowThreshold", .47f);
            if (mat.HasProperty("_ShadowSoftness")) mat.SetFloat("_ShadowSoftness", .025f);
            if (mat.HasProperty("_ShadowStrength")) mat.SetFloat("_ShadowStrength", .66f);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.black);
            if (mat.HasProperty("_TextureStrength")) mat.SetFloat("_TextureStrength", texture ? 1 : 0);
            if (texture && mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0);
            EditorUtility.SetDirty(mat);
            materials[name] = mat;
            return mat;
        }

        static Texture2D Checker(string name, Color a, Color b)
        {
            string path = Folder + "/" + name + ".asset";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (!tex) { tex = new Texture2D(2, 2, TextureFormat.RGBA32, false); AssetDatabase.CreateAsset(tex, path); }
            tex.name = name;
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.SetPixels(new[] { a, b, b, a });
            tex.Apply();
            EditorUtility.SetDirty(tex);
            return tex;
        }

        static Mesh SaveMesh(string name, Mesh mesh)
        {
            string path = Folder + "/" + name + ".asset";
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            mesh.name = name;
            if (!saved) { AssetDatabase.CreateAsset(mesh, path); return mesh; }
            EditorUtility.CopySerialized(mesh, saved);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(saved);
            return saved;
        }

        static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static Mesh Lathe(Vector2[] profile, int segments)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int ring = 0; ring < profile.Length - 1; ring++)
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                float b = (i + 1) * Mathf.PI * 2 / segments;
                Vector3 p0 = new Vector3(Mathf.Cos(a) * profile[ring].x, profile[ring].y, Mathf.Sin(a) * profile[ring].x);
                Vector3 p1 = new Vector3(Mathf.Cos(b) * profile[ring].x, profile[ring].y, Mathf.Sin(b) * profile[ring].x);
                Vector3 p2 = new Vector3(Mathf.Cos(b) * profile[ring + 1].x, profile[ring + 1].y, Mathf.Sin(b) * profile[ring + 1].x);
                Vector3 p3 = new Vector3(Mathf.Cos(a) * profile[ring + 1].x, profile[ring + 1].y, Mathf.Sin(a) * profile[ring + 1].x);
                AddQuad(verts, tris, p0, p3, p2, p1);
            }
            return Finish(verts, tris);
        }

        static Mesh WavyRing(float radius, float height, int segments)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                float b = (i + 1) * Mathf.PI * 2 / segments;
                float ra = radius + (i % 2 == 0 ? .15f : -.08f);
                float rb = radius + ((i + 1) % 2 == 0 ? .15f : -.08f);
                Vector3 pa = new Vector3(Mathf.Cos(a) * ra, i % 2 == 0 ? height : -height, Mathf.Sin(a) * ra);
                Vector3 pb = new Vector3(Mathf.Cos(b) * rb, (i + 1) % 2 == 0 ? height : -height, Mathf.Sin(b) * rb);
                AddTriangle(v, t, Vector3.up * .06f, pb, pa);
                AddQuad(v, t, pa, pb, pb + Vector3.down * .08f, pa + Vector3.down * .08f);
            }
            return Finish(v, t);
        }

        static Mesh Slab(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float thickness)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            Vector3 off = Vector3.down * thickness;
            AddQuad(v, t, a, b, c, d);
            AddQuad(v, t, d + off, c + off, b + off, a + off);
            AddQuad(v, t, a + off, b + off, b, a);
            AddQuad(v, t, b + off, c + off, c, b);
            AddQuad(v, t, c + off, d + off, d, c);
            AddQuad(v, t, d + off, a + off, a, d);
            return Finish(v, t);
        }

        static Mesh TrianglePrism(Vector3 a, Vector3 b, Vector3 c, float thickness)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            Vector3 off = Vector3.down * thickness;
            AddTriangle(v, t, a, c, b);
            AddTriangle(v, t, a + off, b + off, c + off);
            AddQuad(v, t, a, b, b + off, a + off);
            AddQuad(v, t, b, c, c + off, b + off);
            AddQuad(v, t, c, a, a + off, c + off);
            return Finish(v, t);
        }

        static void AddQuad(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int n = v.Count; v.Add(a); v.Add(b); v.Add(c); v.Add(d);
            t.Add(n); t.Add(n + 1); t.Add(n + 2); t.Add(n); t.Add(n + 2); t.Add(n + 3);
        }

        static void AddTriangle(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c)
        {
            int n = v.Count; v.Add(a); v.Add(b); v.Add(c); t.Add(n); t.Add(n + 1); t.Add(n + 2);
        }

        static Mesh Finish(List<Vector3> vertices, List<int> triangles)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            var uv = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++) uv[i] = new Vector2(vertices[i].x, vertices[i].z);
            mesh.uv = uv;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
