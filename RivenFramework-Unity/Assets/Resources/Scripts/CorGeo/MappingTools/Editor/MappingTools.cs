using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class MappingTools : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    
    //[SerializeField] private List<GameObject> m_RequiredLevelObjects = new List<GameObject>();
    //[SerializeField] private List<AssetLabelEntry> m_AssetLabels = new List<AssetLabelEntry>();

    private MappingToolsSettings m_Settings;
    
    private VisualElement m_StatusSection;
    private VisualElement m_AssetsSection;
    private VisualElement m_SettingsSection;
    private VisualElement m_HelpSection;
    
    // Issues tab
    private bool m_NeedsBake = true;
    private Label m_LevelReadyLabel;
    private List<LevelIssue> m_Issues = new List<LevelIssue>();
    private Label m_SceneNameLabel;
    private HelpBox m_LevelStatusHelpBox;
    private VisualElement m_IssuesContainer;
    private Button m_FixAllButton;
    private const string VoxelOccluderLayerName = "Voxel Occluder";
    private ProgressBar m_BakeProgressBar;
    private bool m_IsBaking = false;

    // Mapper assets tab
    private Toolbar m_AssetLabelsToolbar;
    private enum AssetViewMode { Grid, List };
    private AssetViewMode m_ViewMode = AssetViewMode.Grid;
    private List<Object> m_AssetResults = new List<Object>();
    private ListView m_AssetsResultsList;
    private ScrollView m_AssetResultGridScroll;
    private VisualElement m_AssetResultGrid;
    private HashSet<int> m_ResolvedPreviewIndices = new HashSet<int>();
    private HashSet<int> m_SelectedGridIndices = new HashSet<int>();
    private int m_GridSelectionAnchor = -1;
    private Vector2 m_ListDragStartPos;
    private Vector2 m_GridDragStartPos;
    private ToolbarSearchField m_AssetSearchField;
    private List<Object> m_FilteredAssetResults = new List<Object>();
    private string m_CurrentAssetLabel;
    private Dictionary<string, Button> m_AssetLabelButtons = new Dictionary<string, Button>();
    private const string LastViewModeSessionKey = "MappingTools_LastViewMode";
    private const string LastLabelSessionKey = "MappingTools_LastLabel";
    private enum AssetNameDisplayMode { ID, Display }
    private AssetNameDisplayMode m_NameDisplayMode = AssetNameDisplayMode.ID;

    // Settings tab
    private const string SettingsAssetPath = "Assets/Resources/Scripts/CorGeo/MappingTools/MappingToolsSettings.asset";

    // Editor prefs keys
    private const string EditorPrefsKey_Root = "Neverway_MappingTools_";
    private const string EditorPrefsKey_NameDisplayMode = EditorPrefsKey_Root + "NameDisplayMode";
    private const string EditorPrefsKey_AssetViewMode = EditorPrefsKey_Root + "AssetViewMode";

    private VisualElement m_Root;
    
    
    // ---------------------
    // Window basic stuff
    // ---------------------
    [MenuItem("Neverway/Mapping Tools")]
    public static void ShowGUIWindow()
    {
        MappingTools wnd = GetWindow<MappingTools>();
        wnd.titleContent = new GUIContent("⌬ Mapping Tools");
    }
    
    public void CreateGUI()
    {
        // Root window stuffs
        m_Root = rootVisualElement;
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        m_Root.Add(labelFromUXML);
        
        // Section tabs
        m_StatusSection = m_Root.Q<VisualElement>("LevelStatusGroup");
        m_AssetsSection = m_Root.Q<VisualElement>("MapperAssetsGroup");
        m_SettingsSection = m_Root.Q<VisualElement>("ProjectSettings");
        m_HelpSection = m_Root.Q<VisualElement>("Help");
        m_Root.Q<Button>("LevelStatusButton").clicked += () => ShowSection(m_StatusSection);
        m_Root.Q<Button>("MapperAssetsButton").clicked += () => ShowSection(m_AssetsSection);
        m_Root.Q<Button>("ProjectSettingsButton").clicked += () => ShowSection(m_SettingsSection);
        m_Root.Q<Button>("HelpButton").clicked += () => ShowSection(m_HelpSection);
        ShowSection(m_StatusSection);

        // Mapper settings
        m_Settings = LoadOrCreateSettings();
        var serializedObject = new SerializedObject(m_Settings);
        m_Root.Bind(serializedObject);
        m_Root.TrackSerializedObjectValue(serializedObject, _ => EditorUtility.SetDirty(m_Settings));
        
        // Issues tab
        m_SceneNameLabel = m_Root.Q<Label>("SceneName");
        m_LevelStatusHelpBox = m_Root.Q<HelpBox>("LevelStatus");
        m_LevelReadyLabel = m_Root.Q<Label>("LevelReadyLabel");
        m_IssuesContainer = m_Root.Q<VisualElement>("IssuesContainer");
        m_FixAllButton = m_Root.Q<Button>("FixAllButton");
        m_FixAllButton.clicked += FixAllIssues;
        m_BakeProgressBar = m_Root.Q<ProgressBar>("BakeProgressBar");
        m_Root.Q<Button>("BakeLevel").clicked += BakeLevel;
        m_Root.Q<Button>("PartialBakeLevel").clicked += PartialBakeLevel;
        m_Root.Q<Button>("BackToEditMode").clicked += UnbakeLevel;
        EditorApplication.hierarchyChanged += HandleHierarchyChanged;
        EditorSceneManager.activeSceneChangedInEditMode += (_, __) => RefreshLevelStatus();
        RefreshLevelStatus();

        // Mapper assets tab
        m_AssetLabelsToolbar = m_Root.Q<Toolbar>("AssetLabelsToolbar");
        m_AssetsResultsList = m_Root.Q<ListView>("AssetResultList");
        m_AssetResultGridScroll = m_Root.Q<ScrollView>("AssetResultGridScroll");
        m_AssetResultGrid = m_Root.Q<VisualElement>("AssetResultGrid");
        m_Root.Q<ToolbarButton>("ViewGrid").clicked += () => SetViewMode(AssetViewMode.Grid);
        m_Root.Q<ToolbarButton>("ViewListButton").clicked += () => SetViewMode(AssetViewMode.List);
        m_Root.Q<ToolbarButton>("NameDisplay").clicked += () => SetNameDisplayMode(AssetNameDisplayMode.Display);
        m_Root.Q<ToolbarButton>("NameID").clicked += () => SetNameDisplayMode(AssetNameDisplayMode.ID);
        SetUpAssetResultsList();
        PopulateAssetLabelToolbar();
        SerializedProperty assetLabelsProperty = serializedObject.FindProperty(nameof(MappingToolsSettings.m_AssetLabels));
        m_Root.TrackPropertyValue(assetLabelsProperty, _ => PopulateAssetLabelToolbar());
        m_AssetSearchField = m_Root.Q<ToolbarSearchField>("AssetSearchField");
        m_AssetSearchField.RegisterValueChangedCallback(_ => ApplySearchFilter());
        
        string lastLabel = SessionState.GetString(LastLabelSessionKey, null);
        bool lastLabelStillValid = !string.IsNullOrEmpty(lastLabel) && m_Settings.m_AssetLabels.Any(e => e != null && e.m_Label == lastLabel);

        string labelToLoad = lastLabelStillValid
            ? lastLabel
            : m_Settings.m_AssetLabels.FirstOrDefault(e => e != null && !string.IsNullOrWhiteSpace(e.m_Label))?.m_Label;

        if (!string.IsNullOrEmpty(labelToLoad))
        {
            ShowAssetsWithLabel(labelToLoad);
        }

        //AssetViewMode lastViewMode = (AssetViewMode)SessionState.GetInt(LastViewModeSessionKey, (int)AssetViewMode.Grid);
        //SetViewMode(lastViewMode);

        // Load Editor Prefs Values
        SetNameDisplayMode((AssetNameDisplayMode)EditorPrefs.GetInt(EditorPrefsKey_NameDisplayMode, (int)AssetNameDisplayMode.ID));
        SetViewMode((AssetViewMode)EditorPrefs.GetInt(EditorPrefsKey_AssetViewMode, (int)AssetViewMode.Grid));
    }
    
    private static MappingToolsSettings LoadOrCreateSettings()
    {
        MappingToolsSettings settings = AssetDatabase.LoadAssetAtPath<MappingToolsSettings>(SettingsAssetPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<MappingToolsSettings>();
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }
    
    private void SaveSettings()
    {
        if (m_Settings == null) return;
        AssetDatabase.SaveAssets();
    }
    
    private void OnLostFocus()
    {
        SaveSettings();
    }
    
    private void OnDestroy()
    {
        SaveSettings();
        EditorApplication.hierarchyChanged -= HandleHierarchyChanged;
    }
    
    private void ShowSection(VisualElement _sectionToShow)
    {
        if (m_SettingsSection.style.display == DisplayStyle.Flex && _sectionToShow != m_SettingsSection)
        {
            SaveSettings();
        }
        
        m_StatusSection.style.display = (m_StatusSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
        m_AssetsSection.style.display = (m_AssetsSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
        m_SettingsSection.style.display = (m_SettingsSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
        m_HelpSection.style.display = (m_HelpSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    
    // ---------------------
    // Level status and issues
    // ---------------------
    private void ShowIssueDetails(LevelIssue _issue)
    {
        string message = string.IsNullOrWhiteSpace(_issue.m_DetailedMessage) ? _issue.m_Message : _issue.m_DetailedMessage;
        EditorUtility.DisplayDialog("Issue Details", message, "OK");
    }
    
    private void HandleHierarchyChanged()
    {
        if (m_IsBaking) return; 
        m_NeedsBake = true;
        RefreshLevelStatus();
    }
    
    // ---------------------
    // Level status and issues - Issue detection junk
    // ---------------------
    private void RefreshLevelStatus()
    {
        if (m_Settings == null) return;

        m_SceneNameLabel.text = $"</b>Current Scene:<b> {EditorSceneManager.GetActiveScene().name}";

        m_Issues.Clear();
        DetectMissingRequiredObjects();

        RebuildIssuesList();

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            m_LevelStatusHelpBox.style.display = DisplayStyle.None;
            m_LevelReadyLabel.style.display = DisplayStyle.Flex;
            m_LevelReadyLabel.text = "Issue detection is disabled while game is in playmode";
            return;
        }
        
        if (m_Issues.Count > 0)
        {
            m_LevelStatusHelpBox.style.display = DisplayStyle.Flex;
            m_LevelReadyLabel.style.display = DisplayStyle.None;
            m_LevelStatusHelpBox.messageType = HelpBoxMessageType.Error;
            m_LevelStatusHelpBox.text = "Issues found, check list below!";
        }
        else if (m_NeedsBake)
        {
            m_LevelStatusHelpBox.style.display = DisplayStyle.Flex;
            m_LevelReadyLabel.style.display = DisplayStyle.None;
            m_LevelStatusHelpBox.messageType = HelpBoxMessageType.Warning;
            m_LevelStatusHelpBox.text = "Level needs to be baked";
        }
        else
        {
            m_LevelStatusHelpBox.style.display = DisplayStyle.None;
            m_LevelReadyLabel.style.display = DisplayStyle.Flex;
            m_LevelReadyLabel.text = "Level is ready!";
        }
    }
    
    private void DetectMissingRequiredObjects()
    {
        HashSet<string> presentMarkerGuids = new HashSet<string>();
        HashSet<GameObject> legacyConnectedAssets = new HashSet<GameObject>();

        Scene activeScene = EditorSceneManager.GetActiveScene();
        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                RequiredLevelObject marker = t.GetComponent<RequiredLevelObject>();
                if (marker != null && !string.IsNullOrEmpty(marker.m_SourceGuid))
                    presentMarkerGuids.Add(marker.m_SourceGuid);

                GameObject source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(t.gameObject);
                if (source != null)
                    legacyConnectedAssets.Add(source);
            }
        }

        foreach (RequiredLevelObjectEntry entry in m_Settings.m_RequiredLevelObjects)
        {
            if (entry == null || entry.m_Object == null) continue;

            GameObject required = entry.m_Object;
            string requiredGuid = GetAssetGuid(required);
            bool present = (!string.IsNullOrEmpty(requiredGuid) && presentMarkerGuids.Contains(requiredGuid))
                           || legacyConnectedAssets.Contains(required);

            if (!present)
            {
                RequiredLevelObjectEntry capturedEntry = entry;
                m_Issues.Add(new LevelIssue
                {
                    m_Message = $"Missing required object: {required.name}",
                    m_DetailedMessage = $"The prefab object '{required.name}' is required to be present in the level for the game to work correctly, but the asset couldn't be found. Please add it to your level.",
                    m_Severity = HelpBoxMessageType.Error,
                    m_FixAction = () => FixMissingRequiredObject(capturedEntry)
                });
            }
        }
    }
    
    private static string GetAssetGuid(Object _asset)
    {
        string path = AssetDatabase.GetAssetPath(_asset);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.AssetPathToGUID(path);
    }
    
    private bool IsRequiredObjectPresent(GameObject _requiredAsset)
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(t.gameObject) == _requiredAsset)
                    return true;
            }
        }
        return false;
    }
    
    private void FixMissingRequiredObject(RequiredLevelObjectEntry _entry)
    {
        GameObject requiredAsset = _entry.m_Object;
        GameObject instance;

        bool isPrefabAsset = PrefabUtility.GetPrefabAssetType(requiredAsset) != PrefabAssetType.NotAPrefab;

        if (isPrefabAsset)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(requiredAsset, EditorSceneManager.GetActiveScene());

            if (!_entry.m_KeepAsPrefab)
            {
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }
        else
        {
            instance = Instantiate(requiredAsset);
        }

        RequiredLevelObject marker = instance.AddComponent<RequiredLevelObject>();
        marker.m_SourceGuid = GetAssetGuid(requiredAsset);

        Undo.RegisterCreatedObjectUndo(instance, "Add Required Level Object");
        RefreshLevelStatus();
    }
    
    private void FixAllIssues()
    {foreach (LevelIssue issue in m_Issues.ToArray())
        {
            issue.m_FixAction?.Invoke();
        }
    }
    
    /// <summary>
    ///  THIS IS REALLY COR GEO SPECIFIC RIGHT NOW, FUTURE ME CHANGE THIS SO ITS MODULAR WITH FUTURE PROJECTS THNX
    /// </summary>
    private void BakeLevel()
    {
        if (m_IsBaking)
        {
            EditorUtility.DisplayDialog("Bake In Progress", "A bake is already in progress. Please wait for it to finish.", "OK");
            return;
        }

        if (m_Issues.Count > 0)
        {
            EditorUtility.DisplayDialog("Cannot Bake", "There are unresolved issues. Please fix them before baking the level.", "OK");
            return;
        }

        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Cannot Bake", "Cannot bake voxels while in Play Mode. Please exit Play Mode first.", "OK");
            return;
        }
        
        
        m_IsBaking = true;

        // Combine the CSG level mesh
        CSGMeshCombinerTool.CombineLevelMeshes();
        GameObject combinedMeshObject = GameObject.Find("CombinedLevelMesh");
        if (combinedMeshObject == null)
        {
            EditorUtility.DisplayDialog("Bake Failed", "Couldn't find or create the combined level mesh. Please check the Console for errors from the mesh combiner.", "OK");
            return;
        }

        // Assign it to the Voxel Occluder layer
        int occluderLayer = LayerMask.NameToLayer(VoxelOccluderLayerName);
        if (occluderLayer < 0)
        {
            EditorUtility.DisplayDialog("Bake Failed", $"The level geometry needs to be assigned to a layer named \"{VoxelOccluderLayerName}\" to be able to bake the level voxels correctly. Couldn't find a layer named \"{VoxelOccluderLayerName}\". Please check your project's Tags and Layers settings.", "OK");
            return;
        }
        combinedMeshObject.layer = occluderLayer;
        EditorUtility.SetDirty(combinedMeshObject);

        // Find the voxel world manager in the scene
        VoxWorldManager voxManager = FindObjectOfType<VoxWorldManager>();
        if (voxManager == null)
        {
            EditorUtility.DisplayDialog("Bake Failed", "No VoxWorldManager found in the scene.", "OK");
            return;
        }

        
        // Run the same bake routine as the "Bake Voxels in Editor" inspector button
        Editor voxEditor = Editor.CreateEditor(voxManager, typeof(VoxWorldManagerEditor));
        MethodInfo bakeCoroutineMethod = typeof(VoxWorldManagerEditor).GetMethod(
            "BakeVoxelsCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);

        if (bakeCoroutineMethod == null)
        {
            EditorUtility.DisplayDialog("Bake Failed", "Couldn't locate the voxel baking routine. VoxWorldManagerEditor may have changed.", "OK");
            Object.DestroyImmediate(voxEditor);
            return;
        }

        FieldInfo progressField = typeof(VoxWorldManagerEditor).GetField("bakingProgress", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo statusField = typeof(VoxWorldManagerEditor).GetField("bakingStatus", BindingFlags.NonPublic | BindingFlags.Instance);

        IEnumerator bakeRoutine = (IEnumerator)bakeCoroutineMethod.Invoke(voxEditor, null);

        m_BakeProgressBar.style.display = DisplayStyle.Flex;
        m_BakeProgressBar.value = 0;
        m_BakeProgressBar.title = "Starting bake...";

        RunEditorCoroutine(bakeRoutine,
            _onStep: () =>
            {
                float progress = progressField != null ? (float)progressField.GetValue(voxEditor) : 0f;
                string status = statusField != null ? (string)statusField.GetValue(voxEditor) : "";

                m_BakeProgressBar.value = progress * 100f;
                m_BakeProgressBar.title = string.IsNullOrEmpty(status)
                    ? $"{Mathf.RoundToInt(progress * 100f)}%"
                    : $"{status} ({Mathf.RoundToInt(progress * 100f)}%)";
            },
            _onComplete: () =>
            {
                Object.DestroyImmediate(voxEditor);
                m_BakeProgressBar.style.display = DisplayStyle.None;
                m_NeedsBake = false;

                EditorApplication.delayCall += () =>
                {
                    m_IsBaking = false;
                    RefreshLevelStatus();
                };
            });
    }

    private void PartialBakeLevel()
    {
        if (m_IsBaking)
        {
            EditorUtility.DisplayDialog("Bake In Progress", "A bake is already in progress. Please wait for it to finish.", "OK");
            return;
        }

        if (m_Issues.Count > 0)
        {
            EditorUtility.DisplayDialog("Cannot Bake", "There are unresolved issues. Please fix them before baking the level.", "OK");
            return;
        }

        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Cannot Bake", "Cannot bake voxels while in Play Mode. Please exit Play Mode first.", "OK");
            return;
        }
        
        
        m_IsBaking = true;

        // Combine the CSG level mesh
        CSGMeshCombinerTool.CombineLevelMeshes();
        GameObject combinedMeshObject = GameObject.Find("CombinedLevelMesh");
        if (combinedMeshObject == null)
        {
            EditorUtility.DisplayDialog("Bake Failed", "Couldn't find or create the combined level mesh. Please check the Console for errors from the mesh combiner.", "OK");
            return;
        }

        // Assign it to the Voxel Occluder layer
        int occluderLayer = LayerMask.NameToLayer(VoxelOccluderLayerName);
        if (occluderLayer < 0)
        {
            EditorUtility.DisplayDialog("Bake Failed", $"The level geometry needs to be assigned to a layer named \"{VoxelOccluderLayerName}\" to be able to bake the level voxels correctly. Couldn't find a layer named \"{VoxelOccluderLayerName}\". Please check your project's Tags and Layers settings.", "OK");
            return;
        }
        combinedMeshObject.layer = occluderLayer;
        EditorUtility.SetDirty(combinedMeshObject);
        
        // Finish up
        m_NeedsBake = false;
        m_IsBaking = false;
        RefreshLevelStatus();
        
        
        EditorUtility.DisplayDialog("Partial Bake Completed", "The partial bake has completed. The level is ready to be played, but the Voxel Grid has not been rebuilt. You will need to do a full level bake if you want to update the voxels that represent the level geometry!", "OK");
    }

    private void UnbakeLevel()
    {
        GameObject combinedMeshObject = GameObject.Find("CombinedLevelMesh");
        if (combinedMeshObject != null)
        {
            DestroyImmediate(combinedMeshObject);
        }
        
        var meshGroupRoot = GameObject.Find("MeshGroup");
        for (int i = 0; i < meshGroupRoot.transform.childCount; i++)
        {
            meshGroupRoot.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void RebuildIssuesList()
    {
        m_IssuesContainer.Clear();
        
        
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            m_IssuesContainer.Add(new Label("Issue detection is disabled while game is in playmode")
            {
                style = { unityFontStyleAndWeight = FontStyle.Italic, opacity = 0.7f }
            });
            return;
        }

        if (m_Issues.Count == 0)
        {
            m_IssuesContainer.Add(new Label("No issues found.")
            {
                style = { unityFontStyleAndWeight = FontStyle.Italic, opacity = 0.7f }
            });
            return;
        }

        foreach (LevelIssue issue in m_Issues)
        {
            VisualElement row = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 }
            };

            HelpBox issueBox = new HelpBox(issue.m_Message, issue.m_Severity)
            {
                style = { flexGrow = 1, marginRight = 4, flexShrink = 1, minHeight = 0}
            };
            
            Button infoButton = new Button(() => ShowIssueDetails(issue)) { text = "i" };
            Button fixButton = new Button(issue.m_FixAction) { text = "Fix" };

            row.Add(issueBox);
            row.Add(infoButton);
            row.Add(fixButton);
            m_IssuesContainer.Add(row);
        }
    }
    
    private void RunEditorCoroutine(IEnumerator _routine, System.Action _onStep = null, System.Action _onComplete = null)
    {
        EditorApplication.CallbackFunction step = null;
        step = () =>
        {
            bool moved;
            try
            {
                moved = _routine.MoveNext();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                moved = false;
            }
        
            _onStep?.Invoke();
        
            if (!moved)
            {
                EditorApplication.update -= step;
                _onComplete?.Invoke();
            }
        };
        EditorApplication.update += step;
    }
    
    
    // ---------------------
    // Mapper assets
    // ---------------------
    private void PopulateAssetLabelToolbar()
    {
        m_AssetLabelsToolbar.Clear();
        m_AssetLabelButtons.Clear();

        foreach (var entry in m_Settings.m_AssetLabels)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.m_Label)) continue;

            string buttonText = string.IsNullOrWhiteSpace(entry.m_DisplayName) ? entry.m_Label : entry.m_DisplayName;
            string label = entry.m_Label;

            Button labelButton = new Button(() => ShowAssetsWithLabel(label))
            {
                text = buttonText
            };
            m_AssetLabelsToolbar.Add(labelButton);
            m_AssetLabelButtons[label] = labelButton;
        }

        RefreshAssetLabelButtonHighlight();
    }
    
    private void ShowAssetsWithLabel(string _label)
    {
        m_CurrentAssetLabel = _label;
        SessionState.SetString(LastLabelSessionKey, _label ?? string.Empty);

        m_AssetResults.Clear();

        string[] guids = AssetDatabase.FindAssets($"l:{_label}");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null) m_AssetResults.Add(asset);
        }

        RefreshAssetLabelButtonHighlight();
        ApplySearchFilter();
    }

    private void SetUpAssetResultsList()
    {
        m_AssetsResultsList.makeItem = () =>
        {
            VisualElement row = new VisualElement
                { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            Image icon = new Image { name = "Icon", style = { width = 16, height = 16, marginRight = 4 } };
            Label label = new Label { name = "Name" };
            row.Add(icon);
            row.Add(label);

            row.RegisterCallback<PointerDownEvent>(evt => OnResultRowPointerDown(evt, row));
            row.RegisterCallback<PointerMoveEvent>(evt => OnResultRowPointerMove(evt, row));
            row.RegisterCallback<PointerUpEvent>(evt => OnResultRowPointerUp(evt, row));

            return row;
        };

        m_AssetsResultsList.bindItem = (element, index) =>
        {
            Object asset = m_FilteredAssetResults[index];
            element.Q<Image>("Icon").image = AssetPreview.GetMiniThumbnail(asset);
            element.Q<Label>("Name").text = GetDisplayName(asset);
            element.userData = index;
        };

        m_AssetsResultsList.itemsSource = m_FilteredAssetResults;
        m_AssetsResultsList.selectionType = SelectionType.Single;
        m_AssetsResultsList.fixedItemHeight = 20;

        m_AssetsResultsList.selectionChanged += selection =>
        {
            Selection.objects = selection.OfType<Object>().ToArray();
        };
    }

    private void SetViewMode(AssetViewMode _mode)
    {
        m_ViewMode = _mode;
        //SessionState.SetInt(LastViewModeSessionKey, (int)_mode);
        EditorPrefs.SetInt(EditorPrefsKey_AssetViewMode, (int)_mode);

        m_AssetsResultsList.style.display = (_mode == AssetViewMode.List) ? DisplayStyle.Flex : DisplayStyle.None;
        m_AssetResultGridScroll.style.display = (_mode == AssetViewMode.Grid) ? DisplayStyle.Flex : DisplayStyle.None;

        RefreshResultsView();
    }

    private void RefreshResultsView()
    {
        if (m_ViewMode == AssetViewMode.List)
        {
            m_AssetsResultsList.Rebuild();
        }
        else
        {
            BuildAssetGrid();
        }
    }

    private void BuildAssetGrid()
    {
        m_AssetResultGrid.Clear();
        m_ResolvedPreviewIndices.Clear();
        m_SelectedGridIndices.Clear();
        m_GridSelectionAnchor = -1;

        AssetPreview.SetPreviewTextureCacheSize(Mathf.Max(m_FilteredAssetResults.Count + 32, 256));

        for (int i = 0; i < m_FilteredAssetResults.Count; i++)
        {
            int index = i;
            Object asset = m_FilteredAssetResults[i];

            VisualElement card = new VisualElement
            {
                style = { width = 72, marginLeft = 4, marginRight = 4, marginBottom = 8, alignItems = Align.Center }
            };

            Image preview = new Image
            {
                scaleMode = ScaleMode.ScaleToFit, style = { width = 64, height = 64 }
            };

            Texture2D initialPreview = AssetPreview.GetAssetPreview(asset);
            if (initialPreview != null)
            {
                preview.image = initialPreview;
                m_ResolvedPreviewIndices.Add(i);
            }
            else
            {
                preview.image = AssetPreview.GetMiniThumbnail(asset);
            }

            Label nameLabel = new Label(GetDisplayName(asset))
            {
                style = { width = 72, fontSize = 10, unityTextAlign = TextAnchor.UpperCenter, whiteSpace = WhiteSpace.Normal, marginTop = 2 }
            };

            card.Add(preview);
            card.Add(nameLabel);

            card.RegisterCallback<ClickEvent>(evt => OnGridCardClicked(evt, index));
            card.RegisterCallback<PointerDownEvent>(evt => OnGridCardPointerDown(evt, card));
            card.RegisterCallback<PointerMoveEvent>(evt => OnGridCardPointerMove(evt, index, card));
            card.RegisterCallback<PointerUpEvent>(evt => OnGridCardPointerUp(evt, card));

            m_AssetResultGrid.Add(card);
        }

        int elapsedMs = 0;
        const int intervalMs = 200;
        const int timeoutMs = 5000;

        m_Root.schedule.Execute(() => { RefreshLatePreviews(); elapsedMs += intervalMs; }).Every(intervalMs).Until(() => m_ResolvedPreviewIndices.Count >= m_FilteredAssetResults.Count || elapsedMs >= timeoutMs);
    }

    private void RefreshAssetLabelButtonHighlight()
    {
        foreach (var kvp in m_AssetLabelButtons)
        {
            kvp.Value.style.backgroundColor = (kvp.Key == m_CurrentAssetLabel)
                ? new StyleColor(new Color(0.24f, 0.48f, 0.90f, 0.5f))
                : new StyleColor(StyleKeyword.Null);
        }
    }
    
    private void ApplySearchFilter()
    {
        string query = m_AssetSearchField != null ? m_AssetSearchField.value : string.Empty;

        m_FilteredAssetResults.Clear();

        if (string.IsNullOrWhiteSpace(query))
        {
            m_FilteredAssetResults.AddRange(m_AssetResults);
        }
        else
        {
            m_FilteredAssetResults.AddRange(m_AssetResults.Where(a =>
                a != null && a.name.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0));
        }

        RefreshResultsView();
    }
    
    private void SetNameDisplayMode(AssetNameDisplayMode _mode)
    {
        m_NameDisplayMode = _mode;
        EditorPrefs.SetInt(EditorPrefsKey_NameDisplayMode, (int)_mode);

        RefreshResultsView();
    }

    private string GetDisplayName(Object _asset)
    {
        if (m_NameDisplayMode == AssetNameDisplayMode.ID)
            return _asset.name;

        if (_asset is GameObject go)
        {
            Actor actor = go.GetComponent<Actor>();
            if (actor != null && !string.IsNullOrWhiteSpace(actor.displayName))
                return actor.displayName;
        }

        return HumanizeName(_asset.name);
    }

    private static string HumanizeName(string _id)
    {
        if (string.IsNullOrWhiteSpace(_id)) return _id;

        string result = _id.Replace('_', ' ');
        result = Regex.Replace(result, "([a-z0-9])([A-Z])", "$1 $2");
        result = Regex.Replace(result, @"\s+", " ").Trim();

        string[] words = result.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length == 0) continue;
            words[i] = char.ToUpper(words[i][0]) + (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
        }
        return string.Join(" ", words);
    }
    
    // ---------------------
    // Mapper assets - Card selecting and dragging junk
    // ---------------------
    private void OnGridCardClicked(ClickEvent _evt, int _index)
    {
        bool toggleModifier = _evt.ctrlKey || _evt.commandKey;
        bool rangeModifier = _evt.shiftKey;

        if (rangeModifier && m_GridSelectionAnchor >= 0)
        {
            m_SelectedGridIndices.Clear();
            int start = Mathf.Min(m_GridSelectionAnchor, _index);
            int end = Mathf.Max(m_GridSelectionAnchor, _index);
            for (int i = start; i <= end; i++) m_SelectedGridIndices.Add(i);
        }
        else if (toggleModifier)
        {
            if (!m_SelectedGridIndices.Add(_index))
                m_SelectedGridIndices.Remove(_index);
            m_GridSelectionAnchor = _index;
        }
        else
        {
            m_SelectedGridIndices.Clear();
            m_SelectedGridIndices.Add(_index);
            m_GridSelectionAnchor = _index;
        }

        RefreshGridSelectionVisuals();

        Selection.objects = m_SelectedGridIndices
            .Where(i => i >= 0 && i < m_FilteredAssetResults.Count)
            .Select(i => m_FilteredAssetResults[i])
            .ToArray();
    }
    
    private void OnResultRowPointerDown(PointerDownEvent _evt, VisualElement _row)
    {
        if (_evt.button != 0) return;
        m_ListDragStartPos = _evt.position;
        _row.CapturePointer(_evt.pointerId);
    }

    private void OnResultRowPointerMove(PointerMoveEvent _evt, VisualElement _row)
    {
        if (!_row.HasPointerCapture(_evt.pointerId)) return;
        if (Vector2.Distance(_evt.position, m_ListDragStartPos) < 4f) return;

        _row.ReleasePointer(_evt.pointerId);
        BeginDragFromList((int)_row.userData);
    }

    private void OnResultRowPointerUp(PointerUpEvent _evt, VisualElement _row)
    {
        if (_row.HasPointerCapture(_evt.pointerId))
            _row.ReleasePointer(_evt.pointerId);
    }
    
    private void BeginDragFromList(int _index)
    {
        if (_index < 0 || _index >= m_FilteredAssetResults.Count) return;
    
        List<int> selectedIndices = m_AssetsResultsList.selectedIndices.ToList();
    
        Object[] dragged = (selectedIndices.Contains(_index) && selectedIndices.Count > 0)
            ? selectedIndices.Select(i => m_FilteredAssetResults[i]).ToArray()
            : new[] { m_FilteredAssetResults[_index] };
    
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.objectReferences = dragged;
        DragAndDrop.StartDrag(dragged.Length > 1 ? $"{dragged.Length} Assets" : dragged[0].name);
    }
    
    private void OnGridCardPointerDown(PointerDownEvent _evt, VisualElement _card)
    {
        if (_evt.button != 0) return;
        m_GridDragStartPos = _evt.position;
        _card.CapturePointer(_evt.pointerId);
    }

    private void OnGridCardPointerMove(PointerMoveEvent _evt, int _index, VisualElement _card)
    {
        if (!_card.HasPointerCapture(_evt.pointerId)) return;
        if (Vector2.Distance(_evt.position, m_GridDragStartPos) < 4f) return;

        _card.ReleasePointer(_evt.pointerId);
        BeginDragFromGrid(_index);
    }

    private void OnGridCardPointerUp(PointerUpEvent _evt, VisualElement _card)
    {
        if (_card.HasPointerCapture(_evt.pointerId))
            _card.ReleasePointer(_evt.pointerId);
    }

    private void BeginDragFromGrid(int _index)
    {
        if (_index < 0 || _index >= m_FilteredAssetResults.Count) return;

        Object[] dragged = (m_SelectedGridIndices.Contains(_index) && m_SelectedGridIndices.Count > 0)
            ? m_SelectedGridIndices.Select(i => m_FilteredAssetResults[i]).ToArray()
            : new[] { m_FilteredAssetResults[_index] };

        DragAndDrop.PrepareStartDrag();
        DragAndDrop.objectReferences = dragged;
        DragAndDrop.StartDrag(dragged.Length > 1 ? $"{dragged.Length} Assets" : dragged[0].name);
    }

    private void RefreshGridSelectionVisuals()
    {
        for (int i = 0; i < m_AssetResultGrid.childCount; i++)
        {
            VisualElement card = m_AssetResultGrid[i];
            bool isSelected = m_SelectedGridIndices.Contains(i);
            card.style.backgroundColor = isSelected
                ? new StyleColor(new Color(0.24f, 0.48f, 0.90f, 0.5f))
                : new StyleColor(StyleKeyword.Null);
        }
    }

    private void RefreshLatePreviews()
    {
        for (int i = 0; i < m_FilteredAssetResults.Count; i++)
        {
            if (m_ResolvedPreviewIndices.Contains(i)) continue;
            if (i >= m_AssetResultGrid.childCount) break;

            Texture2D preview = AssetPreview.GetAssetPreview(m_FilteredAssetResults[i]);
            if (preview != null)
            {
                Image img = m_AssetResultGrid[i].Q<Image>();
                if (img != null) img.image = preview;
                m_ResolvedPreviewIndices.Add(i);
            }
        }
    }

    private class LevelIssue
    {
        public string m_Message;
        public string m_DetailedMessage;
        public HelpBoxMessageType m_Severity;
        public System.Action m_FixAction;
    }
}

