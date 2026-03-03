using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using AmishSimulator;

namespace AmishSimulator.Editor
{
    public static class SceneSetup
    {
        [MenuItem("Amish Simulator/Create Homestead Scene")]
        public static void CreateHomesteadScene()
        {
            // Create and save new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // ── Systems GameObject ─────────────────────────────────────────
            var systems = new GameObject("--- SYSTEMS ---");
            systems.AddComponent<GameManager>();
            systems.AddComponent<TimeSystem>();
            systems.AddComponent<SeasonSystem>();
            systems.AddComponent<DifficultyManager>();
            systems.AddComponent<EnergySystem>();
            systems.AddComponent<HungerSystem>();
            systems.AddComponent<FoodSystem>();
            systems.AddComponent<RelationshipSystem>();
            systems.AddComponent<OrdnungSystem>();
            systems.AddComponent<EventCalendar>();
            systems.AddComponent<CourtshipSystem>();
            systems.AddComponent<HomesteadBootstrap>();

            // ── Player GameObject ──────────────────────────────────────────
            var player = new GameObject("Player");
            player.AddComponent<PlayerController>();
            player.AddComponent<AgingSystem>();
            player.AddComponent<BeardSystem>();
            player.AddComponent<InteractionSystem>();
            var pc = player.AddComponent<ProceduralCharacter>();
            pc.gender = Gender.Male;
            player.transform.position = new Vector3(0, 0, 0);

            // ── Environment ────────────────────────────────────────────────
            var env = new GameObject("--- ENVIRONMENT ---");

            // Ground plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(env.transform);
            ground.transform.localScale = new Vector3(10, 1, 10);
            var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMat.color = new Color(0.45f, 0.55f, 0.30f); // grass green
            ground.GetComponent<Renderer>().material = groundMat;

            // Barn
            var barnGo = new GameObject("Barn");
            barnGo.transform.SetParent(env.transform);
            barnGo.transform.position = new Vector3(8, 0, 0);
            barnGo.AddComponent<ProceduralBarn>();

            // Butter churn prop + gameplay + interaction
            var churnGo = new GameObject("ButterChurn");
            churnGo.transform.SetParent(env.transform);
            churnGo.transform.position = new Vector3(-3, 0, 2);
            churnGo.AddComponent<ProceduralButterChurn>();
            churnGo.AddComponent<ButterChurning>();
            churnGo.AddComponent<ChurnStation>();

            // Plow prop + gameplay + interaction
            var plowGo = new GameObject("Plow");
            plowGo.transform.SetParent(env.transform);
            plowGo.transform.position = new Vector3(3, 0, -3);
            plowGo.AddComponent<ProceduralPlow>();
            plowGo.AddComponent<Plowing>();
            plowGo.AddComponent<PlowPoint>();

            // Sleep point near the barn entrance
            var sleepGo = new GameObject("SleepPoint");
            sleepGo.transform.SetParent(env.transform);
            sleepGo.transform.position = new Vector3(4, 0, 0);
            sleepGo.AddComponent<SleepPoint>();

            // NPCs — neighbour family patrolling the field
            for (int i = 0; i < 2; i++)
            {
                var npcGo = new GameObject($"NPC_Neighbor{i}");
                npcGo.transform.SetParent(env.transform);
                npcGo.transform.position = new Vector3(-6 + i * 4, 0, 6);
                var npcChar = npcGo.AddComponent<ProceduralCharacter>();
                npcChar.gender = (i % 2 == 0) ? Gender.Male : Gender.Female;
                npcGo.AddComponent<NPCPatrol>();
            }

            // ── Camera setup ───────────────────────────────────────────────
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 8, -12);
                cam.transform.rotation = Quaternion.Euler(30, 0, 0);
                cam.backgroundColor = new Color(0.53f, 0.74f, 0.90f); // sky blue
                cam.gameObject.AddComponent<CameraFollow>();
                cam.gameObject.AddComponent<DayNightController>();
            }

            // ── Directional light ──────────────────────────────────────────
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    l.transform.rotation = Quaternion.Euler(45, -30, 0);
                    l.color = new Color(1f, 0.95f, 0.80f); // warm sunlight
                    l.intensity = 1.2f;
                }
            }

            // ── UI Canvas ─────────────────────────────────────────────────
            var canvasGo = new GameObject("HUD Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasGo.AddComponent<HUD>();
            canvasGo.AddComponent<PauseMenu>();
            canvasGo.AddComponent<FailScreen>();
            canvasGo.AddComponent<ScoreScreen>();

            // ── Save scene ─────────────────────────────────────────────────
            System.IO.Directory.CreateDirectory(Application.dataPath + "/Scenes");
            string path = "Assets/Scenes/Homestead.unity";
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();

            Debug.Log("Homestead scene created at " + path + " — open it in the Project window.");
            EditorUtility.DisplayDialog("Done!", "Homestead scene created at Assets/Scenes/Homestead.unity", "Open it!");
        }

        [MenuItem("Amish Simulator/Create MainMenu Scene")]
        public static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Systems needed for main menu
            var systems = new GameObject("--- SYSTEMS ---");
            systems.AddComponent<GameManager>();
            systems.AddComponent<DifficultyManager>();

            // UI Canvas
            var canvasGo = new GameObject("MainMenu Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasGo.AddComponent<MainMenuUI>();
            canvasGo.AddComponent<DifficultyUI>();

            // Camera
            var cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = new Color(0.20f, 0.25f, 0.15f); // dark farm green
                cam.clearFlags = CameraClearFlags.SolidColor;
            }

            System.IO.Directory.CreateDirectory(Application.dataPath + "/Scenes");
            string path = "Assets/Scenes/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();

            Debug.Log("MainMenu scene created at " + path);
            EditorUtility.DisplayDialog("Done!", "MainMenu scene created at Assets/Scenes/MainMenu.unity", "OK");
        }
    }
}
