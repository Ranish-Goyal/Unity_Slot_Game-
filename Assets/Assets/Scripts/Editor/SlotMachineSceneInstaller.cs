using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
internal static class SlotMachineSceneInstaller
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ArtworkPath = "Assets/Assets/Arts/ProvidedAssets/";

    private static readonly string[] SpriteAssets =
    {
        "slot-machine1.png",
        "slot-symbol1.png",
        "slot-symbol2.png",
        "slot-symbol3.png",
        "slot-symbol4.png",
        "slot_machine_Middle_box.png",
        "slot_machine_buttons-03.png",
        "slot_machine_buttons-04.png",
        "bg_gradient.png"
    };

    private static readonly string[] SymbolSpriteNames =
    {
        "slot-symbol1_0",
        "slot-symbol2_0",
        "slot-symbol3_0",
        "slot-symbol4_0"
    };

    private static readonly string[] SymbolNames = { "7", "CHERRY", "BELL", "BAR" };
    private static readonly int[] PayoutMultipliers = { 25, 12, 8, 5 };

    static SlotMachineSceneInstaller()
    {
        EditorApplication.delayCall += EnsureSampleSceneIsOpen;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void EnsureSampleSceneIsOpen()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && !string.IsNullOrEmpty(activeScene.path))
        {
            InstallInActiveSampleScene();
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
        {
            Debug.LogError("The slot-machine sample scene could not be found at " + ScenePath);
            return;
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        InstallInActiveSampleScene();
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.path == ScenePath)
        {
            EditorApplication.delayCall += InstallInActiveSampleScene;
        }
    }

    private static void InstallInActiveSampleScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            return;
        }

        for (int i = 0; i < SpriteAssets.Length; i++)
        {
            string assetPath = ArtworkPath + SpriteAssets[i];
            if (!File.Exists(assetPath))
            {
                Debug.LogError("Required slot-machine artwork was not found: " + assetPath);
                return;
            }
        }

        Sprite cabinet = LoadSprite(SpriteAssets[0], "slot-machine1_0");
        Sprite[] symbols = new Sprite[SymbolSpriteNames.Length];
        for (int i = 0; i < symbols.Length; i++)
        {
            symbols[i] = LoadSprite(
                "slot-symbol" + (i + 1) + ".png",
                SymbolSpriteNames[i]);
        }

        if (cabinet == null || symbols.Any(sprite => sprite == null))
        {
            Debug.LogError("Could not load the sliced slot-machine cabinet and reel-symbol artwork.");
            return;
        }

        Sprite background = LoadSprite("bg_gradient.png", "bg_gradient_0");
        Sprite controlPanel = LoadSprite(
            "slot_machine_Middle_box.png",
            "slot_machine_Middle_box_0");
        Sprite[] decreaseBetButtons = LoadButtonSprites("slot_machine_buttons-04.png");
        Sprite[] increaseBetButtons = LoadButtonSprites("slot_machine_buttons-03.png");

        if (background == null || controlPanel == null ||
            decreaseBetButtons.Any(sprite => sprite == null) ||
            increaseBetButtons.Any(sprite => sprite == null))
        {
            Debug.LogError("Could not load the sliced slot-machine background, control panel, or button artwork.");
            return;
        }

        GameObject controllerObject = GameObject.Find("Slot Machine Game");
        bool changed = false;
        if (controllerObject == null)
        {
            controllerObject = new GameObject("Slot Machine Game");
            Undo.RegisterCreatedObjectUndo(controllerObject, "Install playable slot machine");
            changed = true;
        }

        SlotMachineGame.SlotMachineController controller =
            controllerObject.GetComponent<SlotMachineGame.SlotMachineController>();
        if (controller == null)
        {
            controller = Undo.AddComponent<SlotMachineGame.SlotMachineController>(controllerObject);
            changed = true;
        }

        SerializedObject serializedController = new SerializedObject(controller);
        SerializedProperty payouts = serializedController.FindProperty("symbolPayouts");
        if (payouts == null)
        {
            Debug.LogError("The slot-machine payout settings could not be configured.");
            return;
        }

        bool needsSceneSave = changed;
        needsSceneSave |= SetSprite(serializedController, "backgroundSprite", background);
        needsSceneSave |= SetSprite(serializedController, "cabinetSprite", cabinet);
        needsSceneSave |= SetSprite(serializedController, "controlPanelSprite", controlPanel);
        needsSceneSave |= SetSpriteArray(
            serializedController,
            "decreaseBetButtonSprites",
            decreaseBetButtons);
        needsSceneSave |= SetSpriteArray(
            serializedController,
            "increaseBetButtonSprites",
            increaseBetButtons);
        needsSceneSave |= SetSpriteArray(
            serializedController,
            "spinButtonSprites",
            increaseBetButtons);

        bool needsSymbols = payouts.arraySize != symbols.Length;
        for (int i = 0; !needsSymbols && i < symbols.Length; i++)
        {
            SerializedProperty entry = payouts.GetArrayElementAtIndex(i);
            needsSymbols = entry.FindPropertyRelative("sprite").objectReferenceValue == null;
        }

        if (needsSymbols)
        {
            payouts.arraySize = symbols.Length;
            for (int i = 0; i < symbols.Length; i++)
            {
                SerializedProperty entry = payouts.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("sprite").objectReferenceValue = symbols[i];
                entry.FindPropertyRelative("displayName").stringValue = SymbolNames[i];
                entry.FindPropertyRelative("tripleMatchMultiplier").intValue = PayoutMultipliers[i];
            }

            needsSceneSave = true;
        }

        if (serializedController.ApplyModifiedPropertiesWithoutUndo())
        {
            needsSceneSave = true;
        }

        Camera gameCamera = Object.FindFirstObjectByType<Camera>();
        if (gameCamera != null && gameCamera.backgroundColor != new Color(0.055f, 0.045f, 0.13f, 1f))
        {
            Undo.RecordObject(gameCamera, "Set slot-machine camera background");
            gameCamera.backgroundColor = new Color(0.055f, 0.045f, 0.13f, 1f);
            needsSceneSave = true;
        }

        if (needsSceneSave)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    private static bool SetSprite(
        SerializedObject target,
        string propertyName,
        Sprite sprite)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property == null || property.objectReferenceValue == sprite)
        {
            return false;
        }

        property.objectReferenceValue = sprite;
        return true;
    }

    private static bool SetSpriteArray(
        SerializedObject target,
        string propertyName,
        Sprite[] sprites)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogError("Could not find the configured slot-machine button sprites: " + propertyName);
            return false;
        }

        bool changed = property.arraySize != sprites.Length;
        if (changed)
        {
            property.arraySize = sprites.Length;
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            SerializedProperty entry = property.GetArrayElementAtIndex(i);
            if (entry.objectReferenceValue != sprites[i])
            {
                entry.objectReferenceValue = sprites[i];
                changed = true;
            }
        }

        return changed;
    }

    private static Sprite[] LoadButtonSprites(string fileName)
    {
        return Enumerable.Range(0, 4)
            .Select(index => LoadSprite(fileName, Path.GetFileNameWithoutExtension(fileName) + "_" + index))
            .ToArray();
    }

    private static Sprite LoadSprite(string fileName, string spriteName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ArtworkPath + fileName)
            .OfType<Sprite>()
            .FirstOrDefault(sprite => sprite.name == spriteName);
    }
}
