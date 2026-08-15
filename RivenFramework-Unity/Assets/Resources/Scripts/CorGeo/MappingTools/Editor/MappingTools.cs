using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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

    private Toolbar m_AssetLabelsToolbar;
    private enum AssetViewMode { Grid, List };
    private AssetViewMode m_ViewMode = AssetViewMode.Grid;
    private List<Object> m_AssetResults = new List<Object>();
    private ListView m_AssetsResultsList;
    private ScrollView m_AssetResultGridScroll;
    private VisualElement m_AssetResultGrid;
    private HashSet<int> m_ResolvedPreviewIndices = new HashSet<int>();

    private const string SettingsAssetPath = "Assets/Resources/Scripts/CorGeo/MappingTools/MappingToolsSettings.asset";

    private VisualElement m_Root;
    
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

    [MenuItem("Neverway/MappingTools")]
    public static void ShowExample()
    {
        MappingTools wnd = GetWindow<MappingTools>();
        wnd.titleContent = new GUIContent("MappingTools");
    }

    public void CreateGUI()
    {
        m_Root = rootVisualElement;

        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        m_Root.Add(labelFromUXML);
        
        m_StatusSection = m_Root.Q<VisualElement>("LevelStatusGroup");
        m_AssetsSection = m_Root.Q<VisualElement>("MapperAssetsGroup");
        m_SettingsSection = m_Root.Q<VisualElement>("ProjectSettings");

        m_Root.Q<Button>("LevelStatusButton").clicked += () => ShowSection(m_StatusSection);
        m_Root.Q<Button>("MapperAssetsButton").clicked += () => ShowSection(m_AssetsSection);
        m_Root.Q<Button>("ProjectSettingsButton").clicked += () => ShowSection(m_SettingsSection);

        ShowSection(m_StatusSection);
        
        //var serializedObject = new SerializedObject(this);
        //m_Root.Bind(serializedObject);

        m_Settings = LoadOrCreateSettings();
        var serializedObject = new SerializedObject(m_Settings);
        m_Root.Bind(serializedObject);
        
        m_Root.TrackSerializedObjectValue(serializedObject, _ =>
        {
            EditorUtility.SetDirty(m_Settings);
        });

        m_AssetLabelsToolbar = m_Root.Q<Toolbar>("AssetLabelsToolbar");
        m_AssetsResultsList = m_Root.Q<ListView>("AssetResultList");
        m_AssetResultGridScroll = m_Root.Q<ScrollView>("AssetResultGridScroll");
        m_AssetResultGrid = m_Root.Q<VisualElement>("AssetResultGrid");
        
        m_Root.Q<ToolbarButton>("ViewGrid").clicked += () => SetViewMode(AssetViewMode.Grid);
        m_Root.Q<ToolbarButton>("ViewListButton").clicked += () => SetViewMode(AssetViewMode.List);

        SetUpAssetResultsList();
        PopulateAssetLabelToolbar();

        SerializedProperty assetLabelsProperty = serializedObject.FindProperty(nameof(MappingToolsSettings.m_AssetLabels));
        m_Root.TrackPropertyValue(assetLabelsProperty, _ => PopulateAssetLabelToolbar());
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
    }

    private void PopulateAssetLabelToolbar()
    {
        m_AssetLabelsToolbar.Clear();

        foreach (var entry in m_Settings.m_AssetLabels)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.m_Label)) continue;
            
            string buttonText = string.IsNullOrWhiteSpace(entry.m_DisplayName) ? entry.m_Label : entry.m_DisplayName;

            Button labelButton = new Button(() => ShowAssetsWithLabel(entry.m_Label))
            {
                text = buttonText
            };
            m_AssetLabelsToolbar.Add(labelButton);
        }
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
            return row;
        };

        m_AssetsResultsList.bindItem = (element, index) =>
        {
            Object asset = m_AssetResults[index];
            element.Q<Image>("Icon").image = AssetPreview.GetMiniThumbnail(asset);
            element.Q<Label>("Name").text = asset.name;
        };

        m_AssetsResultsList.itemsSource = m_AssetResults;
        m_AssetsResultsList.selectionType = SelectionType.Single;
        m_AssetsResultsList.fixedItemHeight = 20;

        m_AssetsResultsList.selectionChanged += selection =>
        {
            if (selection.FirstOrDefault() is Object selectedAsset) EditorGUIUtility.PingObject(selectedAsset);
        };
    }

    private void ShowAssetsWithLabel(string _label)
    {
        m_AssetResults.Clear();

        string[] guids = AssetDatabase.FindAssets($"l:{_label}");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null) m_AssetResults.Add(asset);
        }
        
        RefreshResultsView();
    }

    private void OnLostFocus()
    {
        SaveSettings();
    }

    private void OnDestroy()
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        if (m_Settings == null) return;
        AssetDatabase.SaveAssets();
    }

    private void SetViewMode(AssetViewMode _mode)
    {
        m_ViewMode = _mode;
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
        
        AssetPreview.SetPreviewTextureCacheSize(Mathf.Max(m_AssetResults.Count + 32, 256));

        for (int i = 0; i < m_AssetResults.Count; i++)
        {
            Object asset = m_AssetResults[i];
            
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

            Label nameLabel = new Label(asset.name)
            {
                style = { width = 72, fontSize = 10, unityTextAlign = TextAnchor.UpperCenter, whiteSpace = WhiteSpace.Normal, marginTop = 2 }
            };
            
            card.Add(preview);
            card.Add(nameLabel);
            
            card.RegisterCallback<ClickEvent>(_ => EditorGUIUtility.PingObject(asset));

            m_AssetResultGrid.Add(card);
        }
        
        int elapsedMs = 0;
        const int intervalMs = 200;
        const int timeoutMs = 5000;

        m_Root.schedule.Execute(() => { RefreshLatePreviews(); elapsedMs += intervalMs; }).Every(intervalMs).Until(() => m_ResolvedPreviewIndices.Count >= m_AssetResults.Count || elapsedMs >= timeoutMs);
    }
    

    private void RefreshLatePreviews()
    {
        for (int i = 0; i < m_AssetResults.Count; i++)
        {
            if (m_ResolvedPreviewIndices.Contains(i)) continue;
            if (i >= m_AssetResultGrid.childCount) break;

            Texture2D preview = AssetPreview.GetAssetPreview(m_AssetResults[i]);
            if (preview != null)
            {
                Image img = m_AssetResultGrid[i].Q<Image>();
                if (img != null) img.image = preview;
                m_ResolvedPreviewIndices.Add(i);
            }
        }
    }
}