#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Upgrades
{
    /// <summary>
    /// Creates and edits an Upgrade ScriptableObject and its BasicStatUpgrade prefab together.
    /// Place this file anywhere inside an Editor folder, for example:
    /// Assets/Editor/UpgradePairEditorWindow.cs
    /// </summary>
    public sealed class UpgradePairEditorWindow : EditorWindow
    {
        private const string DefaultOutputFolder = "Assets/Upgrades";

        [SerializeField] private string outputFolder = DefaultOutputFolder;
        [SerializeField] private string newUpgradeName = "New Upgrade";
        [SerializeField] private string newDescription = string.Empty;
        [SerializeField] private float newCost;
        [SerializeField] private Upgrade selectedUpgrade;
        [SerializeField] private Vector2 scrollPosition;

        // Hidden component used as the editable template for newly-created prefabs.
        private GameObject draftGameObject;
        private BasicStatUpgrade draftComponent;
        private SerializedObject draftSerializedObject;

        // Serialized objects for the currently-selected upgrade pair.
        private SerializedObject upgradeSerializedObject;
        private GameObject prefabAsset;
        private BasicStatUpgrade prefabUpgradeComponent;
        private SerializedObject prefabSerializedObject;

        [MenuItem("Tools/Upgrades/Upgrade Pair Editor")]
        private static void OpenWindow()
        {
            UpgradePairEditorWindow window = GetWindow<UpgradePairEditorWindow>();
            window.titleContent = new GUIContent("Upgrade Editor");
            window.minSize = new Vector2(440f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            EnsureDraftComponent();
            BindUpgrade(selectedUpgrade);
        }

        private void OnDisable()
        {
            if (draftGameObject != null)
            {
                DestroyImmediate(draftGameObject);
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is Upgrade upgrade)
            {
                BindUpgrade(upgrade);
                Repaint();
            }
        }

        private void OnGUI()
        {
            EnsureDraftComponent();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawCreateSection();
            EditorGUILayout.Space(14f);
            DrawEditSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawCreateSection()
        {
            EditorGUILayout.LabelField("Create Upgrade Pair", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Creates one Upgrade .asset and one linked prefab containing BasicStatUpgrade.",
                MessageType.Info);

            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
            newUpgradeName = EditorGUILayout.TextField("Upgrade Name", newUpgradeName);

            EditorGUILayout.LabelField("Description");
            newDescription = EditorGUILayout.TextArea(newDescription, GUILayout.MinHeight(64f));

            newCost = EditorGUILayout.FloatField("Cost", newCost);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);

            draftSerializedObject.Update();
            DrawSerializedProperty(draftSerializedObject, "factor", "Factor");
            DrawSerializedProperty(draftSerializedObject, "stat", "Player Stat");
            DrawSerializedProperty(draftSerializedObject, "type", "Modifier Type");
            draftSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newUpgradeName)))
            {
                if (GUILayout.Button("Create Upgrade Asset + Prefab", GUILayout.Height(30f)))
                {
                    CreateUpgradePair();
                }
            }
        }

        private void DrawEditSection()
        {
            EditorGUILayout.LabelField("Edit Existing Upgrade Pair", EditorStyles.boldLabel);

            Upgrade pickedUpgrade = (Upgrade)EditorGUILayout.ObjectField(
                "Upgrade Asset",
                selectedUpgrade,
                typeof(Upgrade),
                false);

            if (pickedUpgrade != selectedUpgrade)
            {
                BindUpgrade(pickedUpgrade);
            }

            if (selectedUpgrade == null)
            {
                EditorGUILayout.HelpBox(
                    "Choose an Upgrade asset here, or select one in the Project window.",
                    MessageType.None);
                return;
            }

            DrawUpgradeAssetFields();
            EditorGUILayout.Space(8f);
            DrawPrefabFields();
            EditorGUILayout.Space(10f);
            DrawActionButtons();
        }

        private void DrawUpgradeAssetFields()
        {
            if (upgradeSerializedObject == null)
            {
                BindUpgrade(selectedUpgrade);
            }

            EditorGUILayout.LabelField("Upgrade Asset", EditorStyles.boldLabel);

            upgradeSerializedObject.Update();

            SerializedProperty upgradeNameProperty =
                upgradeSerializedObject.FindProperty("upgradeName");
            SerializedProperty descriptionProperty =
                upgradeSerializedObject.FindProperty("description");
            SerializedProperty costProperty =
                upgradeSerializedObject.FindProperty("cost");
            SerializedProperty prefabProperty =
                upgradeSerializedObject.FindProperty("upgradePrefab");

            GameObject prefabBeforeEdit =
                prefabProperty != null ? prefabProperty.objectReferenceValue as GameObject : null;

            if (upgradeNameProperty != null)
            {
                EditorGUILayout.PropertyField(upgradeNameProperty, new GUIContent("Upgrade Name"));
            }

            if (descriptionProperty != null)
            {
                EditorGUILayout.LabelField("Description");
                descriptionProperty.stringValue = EditorGUILayout.TextArea(
                    descriptionProperty.stringValue,
                    GUILayout.MinHeight(64f));
            }

            if (costProperty != null)
            {
                EditorGUILayout.PropertyField(costProperty, new GUIContent("Cost"));
            }

            if (prefabProperty != null)
            {
                EditorGUILayout.PropertyField(prefabProperty, new GUIContent("Upgrade Prefab"));
            }

            if (upgradeSerializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(selectedUpgrade);
            }

            if (prefabBeforeEdit != selectedUpgrade.upgradePrefab)
            {
                BindPrefab();
            }
        }

        private void DrawPrefabFields()
        {
            EditorGUILayout.LabelField("Prefab Component", EditorStyles.boldLabel);

            if (selectedUpgrade.upgradePrefab == null)
            {
                EditorGUILayout.HelpBox(
                    "This Upgrade asset does not have a linked prefab.",
                    MessageType.Warning);

                if (GUILayout.Button("Create and Link Missing Prefab"))
                {
                    CreateMissingPrefab();
                }

                return;
            }

            if (prefabAsset != selectedUpgrade.upgradePrefab)
            {
                BindPrefab();
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Editing Prefab",
                    prefabAsset,
                    typeof(GameObject),
                    false);
            }

            if (prefabUpgradeComponent == null)
            {
                EditorGUILayout.HelpBox(
                    "The linked prefab does not contain a BasicStatUpgrade component.",
                    MessageType.Warning);

                if (GUILayout.Button("Add BasicStatUpgrade to Prefab Root"))
                {
                    AddComponentToLinkedPrefab();
                }

                return;
            }

            prefabSerializedObject.Update();
            DrawSerializedProperty(prefabSerializedObject, "factor", "Factor");
            DrawSerializedProperty(prefabSerializedObject, "stat", "Player Stat");
            DrawSerializedProperty(prefabSerializedObject, "type", "Modifier Type");

            if (prefabSerializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(prefabUpgradeComponent);
                PrefabUtility.SavePrefabAsset(prefabAsset);
            }
        }

        private void DrawActionButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save All"))
                {
                    SaveCurrentPair();
                }

                if (GUILayout.Button("Ping Asset"))
                {
                    EditorGUIUtility.PingObject(selectedUpgrade);
                }

                using (new EditorGUI.DisabledScope(prefabAsset == null))
                {
                    if (GUILayout.Button("Open Prefab"))
                    {
                        AssetDatabase.OpenAsset(prefabAsset);
                    }
                }
            }
        }

        private void CreateUpgradePair()
        {
            string normalizedFolder = NormalizeAssetFolder(outputFolder);
            if (!IsAssetsFolder(normalizedFolder))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Output Folder",
                    "The output folder must be inside Assets, for example Assets/Upgrades.",
                    "OK");
                return;
            }

            if (!EnsureAssetFolderExists(normalizedFolder))
            {
                EditorUtility.DisplayDialog(
                    "Could Not Create Folder",
                    $"Unity could not create the folder '{normalizedFolder}'.",
                    "OK");
                return;
            }

            string displayName = newUpgradeName.Trim();
            string baseFileName = SanitizeFileName(displayName);
            string uniqueStem = FindUniquePairStem(normalizedFolder, baseFileName);
            string prefabPath = $"{normalizedFolder}/{uniqueStem}.prefab";
            string assetPath = $"{normalizedFolder}/{uniqueStem}.asset";

            GameObject temporaryObject = new GameObject(uniqueStem);

            try
            {
                BasicStatUpgrade component = temporaryObject.AddComponent<BasicStatUpgrade>();
                EditorUtility.CopySerialized(draftComponent, component);

                GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(
                    temporaryObject,
                    prefabPath,
                    out bool prefabSaved);

                if (!prefabSaved || createdPrefab == null)
                {
                    throw new InvalidOperationException("Unity failed to save the upgrade prefab.");
                }

                Upgrade createdUpgrade = ScriptableObject.CreateInstance<Upgrade>();
                createdUpgrade.upgradeName = displayName;
                createdUpgrade.description = newDescription;
                createdUpgrade.cost = newCost;
                createdUpgrade.upgradePrefab = createdPrefab;

                AssetDatabase.CreateAsset(createdUpgrade, assetPath);
                EditorUtility.SetDirty(createdUpgrade);
                AssetDatabase.SaveAssets();

                outputFolder = normalizedFolder;
                BindUpgrade(createdUpgrade);
                Selection.activeObject = createdUpgrade;
                EditorGUIUtility.PingObject(createdUpgrade);
            }
            catch (Exception exception)
            {
                // Both paths were guaranteed to be unused before creation, so cleanup is safe.
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.DeleteAsset(prefabPath);

                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Upgrade Creation Failed",
                    exception.Message,
                    "OK");
            }
            finally
            {
                DestroyImmediate(temporaryObject);
            }
        }

        private void CreateMissingPrefab()
        {
            string upgradeAssetPath = AssetDatabase.GetAssetPath(selectedUpgrade);
            if (string.IsNullOrEmpty(upgradeAssetPath))
            {
                EditorUtility.DisplayDialog(
                    "Unsaved Upgrade Asset",
                    "Save the Upgrade asset in the project before creating its prefab.",
                    "OK");
                return;
            }

            string folder = Path.GetDirectoryName(upgradeAssetPath)?.Replace('\\', '/');
            string stem = Path.GetFileNameWithoutExtension(upgradeAssetPath);
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{stem}.prefab");
            GameObject temporaryObject = new GameObject(stem);

            try
            {
                temporaryObject.AddComponent<BasicStatUpgrade>();

                GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(
                    temporaryObject,
                    prefabPath,
                    out bool prefabSaved);

                if (!prefabSaved || createdPrefab == null)
                {
                    throw new InvalidOperationException("Unity failed to save the upgrade prefab.");
                }

                Undo.RecordObject(selectedUpgrade, "Link upgrade prefab");
                selectedUpgrade.upgradePrefab = createdPrefab;
                EditorUtility.SetDirty(selectedUpgrade);
                AssetDatabase.SaveAssets();
                BindPrefab();
            }
            catch (Exception exception)
            {
                AssetDatabase.DeleteAsset(prefabPath);
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Prefab Creation Failed", exception.Message, "OK");
            }
            finally
            {
                DestroyImmediate(temporaryObject);
            }
        }

        private void AddComponentToLinkedPrefab()
        {
            if (prefabAsset == null)
            {
                return;
            }

            if (PrefabUtility.GetPrefabAssetType(prefabAsset) == PrefabAssetType.Model)
            {
                EditorUtility.DisplayDialog(
                    "Prefab Is Read-Only",
                    "A BasicStatUpgrade component cannot be added directly to a model prefab. " +
                    "Create a regular prefab or prefab variant first.",
                    "OK");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            GameObject prefabContents = null;

            try
            {
                prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

                if (prefabContents.GetComponent<BasicStatUpgrade>() == null)
                {
                    prefabContents.AddComponent<BasicStatUpgrade>();
                }

                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Could Not Edit Prefab", exception.Message, "OK");
            }
            finally
            {
                if (prefabContents != null)
                {
                    PrefabUtility.UnloadPrefabContents(prefabContents);
                }
            }

            AssetDatabase.ImportAsset(prefabPath);
            BindPrefab();
        }

        private void SaveCurrentPair()
        {
            if (selectedUpgrade != null)
            {
                EditorUtility.SetDirty(selectedUpgrade);
            }

            if (prefabAsset != null && PrefabUtility.IsPartOfPrefabAsset(prefabAsset))
            {
                PrefabUtility.SavePrefabAsset(prefabAsset);
            }

            AssetDatabase.SaveAssets();
        }

        private void BindUpgrade(Upgrade upgrade)
        {
            selectedUpgrade = upgrade;
            upgradeSerializedObject =
                selectedUpgrade != null ? new SerializedObject(selectedUpgrade) : null;
            BindPrefab();
        }

        private void BindPrefab()
        {
            prefabAsset = selectedUpgrade != null ? selectedUpgrade.upgradePrefab : null;
            prefabUpgradeComponent = prefabAsset != null
                ? prefabAsset.GetComponent<BasicStatUpgrade>()
                : null;
            prefabSerializedObject = prefabUpgradeComponent != null
                ? new SerializedObject(prefabUpgradeComponent)
                : null;
        }

        private void EnsureDraftComponent()
        {
            if (draftGameObject != null && draftComponent != null && draftSerializedObject != null)
            {
                return;
            }

            draftGameObject = new GameObject("Upgrade Prefab Draft")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            draftComponent = draftGameObject.AddComponent<BasicStatUpgrade>();

            // Keep the editor-only template from running BasicStatUpgrade.Start in Play Mode.
            // The component itself stays enabled, so its enabled state is copied to new prefabs.
            draftGameObject.SetActive(false);

            draftSerializedObject = new SerializedObject(draftComponent);
        }

        private static void DrawSerializedProperty(
            SerializedObject serializedObject,
            string propertyName,
            string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                EditorGUILayout.HelpBox(
                    $"Could not find the serialized field '{propertyName}'. " +
                    "Update UpgradePairEditorWindow if the runtime field was renamed.",
                    MessageType.Error);
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }

        private static string NormalizeAssetFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return DefaultOutputFolder;
            }

            return folder.Trim().Replace('\\', '/').TrimEnd('/');
        }

        private static bool IsAssetsFolder(string folder)
        {
            return folder == "Assets" ||
                   folder.StartsWith("Assets/", StringComparison.Ordinal);
        }

        private static bool EnsureAssetFolderExists(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return true;
            }

            string[] parts = folder.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                return false;
            }

            string currentPath = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = $"{currentPath}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    string guid = AssetDatabase.CreateFolder(currentPath, parts[i]);
                    if (string.IsNullOrEmpty(guid))
                    {
                        return false;
                    }
                }

                currentPath = nextPath;
            }

            return AssetDatabase.IsValidFolder(folder);
        }

        private static string FindUniquePairStem(string folder, string baseStem)
        {
            string candidate = baseStem;
            int suffix = 1;

            while (AssetDatabase.LoadMainAssetAtPath($"{folder}/{candidate}.asset") != null ||
                   AssetDatabase.LoadMainAssetAtPath($"{folder}/{candidate}.prefab") != null)
            {
                candidate = $"{baseStem} {suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string SanitizeFileName(string value)
        {
            string result = value;
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(invalidCharacter, '_');
            }

            result = result.Trim().Trim('.');
            return string.IsNullOrEmpty(result) ? "New Upgrade" : result;
        }
    }
}
#endif
