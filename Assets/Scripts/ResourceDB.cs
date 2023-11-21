using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class ResourceItem
{
    public enum Type
    {
        Unknown = 0,
        Any = 0,
        Folder = 1,
        Asset = 2
    }

    [SerializeField] private string name;

    [SerializeField] private string ext;

    [SerializeField] private string path;

    [SerializeField] private Type type = Type.Unknown;

    [SerializeField] private string objectTypeName;

    internal Dictionary<string, ResourceItem> childs;
    private System.Type objectType;

    public ResourceItem()
    {
        if (type == Type.Folder)
            childs = new Dictionary<string, ResourceItem>();
    }

    public ResourceItem(string aFileName, string aPath, Type aType, string aObjectType)
    {
        var index = aFileName.LastIndexOf(".");
        if (index > 0)
        {
            name = aFileName.Substring(0, index);
            ext = aFileName.Substring(index + 1);
        }
        else
        {
            name = aFileName;
            ext = "";
        }

        path = aPath;
        type = aType;
        objectTypeName = aObjectType;
        objectType = System.Type.GetType(objectTypeName);
        if (type == Type.Folder)
            childs = new Dictionary<string, ResourceItem>();
    }

    public string Name => name;
    public string Ext => ext;
    public string Path => path;
    public string ResourcesPath => string.IsNullOrEmpty(path) ? name : path + "/" + name;
    public Type ResourcesType => type;
    public ResourceItem Parent { get; private set; }

    public ResourceItem GetChild(string aPath, Type aResourceType = Type.Any)
    {
        if (type != Type.Folder)
            return null;
        var p = aPath;
        var index = aPath.IndexOf('/');
        if (index > 0)
        {
            p = aPath.Substring(0, index);
            aPath = aPath.Substring(index + 1);
        }
        else
        {
            aPath = "";
        }

        ResourceItem item = null;
        if (!childs.TryGetValue(p, out item) || item == null)
            return null;
        if (aPath.Length > 0)
            return item.GetChild(aPath, aResourceType);
        if (aResourceType != Type.Unknown && item.type != aResourceType)
            return null;
        return item;
    }

    public IEnumerable<ResourceItem> GetChilds(string aName, Type aResourceType = Type.Any,
        bool aSearchSubFolders = false, System.Type aAssetType = null)
    {
        if (type == Type.Asset) // assets don't have childs
            yield break;
        var checkName = !string.IsNullOrEmpty(aName);
        var typeCheck = aAssetType != null;
        var items = childs.Values;
        foreach (var item in items)
        {
            if (aResourceType != Type.Any && item.type != aResourceType)
                continue;
            if (checkName && aName != item.Name)
                continue;
            if (typeCheck && !aAssetType.IsAssignableFrom(item.objectType))
                continue;
            yield return item;
        }

        if (aSearchSubFolders)
            foreach (var folder in items.Where(i => i.type == Type.Folder))
            foreach (var item in folder.GetChilds(aName, aResourceType, aSearchSubFolders, aAssetType))
                yield return item;
    }

    public T Load<T>() where T : Object
    {
        //Debug.Log("Load: " + ResourcesPath + " / " + typeof(T).Name);
        return Resources.Load<T>(ResourcesPath);
    }

    internal void OnDeserialize()
    {
        if (string.IsNullOrEmpty(path))
            Parent = ResourceDB.Instance.root;
        else
            Parent = ResourceDB.GetFolder(path);
        if (Parent != null)
            Parent.childs.TryAdd(name, this);
        if (type == Type.Folder) childs = new Dictionary<string, ResourceItem>();
        objectType = System.Type.GetType(objectTypeName);
    }
}

public class ResourceDB : ScriptableObject, ISerializationCallbackReceiver
{
    private static ResourceDB m_Instance;

    [SerializeField] internal List<ResourceItem> items = new();

    [SerializeField] [HideInInspector] private int m_FileCount;

    [SerializeField] [HideInInspector] private int m_FolderCount;

    [SerializeField] [HideInInspector] public bool UpdateAutomatically;

    internal ResourceItem root = new("", "", ResourceItem.Type.Folder, "");

    public ResourceDB()
    {
        m_Instance = this;
    }

    public static ResourceDB Instance
    {
        get
        {
            if (m_Instance != null)
                return m_Instance;
            m_Instance = FindInstance();
            if (m_Instance != null)
                return m_Instance;
            m_Instance = CreateInstance<ResourceDB>();
#if UNITY_EDITOR
            var resDir = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources"));
            if (!resDir.Exists)
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateAsset(m_Instance, "Assets/Resources/ResourceDB.asset");
            m_Instance = FindInstance();
#endif
            return m_Instance;
        }
    }

    public int FileCount => m_FileCount;
    public int FolderCount => m_FolderCount;

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (items == null || items.Count == 0) UpdateDB();
#endif
    }

    public void OnAfterDeserialize()
    {
        root.childs.Clear();
        foreach (var item in items)
            if (item != null)
                item.OnDeserialize();
    }

    public static ResourceDB FindInstance()
    {
        return Resources.Load<ResourceDB>("ResourceDB");
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Update ResourceDB")]
    internal static void TriggerUpdate()
    {
        Instance.UpdateDB();
    }
#endif

    public static ResourceItem GetFolder(string aPath)
    {
        return Instance.root.GetChild(aPath, ResourceItem.Type.Folder);
    }

    public static IEnumerable<ResourceItem> GetAllAssets(string aName, Type aAssetType = null)
    {
        return Instance.root.GetChilds(aName, ResourceItem.Type.Asset, true, aAssetType);
    }

    public static IEnumerable<ResourceItem> GetAllAssets<T>(string aName) where T : Object
    {
        return GetAllAssets(aName, typeof(T));
    }

    public static ResourceItem GetAsset(string aName, Type aAssetType = null)
    {
        return Instance.root.GetChilds(aName, ResourceItem.Type.Asset, true, aAssetType).FirstOrDefault();
    }

    public static string ConvertPath(string aPath)
    {
        return aPath.Replace("\\", "/");
    }

#if UNITY_EDITOR
    private void ScanFolder(DirectoryInfo aFolder, List<DirectoryInfo> aList, bool aOnlyTopFolders)
    {
        var n = aFolder.Name.ToLower();
        if (n == "editor") // ignore folders
            return;
        if (n == "resources")
        {
            aList.Add(aFolder);
            if (aOnlyTopFolders)
                return;
        }

        foreach (var dir in aFolder.GetDirectories()) ScanFolder(dir, aList, aOnlyTopFolders);
    }

    private List<DirectoryInfo> FindResourcesFolders(bool aOnlyTopFolders)
    {
        var assets = new DirectoryInfo(Application.dataPath);
        var list = new List<DirectoryInfo>();
        ScanFolder(assets, list, aOnlyTopFolders);
        return list;
    }

    private void AddFileList(DirectoryInfo aFolder, int aPrefix)
    {
        var relFolder = aFolder.FullName;
        if (relFolder.Length < aPrefix)
            relFolder = "";
        else
            relFolder = relFolder.Substring(aPrefix);
        relFolder = ConvertPath(relFolder);
        foreach (var folder in aFolder.GetDirectories())
        {
            items.Add(new ResourceItem(folder.Name, relFolder, ResourceItem.Type.Folder, ""));
            AddFileList(folder, aPrefix);
        }

        foreach (var file in aFolder.GetFiles())
        {
            var ext = file.Extension.ToLower();
            if (ext == ".meta")
                continue;
            var assetPath = "assets/" + file.FullName.Substring(Application.dataPath.Length + 1);
            assetPath = ConvertPath(assetPath);
            var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            if (obj == null)
            {
                Debug.LogWarning("ResourceDB: File at path " + assetPath +
                                 " couldn't be loaded and is ignored. Probably not an asset?!");
                continue;
            }

            var type = obj.GetType().AssemblyQualifiedName;
            items.Add(new ResourceItem(file.Name, relFolder, ResourceItem.Type.Asset, type));
        }

        Resources.UnloadUnusedAssets();
    }

    public void UpdateDB(bool aSetDirty = false)
    {
        items.Clear();
        root.childs.Clear();
        var topFolders = FindResourcesFolders(true);

        foreach (var folder in topFolders)
        {
            var path = folder.FullName;
            var prefix = path.Length;
            if (!path.EndsWith("/"))
                prefix++;
            AddFileList(folder, prefix);
        }

        m_FolderCount = 0;
        m_FileCount = 0;
        foreach (var item in items)
            if (item.ResourcesType == ResourceItem.Type.Folder)
                m_FolderCount++;
            else if (item.ResourcesType == ResourceItem.Type.Asset)
                m_FileCount++;
        if (aSetDirty)
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
#endif
}


#if UNITY_EDITOR

public class ResourceDBPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (ResourceDB.FindInstance() == null)
            return;
        if (!ResourceDB.Instance.UpdateAutomatically)
            return;
        var files = importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths);
        var update = false;
        foreach (var file in files)
        {
            var fn = file.ToLower();
            if (!fn.Contains("resourcedb.asset") && fn.Contains("/resources/"))
            {
                update = true;
                break;
            }
        }

        if (update) ResourceDB.Instance.UpdateDB();
    }
}
#endif