using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BattleArenaOpenWorldBuilder
{
    private const string SourceScenePath = "Assets/Scenes/Battle Arena.unity";
    private const string OutputScenePath = "Assets/Scenes/Battle Arena Open World Blockout.unity";
    private const string RootName = "Aethelgard_OpenWorld_Map";

    private const string FloorPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Floor_low_C.fbx";
    private const string Wall4Path = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Wall_4x0.6x4_low_C.fbx";
    private const string Wall2Path = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Wall_2x0.6x4_low_C.fbx";
    private const string RockLargePath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Rock_01_low.fbx";
    private const string RockSmall01Path = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Rock_Small_01.fbx";
    private const string RockSmall02Path = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Rock_small_02.fbx";
    private const string RockSmall03Path = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Rock_small_03.fbx";
    private const string TreePath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Tree.fbx";
    private const string PillarPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/pillar_4_low.fbx";
    private const string PillarShortPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/pillar_25_low.fbx";
    private const string LampPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/PillarLamp.fbx";
    private const string StatuePath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Player_Statue.fbx";
    private const string StairsShortPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Stairs_short_low.fbx";
    private const string StairsHighPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Stairs_high_low.fbx";
    private const string ArcMidPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Arc_Mid_high.fbx";
    private const string ArcSidePath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Mesh/Arc_Side_high.test.fbx";
    private const string CoinPrefabPath = "Assets/Collectables/Prefabs/Coin.prefab";
    private const string GatePrefabPath = "Assets/Characters/Enemy/Prefabs/Gate.prefab";
    private const string GateWidePath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Prefab/Gate_wide.prefab";
    private const string GateNarrowPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Prefab/Gate_narrow.prefab";
    private const string LeavesVfxPath = "Assets/ProjectLittleAdventurer/VFX/Prefab/VFX Leaves.prefab";
    private const string WindVfxPath = "Assets/ProjectLittleAdventurer/VFX/Prefab/Particle Wind.prefab";
    private const string SmokeVfxPath = "Assets/ProjectLittleAdventurer/VFX/Prefab/Particle Smoke.prefab";
    private const string WaterMaterialPath = "Assets/ProjectLittleAdventurer/Mesh Asset/Environment/Material/PillarLamp Blue.mat";
    private const string LavaMaterialPath = "Assets/ProjectLittleAdventurer/VFX/Materials/Bullet_Core_VFX.mat";

    private static readonly Vector3 ModelScale = Vector3.one * 100f;

    [MenuItem("Tools/Little Adventure/World/Rebuild Battle Arena Open World Blockout")]
    public static void RebuildFromMenu()
    {
        Rebuild();
    }

    public static void RebuildFromCommandLine()
    {
        Rebuild();
    }

    private static void Rebuild()
    {
        EnsureOutputSceneExists();

        Scene scene = EditorSceneManager.GetSceneByPath(OutputScenePath);
        bool closeAfterSave = !scene.IsValid() || !scene.isLoaded;
        if (closeAfterSave)
            scene = EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Additive);

        RemoveExistingRoot(scene);

        GameObject root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = Vector3.zero;

        BuildWorld(root.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        if (closeAfterSave)
            EditorSceneManager.CloseScene(scene, true);

        AssetDatabase.Refresh();
        Debug.Log($"Built visual open-world blockout scene: {OutputScenePath}");
    }

    private static void EnsureOutputSceneExists()
    {
        if (File.Exists(OutputScenePath))
            return;

        if (!File.Exists(SourceScenePath))
            throw new FileNotFoundException($"Source scene not found: {SourceScenePath}");

        AssetDatabase.CopyAsset(SourceScenePath, OutputScenePath);
        AssetDatabase.ImportAsset(OutputScenePath);
    }

    private static void RemoveExistingRoot(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name == RootName)
            {
                Object.DestroyImmediate(rootObject);
                return;
            }
        }
    }

    private static void BuildWorld(Transform root)
    {
        CreateLabel(root, "AETHELGARD OPEN WORLD BLOCKOUT", new Vector3(16f, 0.1f, 74f), 5.5f);
        BuildRadiantCitadelApproach(root);
        BuildCentralRiverAndBridge(root);
        BuildVerdantWilds(root);
        BuildRadiantJungle(root);
        BuildJungleRuins(root);
        BuildNorthernHighRoad(root);
        BuildDirePeaks(root);
        BuildBlightlands(root);
        BuildDireStronghold(root);
        BuildFinalLandmark(root);
    }

    private static void BuildRadiantCitadelApproach(Transform root)
    {
        Transform section = CreateSection(root, "01_Radiant_Citadel_Approach", new Vector3(16f, 0f, 96f), 4, 3, true);
        CreateLabel(section, "RADIANT CITADEL APPROACH", new Vector3(16f, 0.1f, 120f), 3f);
        PlaceRoad(section, new Vector3(16f, 0f, 80f), new Vector3(16f, 0f, 128f), 4);
        PlaceModel(StatuePath, "Citadel_Route_Statue", section, new Vector3(16f, 0f, 112f), 180f, Vector3.one * 130f);
        PlaceModel(LampPath, "Lamp_Left", section, new Vector3(4f, 0f, 104f), 0f, ModelScale);
        PlaceModel(LampPath, "Lamp_Right", section, new Vector3(28f, 0f, 104f), 0f, ModelScale);
        PlacePrefab(GateNarrowPath, "Visual_Gate_To_Road", section, new Vector3(16f, 0f, 130f), Quaternion.identity, ModelScale);
        PlaceCoins(section, new Vector3(16f, 0.4f, 92f), 5, 2.5f);
    }

    private static void BuildCentralRiverAndBridge(Transform root)
    {
        Transform section = CreateEmpty(root, "02_Central_River_And_Great_Span");
        CreateLabel(section, "THE CENTRAL RIVER", new Vector3(28f, 0.1f, 168f), 4f);
        CreateLabel(section, "GREAT SPAN", new Vector3(72f, 0.1f, 176f), 2.8f);

        PlaceWater(section, new Vector3(16f, -0.12f, 168f), new Vector3(150f, 0.08f, 26f), "Central_River_West");
        PlaceWater(section, new Vector3(108f, -0.12f, 184f), new Vector3(120f, 0.08f, 22f), "Central_River_East");
        PlaceBridge(section, new Vector3(72f, 0f, 176f), 8);
        PlaceModel(ArcMidPath, "Bridge_Arch_Center", section, new Vector3(72f, 0f, 176f), 90f, ModelScale);

        PlaceRockCluster(section, new Vector3(-40f, -1.5f, 160f), 5, 0);
        PlaceRockCluster(section, new Vector3(135f, -1.5f, 194f), 5, 2);
        PlacePrefab(WindVfxPath, "River_Wind_VFX", section, new Vector3(70f, 0.15f, 176f), Quaternion.identity, Vector3.one);
    }

    private static void BuildVerdantWilds(Transform root)
    {
        Transform section = CreateSection(root, "03_Verdant_Wilds", new Vector3(-70f, 0f, 190f), 4, 4, false);
        CreateLabel(section, "VERDANT WILDS", new Vector3(-70f, 0.1f, 224f), 3.2f);
        PlaceForest(section, new Vector3(-70f, 0f, 190f), 16);
        PlaceModel(StatuePath, "Wilds_Hidden_Statue", section, new Vector3(-58f, 0f, 200f), -40f, Vector3.one * 105f);
        PlaceCoins(section, new Vector3(-82f, 0.4f, 182f), 6, 2f);
        PlacePrefab(LeavesVfxPath, "Wilds_Leaves_VFX", section, new Vector3(-70f, 0.1f, 190f), Quaternion.identity, Vector3.one * 0.01f);
    }

    private static void BuildRadiantJungle(Transform root)
    {
        Transform section = CreateSection(root, "04_Radiant_Jungle", new Vector3(-120f, 0f, 120f), 3, 4, false);
        CreateLabel(section, "RADIANT JUNGLE", new Vector3(-120f, 0.1f, 152f), 3f);
        PlaceForest(section, new Vector3(-120f, 0f, 120f), 18);
        PlaceModel(ArcSidePath, "Overgrown_Arch_Left", section, new Vector3(-136f, 0f, 128f), 45f, ModelScale);
        PlaceModel(ArcSidePath, "Overgrown_Arch_Right", section, new Vector3(-104f, 0f, 136f), -135f, ModelScale);
        PlacePrefab(LeavesVfxPath, "Radiant_Jungle_Leaves", section, new Vector3(-120f, 0.1f, 130f), Quaternion.identity, Vector3.one * 0.01f);
    }

    private static void BuildJungleRuins(Transform root)
    {
        Transform section = CreateSection(root, "05_Jungle_Ruins", new Vector3(-122f, 0f, 250f), 4, 4, true);
        CreateLabel(section, "JUNGLE RUINS", new Vector3(-122f, 0.1f, 286f), 3.2f);
        PlaceForest(section, new Vector3(-122f, 0f, 250f), 8);
        PlaceModel(ArcMidPath, "Ruined_Arch", section, new Vector3(-122f, 0f, 250f), 180f, ModelScale);
        PlaceModel(PillarShortPath, "Collapsed_Pillar_01", section, new Vector3(-137f, 0f, 238f), 20f, ModelScale);
        PlaceModel(PillarShortPath, "Collapsed_Pillar_02", section, new Vector3(-105f, 0f, 264f), -20f, ModelScale);
        PlaceRockCluster(section, new Vector3(-145f, -1.2f, 270f), 6, 1);
        PlaceCoins(section, new Vector3(-122f, 0.4f, 264f), 7, 2.2f);
    }

    private static void BuildNorthernHighRoad(Transform root)
    {
        Transform section = CreateEmpty(root, "06_Northern_High_Road");
        CreateLabel(section, "THE NORTHERN HIGH ROAD", new Vector3(40f, 0.1f, 250f), 3f);
        PlaceRoad(section, new Vector3(-4f, 0f, 214f), new Vector3(98f, 0f, 278f), 10);
        PlaceModel(StairsShortPath, "High_Road_Stairs_01", section, new Vector3(18f, 0f, 228f), 35f, ModelScale);
        PlaceModel(StairsHighPath, "High_Road_Stairs_02", section, new Vector3(70f, 0f, 260f), 35f, ModelScale);
        PlaceModel(PillarPath, "Road_Pillar_01", section, new Vector3(6f, 0f, 220f), 0f, ModelScale);
        PlaceModel(PillarPath, "Road_Pillar_02", section, new Vector3(88f, 0f, 270f), 0f, ModelScale);
        PlacePrefab(GatePrefabPath, "Visual_Gate_To_Dire_Peaks", section, new Vector3(102f, 0f, 280f), Quaternion.identity, Vector3.one);
    }

    private static void BuildDirePeaks(Transform root)
    {
        Transform section = CreateSection(root, "07_Dire_Peaks", new Vector3(132f, 0f, 315f), 4, 4, false);
        CreateLabel(section, "THE DIRE PEAKS", new Vector3(132f, 0.1f, 352f), 3.4f);
        PlaceRockCluster(section, new Vector3(114f, -2f, 308f), 9, 2);
        PlaceRockCluster(section, new Vector3(139f, -2f, 322f), 10, 4);
        PlaceRockCluster(section, new Vector3(155f, -2f, 300f), 8, 6);
        PlaceModel(StatuePath, "Peak_Shrine_Statue", section, new Vector3(132f, 0f, 315f), 180f, Vector3.one * 110f);
        PlacePrefab(SmokeVfxPath, "Peak_Mist_Smoke", section, new Vector3(132f, 0.1f, 315f), Quaternion.identity, Vector3.one);
    }

    private static void BuildBlightlands(Transform root)
    {
        Transform section = CreateSection(root, "08_Blightlands", new Vector3(174f, 0f, 190f), 4, 4, false);
        CreateLabel(section, "THE BLIGHTLANDS", new Vector3(174f, 0.1f, 224f), 3.2f);
        PlaceForest(section, new Vector3(174f, 0f, 190f), 10);
        PlaceRockCluster(section, new Vector3(172f, -1.5f, 190f), 7, 5);
        PlaceLava(section, new Vector3(180f, 0.02f, 178f), new Vector3(34f, 0.04f, 4f), "Blight_Crack_01");
        PlaceLava(section, new Vector3(165f, 0.02f, 204f), new Vector3(26f, 0.04f, 4f), "Blight_Crack_02");
        PlacePrefab(SmokeVfxPath, "Blight_Smoke", section, new Vector3(174f, 0.1f, 190f), Quaternion.identity, Vector3.one);
    }

    private static void BuildDireStronghold(Transform root)
    {
        Transform section = CreateSection(root, "09_Dire_Stronghold", new Vector3(216f, 0f, 282f), 5, 5, true);
        CreateLabel(section, "DIRE STRONGHOLD", new Vector3(216f, 0.1f, 326f), 3.5f);
        PlacePrefab(GateWidePath, "Stronghold_Main_Gate", section, new Vector3(216f, 0f, 250f), Quaternion.identity, ModelScale);
        PlacePrefab(GatePrefabPath, "Progression_Gate_Stronghold", section, new Vector3(216f, 0f, 262f), Quaternion.identity, Vector3.one);
        PlaceModel(ArcMidPath, "Stronghold_Inner_Arch", section, new Vector3(216f, 0f, 282f), 0f, ModelScale);
        PlaceModel(PillarPath, "Stronghold_Tower_Left", section, new Vector3(184f, 0f, 282f), 0f, Vector3.one * 135f);
        PlaceModel(PillarPath, "Stronghold_Tower_Right", section, new Vector3(248f, 0f, 282f), 0f, Vector3.one * 135f);
        PlaceLava(section, new Vector3(216f, 0.02f, 304f), new Vector3(58f, 0.04f, 5f), "Stronghold_Lava_Rift");
        PlaceLava(section, new Vector3(236f, 0.02f, 280f), new Vector3(5f, 0.04f, 45f), "Stronghold_Lava_Side_Rift");
        PlaceRockCluster(section, new Vector3(248f, -1.5f, 314f), 7, 7);
        PlacePrefab(SmokeVfxPath, "Stronghold_Smoke", section, new Vector3(216f, 0.1f, 290f), Quaternion.identity, Vector3.one);
    }

    private static void BuildFinalLandmark(Transform root)
    {
        Transform section = CreateEmpty(root, "10_Final_Landmark_Exit");
        CreateLabel(section, "NEXT REGION / STORY EXIT", new Vector3(216f, 0.1f, 360f), 3f);
        PlaceRoad(section, new Vector3(216f, 0f, 326f), new Vector3(216f, 0f, 366f), 4);
        PlaceModel(StatuePath, "Exit_Statue", section, new Vector3(216f, 0f, 368f), 180f, Vector3.one * 130f);
        PlaceModel(LampPath, "Exit_Lamp_Left", section, new Vector3(208f, 0f, 360f), 0f, ModelScale);
        PlaceModel(LampPath, "Exit_Lamp_Right", section, new Vector3(224f, 0f, 360f), 0f, ModelScale);
    }

    private static Transform CreateSection(Transform root, string name, Vector3 center, int widthTiles, int lengthTiles, bool walls)
    {
        Transform section = CreateEmpty(root, name);
        PlaceFloorGrid(section, center, widthTiles, lengthTiles);
        if (walls)
            PlaceBoundaryWalls(section, center, widthTiles, lengthTiles);
        return section;
    }

    private static Transform CreateEmpty(Transform root, string name)
    {
        GameObject section = new GameObject(name);
        SceneManager.MoveGameObjectToScene(section, root.gameObject.scene);
        section.transform.SetParent(root);
        section.transform.position = Vector3.zero;
        return section.transform;
    }

    private static void PlaceFloorGrid(Transform parent, Vector3 center, int widthTiles, int lengthTiles)
    {
        const float tile = 16f;
        float startX = center.x - (widthTiles - 1) * tile * 0.5f;
        float startZ = center.z - (lengthTiles - 1) * tile * 0.5f;

        for (int x = 0; x < widthTiles; x++)
        {
            for (int z = 0; z < lengthTiles; z++)
            {
                Vector3 position = new Vector3(startX + x * tile, center.y, startZ + z * tile);
                PlaceModel(FloorPath, $"Floor_{x}_{z}", parent, position, 0f, ModelScale);
            }
        }
    }

    private static void PlaceBoundaryWalls(Transform parent, Vector3 center, int widthTiles, int lengthTiles)
    {
        const float tile = 16f;
        const float step = 4f;
        float minX = center.x - widthTiles * tile * 0.5f;
        float maxX = center.x + widthTiles * tile * 0.5f;
        float minZ = center.z - lengthTiles * tile * 0.5f;
        float maxZ = center.z + lengthTiles * tile * 0.5f;

        for (float x = minX + 2f; x <= maxX - 2f; x += step)
        {
            if (Mathf.Abs(x - center.x) > 5f)
            {
                PlaceModel(Wall4Path, "Wall_North", parent, new Vector3(x, center.y, maxZ), 0f, ModelScale);
                PlaceModel(Wall4Path, "Wall_South", parent, new Vector3(x, center.y, minZ), 0f, ModelScale);
            }
        }

        for (float z = minZ + 2f; z <= maxZ - 2f; z += step)
        {
            PlaceModel(Wall2Path, "Wall_West", parent, new Vector3(minX, center.y, z), 90f, ModelScale);
            PlaceModel(Wall2Path, "Wall_East", parent, new Vector3(maxX, center.y, z), 90f, ModelScale);
        }
    }

    private static void PlaceRoad(Transform parent, Vector3 start, Vector3 end, int tileCount)
    {
        Vector3 direction = (end - start).normalized;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        for (int i = 0; i < tileCount; i++)
        {
            float t = tileCount == 1 ? 0f : i / (float)(tileCount - 1);
            Vector3 position = Vector3.Lerp(start, end, t);
            PlaceModel(FloorPath, $"Road_{i + 1}", parent, position, angle, ModelScale);
        }
    }

    private static void PlaceBridge(Transform parent, Vector3 center, int lengthTiles)
    {
        for (int i = 0; i < lengthTiles; i++)
        {
            Vector3 position = center + new Vector3((i - lengthTiles * 0.5f) * 8f, 0.15f, 0f);
            PlaceModel(FloorPath, $"Bridge_Floor_{i + 1}", parent, position, 90f, ModelScale);
            PlaceModel(Wall2Path, $"Bridge_Rail_N_{i + 1}", parent, position + new Vector3(0f, 0.35f, 5f), 90f, Vector3.one * 70f);
            PlaceModel(Wall2Path, $"Bridge_Rail_S_{i + 1}", parent, position + new Vector3(0f, 0.35f, -5f), 90f, Vector3.one * 70f);
        }
    }

    private static void PlaceForest(Transform parent, Vector3 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = i * 137.5f;
            float radius = 8f + (i % 5) * 5f;
            Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            PlaceModel(TreePath, $"Tree_{i + 1}", parent, position, angle, Vector3.one * (95f + (i % 4) * 12f));
        }
    }

    private static void PlaceRockCluster(Transform parent, Vector3 center, int count, int offset)
    {
        string[] rockPaths = { RockLargePath, RockSmall01Path, RockSmall02Path, RockSmall03Path };
        for (int i = 0; i < count; i++)
        {
            float angle = (i + offset) * 91f;
            float radius = 3f + (i % 4) * 3.5f;
            Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            float scale = i % 3 == 0 ? 330f : 190f + (i % 3) * 35f;
            PlaceModel(rockPaths[i % rockPaths.Length], $"Rock_{i + 1}", parent, position, angle, Vector3.one * scale);
        }
    }

    private static void PlaceWater(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        PlacePrimitive(parent, name, position, scale, LoadMaterial(WaterMaterialPath));
    }

    private static void PlaceLava(Transform parent, Vector3 position, Vector3 scale, string name)
    {
        PlacePrimitive(parent, name, position, scale, LoadMaterial(LavaMaterialPath));
    }

    private static void PlacePrimitive(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        SceneManager.MoveGameObjectToScene(primitive, parent.gameObject.scene);
        primitive.name = name;
        primitive.transform.SetParent(parent);
        primitive.transform.position = position;
        primitive.transform.localScale = scale;
        if (material != null)
            primitive.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(primitive.GetComponent<Collider>());
    }

    private static void PlaceCoins(Transform parent, Vector3 center, int count, float spacing)
    {
        float start = -(count - 1) * spacing * 0.5f;
        for (int i = 0; i < count; i++)
        {
            Vector3 position = center + new Vector3(start + i * spacing, 0f, 0f);
            PlacePrefab(CoinPrefabPath, $"Coin_{i + 1}", parent, position, Quaternion.identity, Vector3.one);
        }
    }

    private static void CreateLabel(Transform parent, string text, Vector3 position, float size)
    {
        GameObject label = new GameObject($"Label_{text}");
        SceneManager.MoveGameObjectToScene(label, parent.gameObject.scene);
        label.transform.SetParent(parent);
        label.transform.position = position;
        label.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = size;
        textMesh.fontSize = 42;
        textMesh.color = new Color(0.95f, 0.86f, 0.58f, 1f);
    }

    private static GameObject PlaceModel(string path, string name, Transform parent, Vector3 position, float yRotation, Vector3 scale)
    {
        return PlacePrefab(path, name, parent, position, Quaternion.Euler(-90f, yRotation, 0f), scale);
    }

    private static GameObject PlacePrefab(string path, string name, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogWarning($"Missing world asset: {path}");
            return null;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(asset, parent.gameObject.scene) as GameObject;
        if (instance == null)
            return null;

        instance.name = name;
        instance.transform.SetParent(parent);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
        GameObjectUtility.SetStaticEditorFlags(instance, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        return instance;
    }

    private static Material LoadMaterial(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }
}
