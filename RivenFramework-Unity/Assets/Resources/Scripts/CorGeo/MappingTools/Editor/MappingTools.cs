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
    
    private VisualElement m_MapsSection;
    private VisualElement m_StatusSection;
    private VisualElement m_AssetsSection;
    private VisualElement m_SettingsSection;
    private VisualElement m_HelpSection;
    
    // Maps tab
    private enum MapsSelection { BuildList, Group }
    private LevelOrganizer m_LevelOrganizer;
    private SerializedObject m_LevelOrganizerSO;
    private VisualElement m_MapsSidebarContent;
    private VisualElement m_MapsContent;
    private MapsSelection m_MapsSelectionType = MapsSelection.BuildList;
    private string m_SelectedGroupId;
    private bool m_MapsGroupEditMode = false;
    private string m_SidebarDragGroupId;
    private VisualElement m_SidebarDragRow;
    private const string LastMapsSelectionSessionKey = "MappingTools_LastMapsSelectedGroupId";

    private VisualElement m_MapsSidebar;
    private VisualElement m_MapsSidebarSplitter;
    private float m_SidebarResizeStartWidth;
    private Vector2 m_SidebarResizeStartPos;
    private const float MapsSidebarMinWidth = 120f;
    private const float MapsSidebarMaxWidth = 500f;
    private const string EditorPrefsKey_MapsSidebarWidth = EditorPrefsKey_Root + "MapsSidebarWidth";

    private string m_LevelSetDragGroupId;
    private VisualElement m_LevelSetDragRow;
    
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
        m_MapsSection = m_Root.Q<VisualElement>("MapsGroup");
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

        try
        {
            m_Root.Q<Button>("MapsButton").clicked += () => ShowSection(m_MapsSection);

            ObjectField levelOrganizerField = m_Root.Q<ObjectField>("LevelOrganizerField");
            levelOrganizerField.objectType = typeof(LevelOrganizer);
            ObjectField templateSceneField = m_Root.Q<ObjectField>("TemplateMapSceneField");
            templateSceneField.objectType = typeof(SceneAsset);

            InitMapsTab();
            SerializedProperty levelOrganizerProp = serializedObject.FindProperty(nameof(MappingToolsSettings.m_LevelOrganizer));
            m_Root.TrackPropertyValue(levelOrganizerProp, _ => LoadLevelOrganizerFromSettings());
        }
        catch (System.Exception e)
        {
            Debug.LogError("MappingTools: Maps tab failed to initialize (see exception below)");
            Debug.LogException(e);
        }
        
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
        
        LoadLastSelectedAssetLabel();

        //AssetViewMode lastViewMode = (AssetViewMode)SessionState.GetInt(LastViewModeSessionKey, (int)AssetViewMode.Grid);
        //SetViewMode(lastViewMode);

        // Load Editor Prefs Values
        SetNameDisplayMode((AssetNameDisplayMode)EditorPrefs.GetInt(EditorPrefsKey_NameDisplayMode, (int)AssetNameDisplayMode.ID));
        SetViewMode((AssetViewMode)EditorPrefs.GetInt(EditorPrefsKey_AssetViewMode, (int)AssetViewMode.Grid));

        EditorApplication.delayCall += () =>
        {
            if (this == null) return; 

            try
            {
                LoadLevelOrganizerFromSettings();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            PopulateAssetLabelToolbar();
            LoadLastSelectedAssetLabel();
        };
    }

    private void LoadLastSelectedAssetLabel()
    {
        string lastLabel = SessionState.GetString(LastLabelSessionKey, null);
        bool lastLabelStillValid = !string.IsNullOrEmpty(lastLabel) && m_Settings.m_AssetLabels.Any(e => e != null && e.m_Label == lastLabel);

        string labelToLoad = lastLabelStillValid
            ? lastLabel
            : m_Settings.m_AssetLabels.FirstOrDefault(e => e != null && !string.IsNullOrWhiteSpace(e.m_Label))?.m_Label;

        if (!string.IsNullOrEmpty(labelToLoad))
        {
            ShowAssetsWithLabel(labelToLoad);
        }
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
        
        if (m_MapsSection != null)
        {
            m_MapsSection.style.display = (m_MapsSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
        }
        m_StatusSection.style.display = (m_StatusSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
        m_AssetsSection.style.display = (m_AssetsSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
        m_SettingsSection.style.display = (m_SettingsSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
        m_HelpSection.style.display = (m_HelpSection == _sectionToShow) ? DisplayStyle.Flex : DisplayStyle.None;
        if (_sectionToShow == m_MapsSection)
        {
            RefreshMapsTab();
        }
    }

    // ---------------------
    // Maps - setup
    // ---------------------
    private void InitMapsTab()
    {
        m_MapsSidebarContent = m_Root.Q<VisualElement>("MapsSidebarContent");
        m_MapsContent = m_Root.Q<VisualElement>("MapsContent");
        m_MapsSidebar = m_Root.Q<VisualElement>("MapsSidebar");
        m_MapsSidebarSplitter = m_Root.Q<VisualElement>("MapsSidebarSplitter");

        const float defaultSidebarWidth = 200f;
        float savedWidth = EditorPrefs.GetFloat(EditorPrefsKey_MapsSidebarWidth, defaultSidebarWidth);
        if (m_MapsSidebar != null)
        {
            m_MapsSidebar.style.width = Mathf.Clamp(savedWidth, MapsSidebarMinWidth, MapsSidebarMaxWidth);
        }
        RegisterSidebarSplitterDrag();

        m_SelectedGroupId = SessionState.GetString(LastMapsSelectionSessionKey, null);
        m_MapsSelectionType = string.IsNullOrEmpty(m_SelectedGroupId) ? MapsSelection.BuildList : MapsSelection.Group;

        LoadLevelOrganizerFromSettings();
    }

    // ---------------------
    // Maps - sidebar resize splitter
    // ---------------------
    private void RegisterSidebarSplitterDrag()
    {
        if (m_MapsSidebarSplitter == null || m_MapsSidebar == null) return;

        m_MapsSidebarSplitter.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button != 0) return;
            m_SidebarResizeStartWidth = m_MapsSidebar.resolvedStyle.width;
            m_SidebarResizeStartPos = evt.position;
            m_MapsSidebarSplitter.CapturePointer(evt.pointerId);
        });

        m_MapsSidebarSplitter.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (!m_MapsSidebarSplitter.HasPointerCapture(evt.pointerId)) return;

            float delta = evt.position.x - m_SidebarResizeStartPos.x;
            float newWidth = Mathf.Clamp(m_SidebarResizeStartWidth + delta, MapsSidebarMinWidth, MapsSidebarMaxWidth);
            m_MapsSidebar.style.width = newWidth;
        });

        m_MapsSidebarSplitter.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (!m_MapsSidebarSplitter.HasPointerCapture(evt.pointerId)) return;
            m_MapsSidebarSplitter.ReleasePointer(evt.pointerId);
            EditorPrefs.SetFloat(EditorPrefsKey_MapsSidebarWidth, m_MapsSidebar.resolvedStyle.width);
        });
    }

    private void LoadLevelOrganizerFromSettings()
    {
        LevelOrganizer organizer = m_Settings != null ? m_Settings.m_LevelOrganizer : null;

        if (organizer != m_LevelOrganizer)
        {
            m_LevelOrganizer = organizer;
            m_LevelOrganizerSO = m_LevelOrganizer != null ? new SerializedObject(m_LevelOrganizer) : null;

            if (m_LevelOrganizerSO != null)
            {
                EnsureGroupIds();

                m_Root.TrackSerializedObjectValue(m_LevelOrganizerSO, _ =>
                {
                    EditorUtility.SetDirty(m_LevelOrganizer);
                    SyncBuildSettingsFromBuildList();
                    RefreshMapsTab();
                });
            }
        }

        if (m_LevelOrganizer != null)
        {
            SyncBuildSettingsFromBuildList();
        }

        RefreshMapsTab();
    }

    private void EnsureGroupIds()
    {
        if (m_LevelOrganizer == null) return;

        bool changed = false;

        if (m_LevelOrganizer.LevelSetGroups == null)
        {
            m_LevelOrganizer.LevelSetGroups = new List<LevelSetGroup>();
        }

        foreach (LevelSetGroup group in m_LevelOrganizer.LevelSetGroups)
        {
            if (group != null && string.IsNullOrEmpty(group.id))
            {
                group.id = Guid.NewGuid().ToString();
                changed = true;
            }
        }

        if (m_LevelOrganizer.BuildListGroupIds == null)
        {
            m_LevelOrganizer.BuildListGroupIds = new List<string>();
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(m_LevelOrganizer);
            m_LevelOrganizerSO.Update();
        }
    }

    // ---------------------
    // Maps - sidebar
    // ---------------------
    private void RefreshMapsTab()
    {
        if (m_MapsSidebarContent == null || m_MapsContent == null) return;

        m_MapsSidebarContent.Clear();

        if (m_LevelOrganizer == null)
        {
            m_MapsSidebarContent.Add(new HelpBox(
                "No Level Organizer assigned. Set one in the Project Settings tab.",
                HelpBoxMessageType.Warning) { style = { whiteSpace = WhiteSpace.Normal } });
            m_MapsContent.Clear();
            return;
        }

        Button buildListButton = new Button(SelectBuildList) { text = "Build List" };
        buildListButton.AddToClassList("maps-sidebar-header");
        buildListButton.style.marginBottom = 10;
        buildListButton.style.height = 25;
        if (m_MapsSelectionType == MapsSelection.BuildList) buildListButton.AddToClassList("maps-sidebar-selected");
        m_MapsSidebarContent.Add(buildListButton);

        List<LevelSetGroup> allGroups = m_LevelOrganizer.LevelSetGroups ?? new List<LevelSetGroup>();
        foreach (LevelSetGroup group in allGroups)
        {
            if (group == null) continue;
            m_MapsSidebarContent.Add(CreateSidebarGroupRow(group));
        }

        Button addGroupButton = new Button(AddNewGroup) { text = "New Group" };
        addGroupButton.style.marginTop = 10;
        m_MapsSidebarContent.Add(addGroupButton);

        RefreshMapsContent();
    }

    private VisualElement CreateSidebarGroupRow(LevelSetGroup _group)
    {
        VisualElement row = new VisualElement
        {
            style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
        };
        row.AddToClassList("maps-sidebar-row");

        Label dragHandle = new Label("\u2261")
        {
            tooltip = "Drag to reorder",
            style = { width = 14, opacity = 0.4f, unityTextAlign = TextAnchor.MiddleCenter, marginRight = 2 }
        };
        row.Add(dragHandle);

        bool inBuildList = m_LevelOrganizer.BuildListGroupIds != null && m_LevelOrganizer.BuildListGroupIds.Contains(_group.id);

        Button nameButton = new Button(() => SelectGroup(_group.id))
        {
            text = string.IsNullOrWhiteSpace(_group.name) ? "(Unnamed Group)" : _group.name,
            tooltip = inBuildList ? "In Build List" : "Not in Build List"
        };
        nameButton.AddToClassList("maps-sidebar-item");
        nameButton.style.flexGrow = 1;
        if (inBuildList)
        {
            nameButton.style.unityFontStyleAndWeight = FontStyle.Bold;
        }
        if (m_MapsSelectionType == MapsSelection.Group && m_SelectedGroupId == _group.id)
        {
            nameButton.AddToClassList("maps-sidebar-selected");
        }
        row.Add(nameButton);

        Button deleteButton = new Button(() => DeleteGroup(_group.id)) { text = "\u2715", tooltip = "Delete Group" };
        deleteButton.style.width = 20;
        deleteButton.style.marginLeft = 2;
        row.Add(deleteButton);

        RegisterSidebarRowDrag(row, dragHandle, _group.id);

        return row;
    }

    // ---------------------
    // Maps - sidebar drag-to-reorder
    // ---------------------
    private void RegisterSidebarRowDrag(VisualElement _row, VisualElement _dragHandle, string _groupId)
    {
        _dragHandle.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button != 0) return;
            m_SidebarDragGroupId = _groupId;
            m_SidebarDragRow = _row;
            Undo.RecordObject(m_LevelOrganizer, "Reorder Level Set Groups");
            _dragHandle.CapturePointer(evt.pointerId);
        });

        _dragHandle.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (!_dragHandle.HasPointerCapture(evt.pointerId) || m_SidebarDragRow == null) return;

            VisualElement container = m_MapsSidebarContent;
            Vector2 localPos = container.WorldToLocal(evt.position);

            int currentIndex = container.IndexOf(m_SidebarDragRow);
            int targetIndex = currentIndex;

            for (int i = 0; i < container.childCount; i++)
            {
                VisualElement sibling = container[i];
                if (!sibling.ClassListContains("maps-sidebar-row")) continue;
                Rect bounds = sibling.layout;
                if (localPos.y >= bounds.yMin && localPos.y <= bounds.yMax)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex != currentIndex)
            {
                container.Insert(targetIndex, m_SidebarDragRow);
                MoveGroupInMasterListLive(m_SidebarDragGroupId, targetIndex - 1);
            }
        });

        _dragHandle.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (_dragHandle.HasPointerCapture(evt.pointerId))
                _dragHandle.ReleasePointer(evt.pointerId);

            if (m_SidebarDragGroupId != null)
            {
                EditorUtility.SetDirty(m_LevelOrganizer);
                m_LevelOrganizerSO.Update();
            }

            m_SidebarDragGroupId = null;
            m_SidebarDragRow = null;
        });
    }

    private void MoveGroupInMasterListLive(string _groupId, int _newIndex)
    {
        List<LevelSetGroup> groups = m_LevelOrganizer.LevelSetGroups;
        if (groups == null) return;

        int oldIndex = groups.FindIndex(g => g != null && g.id == _groupId);
        if (oldIndex < 0) return;

        _newIndex = Mathf.Clamp(_newIndex, 0, groups.Count - 1);
        if (_newIndex == oldIndex) return;

        LevelSetGroup group = groups[oldIndex];
        groups.RemoveAt(oldIndex);
        groups.Insert(_newIndex, group);
    }

    private void SelectBuildList()
    {
        m_MapsSelectionType = MapsSelection.BuildList;
        m_SelectedGroupId = null;
        m_MapsGroupEditMode = false;
        SessionState.EraseString(LastMapsSelectionSessionKey);
        RefreshMapsTab();
    }

    private void SelectGroup(string _groupId)
    {
        m_MapsSelectionType = MapsSelection.Group;
        m_SelectedGroupId = _groupId;
        m_MapsGroupEditMode = false;
        SessionState.SetString(LastMapsSelectionSessionKey, _groupId);
        RefreshMapsTab();
    }

    // ---------------------
    // Maps - content router
    // ---------------------
    private void RefreshMapsContent()
    {
        m_MapsContent.Clear();
        if (m_LevelOrganizer == null) return;

        if (m_MapsSelectionType == MapsSelection.BuildList)
        {
            BuildBuildListOverview();
        }
        else
        {
            LevelSetGroup group = m_LevelOrganizer.LevelSetGroups?.FirstOrDefault(g => g != null && g.id == m_SelectedGroupId);
            if (group == null)
            {
                m_MapsContent.Add(new Label("Select a group from the list on the left.")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Italic, opacity = 0.7f }
                });
                return;
            }
            BuildGroupEditor(group);
        }
    }

    // ---------------------
    // Maps - Build List overview
    // ---------------------
    private void BuildBuildListOverview()
    {
        m_MapsContent.Add(new Label("Build List")
        {
            style = { fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 2 }
        });
        m_MapsContent.Add(new Label(
            "Groups below are written to Build Settings in this order. Within a group, scenes are added in the order their level sets and levels appear.")
        {
            style = { whiteSpace = WhiteSpace.Normal, opacity = 0.8f, marginBottom = 8 }
        });

        List<LevelSetGroup> allGroups = m_LevelOrganizer.LevelSetGroups ?? new List<LevelSetGroup>();
        List<string> buildListIds = m_LevelOrganizer.BuildListGroupIds ?? new List<string>();
        List<LevelSetGroup> availableGroups = allGroups.Where(g => g != null && !buildListIds.Contains(g.id)).ToList();

        VisualElement addRow = new VisualElement
        {
            style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 10, alignItems = Align.Center }
        };
        if (availableGroups.Count > 0)
        {
            List<string> choices = availableGroups
                .Select(g => string.IsNullOrWhiteSpace(g.name) ? "(Unnamed Group)" : g.name)
                .ToList();

            DropdownField addDropdown = new DropdownField(choices, 0)
            {
                style = { flexGrow = 1, flexShrink = 1, minWidth = 0, marginRight = 4, marginBottom = 4 }
            };
            Button addButton = new Button(() =>
            {
                int idx = addDropdown.index;
                if (idx < 0 || idx >= availableGroups.Count) return;
                AddGroupToBuildList(availableGroups[idx].id);
            })
            {
                text = "Add To Build List",
                style = { flexShrink = 0, marginBottom = 4 }
            };

            addRow.Add(addDropdown);
            addRow.Add(addButton);
        }
        else
        {
            addRow.Add(new Label("All groups are already in the Build List.")
            {
                style = { opacity = 0.6f, unityFontStyleAndWeight = FontStyle.Italic }
            });
        }
        m_MapsContent.Add(addRow);

        List<LevelSetGroup> includedGroups = buildListIds
            .Select(id => allGroups.FirstOrDefault(g => g != null && g.id == id))
            .Where(g => g != null)
            .ToList();

        if (includedGroups.Count == 0)
        {
            m_MapsContent.Add(new HelpBox("No groups are in the Build List.", HelpBoxMessageType.Info));
            return;
        }

        int sceneCounter = 0;
        for (int i = 0; i < includedGroups.Count; i++)
        {
            LevelSetGroup group = includedGroups[i];
            int capturedIndex = i;

            Foldout groupFoldout = new Foldout
            {
                text = string.IsNullOrWhiteSpace(group.name) ? "(Unnamed Group)" : group.name,
                value = false
            };
            groupFoldout.style.marginBottom = 6;

            VisualElement controlsRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 4 } };
            Button upButton = new Button(() => MoveBuildListGroup(capturedIndex, -1)) { text = "\u2191" };
            Button downButton = new Button(() => MoveBuildListGroup(capturedIndex, 1)) { text = "\u2193" };
            Button removeButton = new Button(() => RemoveBuildListGroup(capturedIndex)) { text = "Remove From Build List" };
            upButton.SetEnabled(capturedIndex > 0);
            downButton.SetEnabled(capturedIndex < includedGroups.Count - 1);
            upButton.style.width = 24;
            downButton.style.width = 24;
            controlsRow.Add(upButton);
            controlsRow.Add(downButton);
            controlsRow.Add(removeButton);
            groupFoldout.Add(controlsRow);

            List<LevelSet> levelSets = group.levelSets ?? new List<LevelSet>();
            foreach (LevelSet levelSet in levelSets)
            {
                Label setLabel = new Label(string.IsNullOrWhiteSpace(levelSet.name) ? "(Unnamed Level Set)" : levelSet.name)
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 4 }
                };
                groupFoldout.Add(setLabel);

                List<SceneReference> levels = levelSet.levels ?? new List<SceneReference>();
                foreach (SceneReference level in levels)
                {
                    sceneCounter++;
                    bool missing = string.IsNullOrEmpty(SceneReferenceUtil.GetScenePath(level));
                    Label sceneRow = new Label($"{sceneCounter}. {SceneReferenceUtil.GetDisplayName(level)}")
                    {
                        style = { marginLeft = 12 }
                    };
                    if (missing)
                    {
                        sceneRow.style.color = new StyleColor(new Color(0.9f, 0.4f, 0.4f));
                    }
                    groupFoldout.Add(sceneRow);
                }
            }

            m_MapsContent.Add(groupFoldout);
        }

        Button resyncButton = new Button(SyncBuildSettingsFromBuildList) { text = "Force Resync Build Settings" };
        resyncButton.style.marginTop = 8;
        m_MapsContent.Add(resyncButton);
    }

    private void AddGroupToBuildList(string _groupId)
    {
        Undo.RecordObject(m_LevelOrganizer, "Add Group To Build List");
        if (m_LevelOrganizer.BuildListGroupIds == null) m_LevelOrganizer.BuildListGroupIds = new List<string>();
        m_LevelOrganizer.BuildListGroupIds.Add(_groupId);
        EditorUtility.SetDirty(m_LevelOrganizer);
        m_LevelOrganizerSO.Update();
        SyncBuildSettingsFromBuildList();
        RefreshMapsTab();
    }

    private void RemoveBuildListGroup(int _index)
    {
        List<string> ids = m_LevelOrganizer.BuildListGroupIds;
        if (ids == null || _index < 0 || _index >= ids.Count) return;

        Undo.RecordObject(m_LevelOrganizer, "Remove Group From Build List");
        ids.RemoveAt(_index);
        EditorUtility.SetDirty(m_LevelOrganizer);
        m_LevelOrganizerSO.Update();
        SyncBuildSettingsFromBuildList();
        RefreshMapsTab();
    }

    private void MoveBuildListGroup(int _index, int _direction)
    {
        List<string> ids = m_LevelOrganizer.BuildListGroupIds;
        if (ids == null) return;
        int newIndex = _index + _direction;
        if (newIndex < 0 || newIndex >= ids.Count) return;

        Undo.RecordObject(m_LevelOrganizer, "Reorder Build List");
        (ids[_index], ids[newIndex]) = (ids[newIndex], ids[_index]);
        EditorUtility.SetDirty(m_LevelOrganizer);
        m_LevelOrganizerSO.Update();
        SyncBuildSettingsFromBuildList();
        RefreshMapsTab();
    }

    private void SyncBuildSettingsFromBuildList()
    {
        if (m_LevelOrganizer == null) return;

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        List<string> buildListIds = m_LevelOrganizer.BuildListGroupIds ?? new List<string>();
        List<LevelSetGroup> allGroups = m_LevelOrganizer.LevelSetGroups ?? new List<LevelSetGroup>();

        foreach (string groupId in buildListIds)
        {
            LevelSetGroup group = allGroups.FirstOrDefault(g => g != null && g.id == groupId);
            if (group?.levelSets == null) continue;

            foreach (LevelSet levelSet in group.levelSets)
            {
                if (levelSet?.levels == null) continue;

                foreach (SceneReference level in levelSet.levels)
                {
                    string path = SceneReferenceUtil.GetScenePath(level);
                    if (string.IsNullOrEmpty(path)) continue;
                    scenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---------------------
    // Maps - Group / Level Set editing
    // ---------------------
    private SerializedProperty GetGroupProperty(string _groupId)
    {
        if (m_LevelOrganizerSO == null) return null;
        m_LevelOrganizerSO.Update();

        SerializedProperty groupsProp = m_LevelOrganizerSO.FindProperty(nameof(LevelOrganizer.LevelSetGroups));
        for (int i = 0; i < groupsProp.arraySize; i++)
        {
            SerializedProperty groupProp = groupsProp.GetArrayElementAtIndex(i);
            SerializedProperty idProp = groupProp.FindPropertyRelative(nameof(LevelSetGroup.id));
            if (idProp != null && idProp.stringValue == _groupId) return groupProp;
        }
        return null;
    }

    private SerializedProperty GetLevelSetProperty(string _groupId, int _levelSetIndex)
    {
        SerializedProperty groupProp = GetGroupProperty(_groupId);
        if (groupProp == null) return null;

        SerializedProperty levelSetsProp = groupProp.FindPropertyRelative(nameof(LevelSetGroup.levelSets));
        if (levelSetsProp == null || _levelSetIndex < 0 || _levelSetIndex >= levelSetsProp.arraySize) return null;

        return levelSetsProp.GetArrayElementAtIndex(_levelSetIndex);
    }

    private void BuildGroupEditor(LevelSetGroup _group)
    {
        SerializedProperty groupProp = GetGroupProperty(_group.id);
        if (groupProp == null)
        {
            m_MapsContent.Add(new HelpBox("This group could not be found.", HelpBoxMessageType.Warning));
            return;
        }

        SerializedProperty nameProp = groupProp.FindPropertyRelative(nameof(LevelSetGroup.name));
        SerializedProperty descProp = groupProp.FindPropertyRelative(nameof(LevelSetGroup.description));
        SerializedProperty levelSetsProp = groupProp.FindPropertyRelative(nameof(LevelSetGroup.levelSets));

        VisualElement headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 } };

        if (m_MapsGroupEditMode)
        {
            TextField nameField = new TextField
            {
                value = nameProp.stringValue,
                style = { flexGrow = 1, flexShrink = 1, minWidth = 0, fontSize = 16 }
            };
            nameField.RegisterValueChangedCallback(evt =>
            {
                nameProp.stringValue = evt.newValue;
                m_LevelOrganizerSO.ApplyModifiedProperties();
            });
            headerRow.Add(nameField);
        }
        else
        {
            Label titleLabel = new Label(string.IsNullOrWhiteSpace(nameProp.stringValue) ? "(Unnamed Group)" : nameProp.stringValue)
            {
                style = { fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1, flexShrink = 1, minWidth = 0 }
            };
            headerRow.Add(titleLabel);
        }

        Button editToggleButton = new Button(ToggleGroupEditMode) { text = m_MapsGroupEditMode ? "Done" : "\u270E Edit" };
        editToggleButton.style.width = 70;
        editToggleButton.style.flexShrink = 0;
        headerRow.Add(editToggleButton);
        m_MapsContent.Add(headerRow);

        if (m_MapsGroupEditMode)
        {
            TextField descField = new TextField("Description") { value = descProp.stringValue, multiline = true };
            descField.style.minHeight = 40;
            descField.style.marginBottom = 8;
            descField.RegisterValueChangedCallback(evt =>
            {
                descProp.stringValue = evt.newValue;
                m_LevelOrganizerSO.ApplyModifiedProperties();
            });
            m_MapsContent.Add(descField);
        }
        else if (!string.IsNullOrWhiteSpace(descProp.stringValue))
        {
            Label descLabel = new Label(descProp.stringValue)
            {
                style = { whiteSpace = WhiteSpace.Normal, opacity = 0.85f, marginBottom = 8 }
            };
            m_MapsContent.Add(descLabel);
        }

        VisualElement separator = new VisualElement
        {
            style = { height = 1, backgroundColor = new StyleColor(new Color(1, 1, 1, 0.1f)), marginBottom = 10 }
        };
        m_MapsContent.Add(separator);

        int levelSetCount = levelSetsProp.arraySize;
        for (int i = 0; i < levelSetCount; i++)
        {
            m_MapsContent.Add(BuildLevelSetEditor(_group.id, i));
        }

        if (m_MapsGroupEditMode)
        {
            Button addLevelSetButton = new Button(() => AddLevelSet(_group.id)) { text = "Add Level Set" };
            addLevelSetButton.style.marginTop = 4;
            m_MapsContent.Add(addLevelSetButton);
        }
    }

    private void ToggleGroupEditMode()
    {
        m_MapsGroupEditMode = !m_MapsGroupEditMode;
        RefreshMapsContent();
    }

    private VisualElement BuildLevelSetEditor(string _groupId, int _index)
    {
        SerializedProperty levelSetProp = GetLevelSetProperty(_groupId, _index);
        if (levelSetProp == null) return new VisualElement();

        SerializedProperty nameProp = levelSetProp.FindPropertyRelative(nameof(LevelSet.name));
        SerializedProperty descProp = levelSetProp.FindPropertyRelative(nameof(LevelSet.description));
        SerializedProperty levelsProp = levelSetProp.FindPropertyRelative(nameof(LevelSet.levels));

        Foldout foldout = new Foldout
        {
            text = string.IsNullOrWhiteSpace(nameProp.stringValue) ? $"Level Set {_index + 1}" : nameProp.stringValue,
            value = true
        };
        foldout.AddToClassList("levelset-row");
        foldout.style.marginBottom = 10;
        foldout.style.paddingLeft = 6;
        foldout.style.borderLeftWidth = 2;
        foldout.style.borderLeftColor = new StyleColor(new Color(1, 1, 1, 0.08f));

        if (m_MapsGroupEditMode)
        {
            VisualElement headerControls = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 } };
            Label dragHandle = new Label("\u2261")
            {
                tooltip = "Drag to reorder",
                style = { width = 14, opacity = 0.4f, unityTextAlign = TextAnchor.MiddleCenter, marginRight = 4 }
            };
            headerControls.Add(dragHandle);
            foldout.Add(headerControls);
            RegisterLevelSetRowDrag(foldout, dragHandle, _groupId);

            TextField setNameField = new TextField("Name") { value = nameProp.stringValue };
            setNameField.RegisterValueChangedCallback(evt =>
            {
                nameProp.stringValue = evt.newValue;
                m_LevelOrganizerSO.ApplyModifiedProperties();
                foldout.text = string.IsNullOrWhiteSpace(evt.newValue) ? $"Level Set {_index + 1}" : evt.newValue;
            });
            foldout.Add(setNameField);

            TextField setDescField = new TextField("Description") { value = descProp.stringValue, multiline = true };
            setDescField.style.minHeight = 32;
            setDescField.RegisterValueChangedCallback(evt =>
            {
                descProp.stringValue = evt.newValue;
                m_LevelOrganizerSO.ApplyModifiedProperties();
            });
            foldout.Add(setDescField);

            ListView levelsListView = new ListView
            {
                showBorder = true,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                fixedItemHeight = 22,
                style = { minHeight = 30, marginTop = 4, marginBottom = 4 }
            };

            levelsListView.makeItem = () =>
            {
                ObjectField sceneField = new ObjectField
                {
                    name = "SceneField",
                    objectType = typeof(SceneAsset),
                    style = { flexGrow = 1 }
                };
                return sceneField;
            };

            levelsListView.bindItem = (element, index) =>
            {
                ObjectField sceneField = (ObjectField)element;
                SerializedProperty levelProp = levelsProp.GetArrayElementAtIndex(index);
                SerializedProperty sceneAssetProp = levelProp.FindPropertyRelative("sceneAsset");
                if (sceneAssetProp != null)
                {
                    sceneField.BindProperty(sceneAssetProp);
                }
            };

            levelsListView.BindProperty(levelsProp);
            foldout.Add(levelsListView);

            Button newMapButton = new Button(() => CreateNewMapFromTemplate(_groupId, _index)) { text = "New Map From Template" };
            newMapButton.style.marginTop = 4;
            foldout.Add(newMapButton);

            Button deleteSetButton = new Button(() => DeleteLevelSet(_groupId, _index)) { text = "Delete Level Set" };
            deleteSetButton.style.marginTop = 4;
            foldout.Add(deleteSetButton);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(descProp.stringValue))
            {
                Label descLabel = new Label(descProp.stringValue)
                {
                    style = { whiteSpace = WhiteSpace.Normal, opacity = 0.8f, marginBottom = 4 }
                };
                foldout.Add(descLabel);
            }

            int levelCount = levelsProp.arraySize;
            if (levelCount == 0)
            {
                foldout.Add(new Label("No scenes in this level set.")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Italic, opacity = 0.6f }
                });
            }
            else
            {
                for (int i = 0; i < levelCount; i++)
                {
                    SerializedProperty levelProp = levelsProp.GetArrayElementAtIndex(i);
                    SerializedProperty sceneAssetProp = levelProp.FindPropertyRelative("sceneAsset");
                    Object sceneAsset = sceneAssetProp != null ? sceneAssetProp.objectReferenceValue : null;
                    bool missing = sceneAsset == null;
                    string displayName = missing ? "(missing scene)" : sceneAsset.name;

                    Button sceneButton = new Button(() => OpenSceneAsset(sceneAsset))
                    {
                        text = displayName,
                        tooltip = missing ? "This scene reference is missing" : $"Click to open {displayName}"
                    };
                    sceneButton.SetEnabled(!missing);
                    sceneButton.style.marginBottom = 2;
                    sceneButton.style.unityTextAlign = TextAnchor.MiddleLeft;
                    if (missing)
                    {
                        sceneButton.style.color = new StyleColor(new Color(0.9f, 0.4f, 0.4f));
                    }
                    foldout.Add(sceneButton);
                }
            }
        }

        return foldout;
    }

    private void OpenSceneAsset(Object _sceneAsset)
    {
        if (_sceneAsset == null) return;

        string path = AssetDatabase.GetAssetPath(_sceneAsset);
        if (string.IsNullOrEmpty(path)) return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    private void AddNewGroup()
    {
        Undo.RecordObject(m_LevelOrganizer, "Add Level Set Group");
        if (m_LevelOrganizer.LevelSetGroups == null) m_LevelOrganizer.LevelSetGroups = new List<LevelSetGroup>();

        LevelSetGroup newGroup = new LevelSetGroup
        {
            id = Guid.NewGuid().ToString(),
            name = "New Group",
            levelSets = new List<LevelSet>()
        };
        m_LevelOrganizer.LevelSetGroups.Add(newGroup);
        EditorUtility.SetDirty(m_LevelOrganizer);
        m_LevelOrganizerSO.Update();

        SelectGroup(newGroup.id);
        m_MapsGroupEditMode = true;
        RefreshMapsContent();
    }

    private void DeleteGroup(string _groupId)
    {
        LevelSetGroup group = m_LevelOrganizer.LevelSetGroups?.FirstOrDefault(g => g != null && g.id == _groupId);
        if (group == null) return;

        if (!EditorUtility.DisplayDialog("Delete Group",
                $"Delete group '{group.name}' and all of its level sets? This can't easily be undone.", "Delete", "Cancel"))
        {
            return;
        }

        Undo.RecordObject(m_LevelOrganizer, "Delete Level Set Group");
        m_LevelOrganizer.LevelSetGroups.Remove(group);
        m_LevelOrganizer.BuildListGroupIds?.Remove(_groupId);
        EditorUtility.SetDirty(m_LevelOrganizer);
        m_LevelOrganizerSO.Update();
        SyncBuildSettingsFromBuildList();

        if (m_SelectedGroupId == _groupId)
        {
            SelectBuildList();
        }
        else
        {
            RefreshMapsTab();
        }
    }

    private void AddLevelSet(string _groupId)
    {
        LevelSetGroup group = m_LevelOrganizer.LevelSetGroups?.FirstOrDefault(g => g != null && g.id == _groupId);
        if (group == null) return;

        Undo.RecordObject(m_LevelOrganizer, "Add Level Set");
        if (group.levelSets == null) group.levelSets = new List<LevelSet>();
        group.levelSets.Add(new LevelSet { name = "New Level Set", levels = new List<SceneReference>() });
        EditorUtility.SetDirty(m_LevelOrganizer);
        m_LevelOrganizerSO.Update();

        if (m_LevelOrganizer.BuildListGroupIds != null && m_LevelOrganizer.BuildListGroupIds.Contains(_groupId))
        {
            SyncBuildSettingsFromBuildList();
        }
        RefreshMapsTab();
    }

    private void DeleteLevelSet(string _groupId, int _index)
    {
        LevelSetGroup group = m_LevelOrganizer.LevelSetGroups?.FirstOrDefault(g => g != null && g.id == _groupId);
        if (group?.levelSets == null || _index < 0 || _index >= group.levelSets.Count) return;

        if (!EditorUtility.DisplayDialog("Delete Level Set",
                $"Delete level set '{group.levelSets[_index].name}'? This can't easily be undone.", "Delete", "Cancel"))
        {
            return;
        }

        Undo.RecordObject(m_LevelOrganizer, "Delete Level Set");
        group.levelSets.RemoveAt(_index);
        EditorUtility.SetDirty(m_LevelOrganizer);
        m_LevelOrganizerSO.Update();

        if (m_LevelOrganizer.BuildListGroupIds != null && m_LevelOrganizer.BuildListGroupIds.Contains(_groupId))
        {
            SyncBuildSettingsFromBuildList();
        }
        RefreshMapsTab();
    }

    // ---------------------
    // Maps - Level Set drag-to-reorder
    // ---------------------
    private void RegisterLevelSetRowDrag(VisualElement _row, VisualElement _dragHandle, string _groupId)
    {
        _dragHandle.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button != 0) return;
            m_LevelSetDragGroupId = _groupId;
            m_LevelSetDragRow = _row;
            Undo.RecordObject(m_LevelOrganizer, "Reorder Level Sets");
            _dragHandle.CapturePointer(evt.pointerId);
        });

        _dragHandle.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (!_dragHandle.HasPointerCapture(evt.pointerId) || m_LevelSetDragRow == null) return;

            VisualElement container = m_MapsContent;
            Vector2 localPos = container.WorldToLocal(evt.position);

            List<VisualElement> rows = container.Children().Where(c => c.ClassListContains("levelset-row")).ToList();
            int currentIndex = rows.IndexOf(m_LevelSetDragRow);
            if (currentIndex < 0) return;

            int targetIndex = currentIndex;
            VisualElement targetSibling = null;
            for (int i = 0; i < rows.Count; i++)
            {
                Rect bounds = rows[i].layout;
                if (localPos.y >= bounds.yMin && localPos.y <= bounds.yMax)
                {
                    targetIndex = i;
                    targetSibling = rows[i];
                    break;
                }
            }

            if (targetIndex != currentIndex && targetSibling != null)
            {
                container.Insert(container.IndexOf(targetSibling), m_LevelSetDragRow);
                MoveLevelSetToIndex(m_LevelSetDragGroupId, currentIndex, targetIndex);
            }
        });

        _dragHandle.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (_dragHandle.HasPointerCapture(evt.pointerId))
                _dragHandle.ReleasePointer(evt.pointerId);

            if (m_LevelSetDragGroupId != null)
            {
                EditorUtility.SetDirty(m_LevelOrganizer);
                m_LevelOrganizerSO.Update();

                if (m_LevelOrganizer.BuildListGroupIds != null && m_LevelOrganizer.BuildListGroupIds.Contains(m_LevelSetDragGroupId))
                {
                    SyncBuildSettingsFromBuildList();
                }
                RefreshMapsContent();
            }

            m_LevelSetDragGroupId = null;
            m_LevelSetDragRow = null;
        });
    }

    private void MoveLevelSetToIndex(string _groupId, int _oldIndex, int _newIndex)
    {
        LevelSetGroup group = m_LevelOrganizer.LevelSetGroups?.FirstOrDefault(g => g != null && g.id == _groupId);
        if (group?.levelSets == null) return;
        if (_oldIndex < 0 || _oldIndex >= group.levelSets.Count) return;

        _newIndex = Mathf.Clamp(_newIndex, 0, group.levelSets.Count - 1);
        if (_newIndex == _oldIndex) return;

        LevelSet levelSet = group.levelSets[_oldIndex];
        group.levelSets.RemoveAt(_oldIndex);
        group.levelSets.Insert(_newIndex, levelSet);
    }

    // ---------------------
    // Maps - New map from template
    // ---------------------
    private void CreateNewMapFromTemplate(string _groupId, int _levelSetIndex)
    {
        SceneAsset template = m_Settings != null ? m_Settings.m_TemplateMapScene : null;
        if (template == null)
        {
            EditorUtility.DisplayDialog("No Template Map Set",
                "Assign a Template Map Scene in the Project Settings tab first.", "OK");
            return;
        }

        string templatePath = AssetDatabase.GetAssetPath(template);
        string defaultDirectory = System.IO.Path.GetDirectoryName(templatePath);

        string newPath = EditorUtility.SaveFilePanelInProject(
            "New Map From Template",
            "New Map",
            "unity",
            "Choose a name and location for the new map.",
            defaultDirectory);

        if (string.IsNullOrEmpty(newPath)) return;

        if (!AssetDatabase.CopyAsset(templatePath, newPath))
        {
            EditorUtility.DisplayDialog("Copy Failed", $"Couldn't copy the template scene to '{newPath}'.", "OK");
            return;
        }
        AssetDatabase.Refresh();

        SceneAsset newSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(newPath);
        LevelSetGroup group = m_LevelOrganizer.LevelSetGroups?.FirstOrDefault(g => g != null && g.id == _groupId);
        if (group?.levelSets == null || _levelSetIndex < 0 || _levelSetIndex >= group.levelSets.Count)
        {
            EditorUtility.DisplayDialog("Scene Created",
                $"The scene was created at '{newPath}', but the target level set couldn't be found to add it automatically.", "OK");
            return;
        }

        Undo.RecordObject(m_LevelOrganizer, "Add New Map From Template");
        LevelSet levelSet = group.levelSets[_levelSetIndex];
        if (levelSet.levels == null) levelSet.levels = new List<SceneReference>();
        levelSet.levels.Add(SceneReferenceUtil.CreateFromSceneAsset(newSceneAsset));
        EditorUtility.SetDirty(m_LevelOrganizer);
        m_LevelOrganizerSO.Update();

        if (m_LevelOrganizer.BuildListGroupIds != null && m_LevelOrganizer.BuildListGroupIds.Contains(_groupId))
        {
            SyncBuildSettingsFromBuildList();
        }
        RefreshMapsTab();
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