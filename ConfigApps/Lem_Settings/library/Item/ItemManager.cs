// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lemoine.FileRepository;
using Lemoine.Info;
using Lemoine.Core.Log;

namespace Lemoine.Settings
{
  /// <summary>
  /// This singleton class ItemManager loads all Items present
  /// in dlls next to the executable
  /// </summary>
  public sealed class ItemManager
  {
    static readonly string ITEM_INSTALLATION_DIRECTORY_KEY = "Settings.InstallItemsDirectory";
    static readonly string ITEM_INSTALLATION_DIRECTORY_DEFAULT = "settings.d";

    static readonly string ALTERNATIVE_SETTINGS_DIRECTORY_KEY = "Settings.AlternativeItemsDirectory";
    static readonly string ALTERNATIVE_SETTINGS_DIRECTORY_DEFAULT = "";

    static readonly ILog log = LogManager.GetLogger (typeof (ItemManager).FullName);

    #region Members
    List<IItem> m_items = new List<IItem> ();
    readonly DirectoryInfo m_itemInstallDir = null;
    readonly DirectoryInfo m_itemUserDir = new DirectoryInfo (Path.Combine (
      PulseInfo.LocalConfigurationDirectory, LemSettingsGlobal.GetSyncSettingsDirectory ()));
    readonly DirectoryInfo m_itemAlternativeDir;
    bool m_itemAlternativeDirValid;
    Dictionary<Type, IList<IItem>> m_interactionsReadOnly = new Dictionary<Type, IList<IItem>> ();
    Dictionary<Type, IList<IItem>> m_interactions = new Dictionary<Type, IList<IItem>> ();
    #endregion // Members

    #region Event
    /// <summary>
    /// Event raised when items changed
    /// </summary>
    public static event Action ItemsChanged;
    #endregion // Event

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class!)
    /// </summary>
    private ItemManager ()
    {
      var itemInstallationDirectory = Lemoine.Info.ConfigSet
        .LoadAndGet<string> (ITEM_INSTALLATION_DIRECTORY_KEY, ITEM_INSTALLATION_DIRECTORY_DEFAULT);
      foreach (var installDirectory in GetInstallDirectories ()) {
        var path = Path.Combine (installDirectory, itemInstallationDirectory);
        if (Directory.Exists (path)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ItemManager: item installation directory is {m_itemInstallDir}");
          }
          m_itemInstallDir = new DirectoryInfo (path);
          break;
        }
      }
      if (null == m_itemInstallDir) {
        log.Fatal ($"ItemManager: {itemInstallationDirectory} directory not found in various possible installation directories => use the program directory {Lemoine.Info.ProgramInfo.AbsoluteDirectory} by default");
        m_itemInstallDir = new DirectoryInfo (Lemoine.Info.ProgramInfo.AbsoluteDirectory);
      }

      var itemAlternativeDir = Lemoine.Info.ConfigSet.LoadAndGet<string> (ALTERNATIVE_SETTINGS_DIRECTORY_KEY,
                                                        ALTERNATIVE_SETTINGS_DIRECTORY_DEFAULT);
      if (string.IsNullOrEmpty (itemAlternativeDir)) {
        m_itemAlternativeDirValid = false;
      }
      else {
        m_itemAlternativeDir = new DirectoryInfo (itemAlternativeDir);
        m_itemAlternativeDirValid = m_itemAlternativeDir.Exists;
      }
    }

    static IEnumerable<string> GetInstallDirectories ()
    {
      var programDirectory = Lemoine.Info.ProgramInfo.AbsoluteDirectory;
      var directories = new string[] { programDirectory, Path.Combine (programDirectory, ".."), System.Environment.CurrentDirectory };
      foreach (var directory in directories) {
        yield return directory;
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Refresh all items based on local files
    /// </summary>
    public static void RefreshItems ()
    {
      Instance.LoadItems ();
    }

    /// <summary>
    /// Retrieve items with 3 possible filters
    /// - all or a specific category
    /// - only favorite items
    /// - only items that match (totally or partially) a series of keywords
    /// </summary>
    /// <param name="category">can be null or empty</param>
    /// <param name="onlyFavorite"></param>
    /// <param name="keywordsFilter">can be null or empty</param>
    /// <returns></returns>
    public static IList<IItem> GetItems (string category, bool onlyFavorite, ICollection<string> keywordsFilter)
    {
      var items = new List<IItem> ();

      bool applyCategoryFilter = !String.IsNullOrEmpty (category);
      bool applyKeywordsFilter = (keywordsFilter != null && keywordsFilter.Count > 0);

      DateTime? oldestLastUsed = null;
      DateTime? newestLastUsed = null;
      foreach (IItem item in Instance.m_items) {
        // The item must not be hidden
        bool itemOk = ((item.Flags & LemSettingsGlobal.ItemFlag.HIDDEN) == 0);

        // The category must match
        if (itemOk && applyCategoryFilter) {
          string itemCategory = item.Category;
          if (itemCategory == "") {
            itemCategory = LemSettingsGlobal.DEFAULT_CATEGORY;
          }

          itemOk = (category == itemCategory);
        }

        // Among the favorite items
        if (itemOk && onlyFavorite) {
          itemOk = ((item.Flags & LemSettingsGlobal.ItemFlag.FAVORITE) != 0);
        }

        // Keywords must partially or totally match
        if (itemOk && applyKeywordsFilter) {
          item.Score = KeywordSearch.GetScore (item.FullKeywords, keywordsFilter);
          itemOk &= (item.Score > 0);
        }
        else {
          item.Score = 100.0;
        }

        if (itemOk) {
          if (item.LastUsed.Year > 2000) {
            if (!oldestLastUsed.HasValue || item.LastUsed < oldestLastUsed) {
              oldestLastUsed = item.LastUsed;
            }

            if (!newestLastUsed.HasValue || item.LastUsed > newestLastUsed) {
              newestLastUsed = item.LastUsed;
            }
          }
          items.Add (item);
        }
      }

      // Take into account the items that have been used recently
      if (oldestLastUsed.HasValue && newestLastUsed.HasValue && oldestLastUsed != newestLastUsed) {
        foreach (IItem item in items) {
          // Position of the item between the oldest and newest dates
          double position = 0;
          if (item.LastUsed.Year > 2000) {
            position = (item.LastUsed - oldestLastUsed.Value).TotalSeconds /
              (newestLastUsed.Value - oldestLastUsed.Value).TotalSeconds;
          }

          // Apply a coefficient from 0.8 to 1.2
          item.Score *= 0.8 + position * 0.4;
        }
      }

      // Compute the maximum score
      double maximumScore = 0;
      foreach (IItem item in items) {
        if (item.Score > maximumScore) {
          maximumScore = item.Score;
        }
      }

      // Normalization 0 - 100
      if (maximumScore > 0) {
        foreach (IItem item in items) {
          item.Score *= 100.0 / maximumScore;
        }
      }

      return items;
    }

    /// <summary>
    /// Get all items sharing the same id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Can be empty but not null</returns>
    public static IList<IItem> GetItems (string id)
    {
      var list = new List<IItem> ();

      foreach (IItem item in Instance.m_items) {
        if (string.Equals (item.ID, id)) {
          list.Add (item);
        }
      }

      return list;
    }

    /// <summary>
    /// Get a specific item
    /// If the subId is not filled, return the first item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="subId"></param>
    /// <returns></returns>
    public static IItem GetItem (string id, string subId)
    {
      foreach (IItem item in Instance.m_items) {
        if (string.Equals (item.ID, id) && (string.IsNullOrEmpty (subId) || string.Equals (item.SubID, subId))) {
          return item;
        }
      }

      return null;
    }

    /// <summary>
    /// Get all possible categories sorted by alphabetical order
    /// </summary>
    /// <returns>Categories</returns>
    public static IList<string> GetCategories ()
    {
      var categories = new List<string> ();

      foreach (IItem item in Instance.m_items) {
        string category = item.Category;
        if (category == "") {
          category = LemSettingsGlobal.DEFAULT_CATEGORY;
        }

        if (!categories.Contains (category)) {
          categories.Add (category);
        }
      }
      categories.Sort ();

      return categories;
    }

    /// <summary>
    /// Get items that have been created with the same dll and conf
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IList<IItem> GetBrothers (IItem item)
    {
      var items = new List<IItem> ();

      foreach (IItem itemInList in Instance.m_items) {
        if (itemInList.DllPath == item.DllPath && itemInList.IniPath == item.IniPath) {
          items.Add (itemInList);
        }
      }

      return items;
    }

    /// <summary>
    /// Get items that deal (read-only) with data used by another item
    /// </summary>
    /// <param name="item">item that deals with some data</param>
    /// <returns></returns>
    public static IList<IItem> GetRelatedReadOnlyItems (IItem item)
    {
      var items = new List<IItem> ();

      foreach (Type type in item.Types.Keys) {
        if (Instance.m_interactionsReadOnly.ContainsKey (type)) {
          foreach (IItem typeDisplay in Instance.m_interactionsReadOnly[type]) {
            if (!items.Contains (typeDisplay) && typeDisplay != item) {
              items.Add (typeDisplay);
            }
          }
        }
      }

      return items;
    }

    /// <summary>
    /// Get items that deal with data used by another item
    /// </summary>
    /// <param name="item">item that deals with some data</param>
    /// <returns></returns>
    public static IList<IItem> GetRelatedItems (IItem item)
    {
      var items = new List<IItem> ();

      foreach (Type type in item.Types.Keys) {
        if (Instance.m_interactions.ContainsKey (type)) {
          foreach (IItem typeWriter in Instance.m_interactions[type]) {
            if (!items.Contains (typeWriter) && typeWriter != item) {
              items.Add (typeWriter);
            }
          }
        }
      }

      return items;
    }

    /// <summary>
    /// Get items that write data, allowed by a page
    /// </summary>
    /// <param name="item">item comprising the page</param>
    /// <param name="page">page that allows editing some data externally</param>
    /// <returns></returns>
    public static IList<IItem> GetRelatedWriterItems (IItem item, IItemPage page)
    {
      var items = new List<IItem> ();

      IList<Type> types = null;
      if (page is IWizardPage) {
        types = (page as IWizardPage).EditableTypes;
      }

      if (page is IConfiguratorPage) {
        types = (page as IConfiguratorPage).EditableTypes;
      }

      if (types != null) {
        foreach (Type type in types) {
          if (Instance.m_interactions.ContainsKey (type)) {
            foreach (IItem typeWriter in Instance.m_interactions[type]) {
              if (!items.Contains (typeWriter) && typeWriter != item) {
                items.Add (typeWriter);
              }
            }
          }
        }
      }

      return items;
    }

    /// <summary>
    /// Remove an item
    /// </summary>
    /// <param name="item">item to be removed</param>
    /// <returns>return true if success</returns>
    public static bool RemoveItem (IItem item)
    {
      bool ok = true;
      var filesToBeDeleted = new List<string> ();

      if (item.IniPath != "") {
        // Delete inipath
        filesToBeDeleted.Add (item.IniPath);

        // Delete dllpath if no more items use it
        bool stillUsed = false;
        foreach (IItem itemInList in Instance.m_items) {
          if (itemInList.IniPath != item.IniPath &&
              itemInList.DllPath == item.DllPath) {
            stillUsed = true;
            break;
          }
        }

        if (!stillUsed) {
          filesToBeDeleted.Add (item.DllPath);
        }
      }
      else {
        // Delete dllpath
        filesToBeDeleted.Add (item.DllPath);
      }

      // Unload all items
      Instance.m_items.Clear ();
      Instance.m_interactionsReadOnly.Clear ();
      Instance.m_interactions.Clear ();

      // Delete files that have to be deleted
      try {
        foreach (string fileToBeDeleted in filesToBeDeleted) {
          File.Delete (fileToBeDeleted);
        }
      }
      catch (Exception e) {
        log.Error ("Cannot remove an item", e);
        ok = false;
      }

      if (ItemsChanged != null) {
        ItemsChanged ();
      }

      return ok;
    }

    void LoadItems ()
    {
      // Clear existing items
      m_items.Clear ();
      m_interactionsReadOnly.Clear ();
      m_interactions.Clear ();

      // Load all dlls associated with their config files
      // (install directory has the priority over the user directory)
      var dlls = new List<ItemDllLoader> ();
      if (m_itemInstallDir.Exists) {
        GetDlls (dlls, m_itemInstallDir, LemSettingsGlobal.SEARCH_DIRECTORY_DEPTH);
      }
      if (m_itemUserDir.Exists) {
        GetDlls (dlls, m_itemUserDir, LemSettingsGlobal.SEARCH_DIRECTORY_DEPTH);
      }
      if (m_itemAlternativeDirValid && m_itemAlternativeDir.Exists) {
        GetDlls (dlls, m_itemAlternativeDir, LemSettingsGlobal.SEARCH_DIRECTORY_DEPTH);
      }

      // Retrieve all items provided by the dlls
      foreach (ItemDllLoader dll in dlls) {
        foreach (IItem item in dll.Items) {
          m_items.Add (item);

          // Register the item as dealing with a type, readonly or not
          IDictionary<Type, LemSettingsGlobal.InteractionType> types = item.Types;

          if (types != null) {
            // Can this item be a exclusive reader?
            bool exclusiveReaderPossible = ((item.Flags & LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED) != 0);

            foreach (Type type in types.Keys) {
              if ((types[type] & LemSettingsGlobal.InteractionType.PRINCIPAL) != 0) {
                if (!m_interactions.ContainsKey (type)) {
                  m_interactions[type] = new List<IItem> ();
                }

                m_interactions[type].Add (item);

                // Readonly possible?
                if (exclusiveReaderPossible) {
                  if (!m_interactionsReadOnly.ContainsKey (type)) {
                    m_interactionsReadOnly[type] = new List<IItem> ();
                  }

                  m_interactionsReadOnly[type].Add (item);
                }
              }
            }
          }
        }
      }
    }

    void GetDlls (IList<ItemDllLoader> listDlls, DirectoryInfo dir, int depth)
    {
      // Find all paths for directories, dll and ini files
      IList<string> dllPaths = new List<string> ();
      IList<string> iniPaths = new List<string> ();
      IList<DirectoryInfo> dirPaths = new List<DirectoryInfo> ();
      foreach (FileInfo file in dir.GetFiles ()) {
        if (file.Extension == ".dll") {
          if (!file.Name.StartsWith ("fwlib", StringComparison.CurrentCultureIgnoreCase)) { // Exclude the Focas library
            dllPaths.Add (file.FullName);
          }
        }
        else if (file.Extension == ".ini") {
          iniPaths.Add (file.FullName);
        }
        else if (file.Extension == ".lnk") {
          try {
            // We follow the link
            string linkPath = GetLnkTarget (file.FullName);
            FileAttributes attr = File.GetAttributes (linkPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
              var dirInfo = new DirectoryInfo (linkPath);
              dirPaths.Add (dirInfo);
            }
            else {
              var fileInfo = new FileInfo (linkPath);
              if (fileInfo.Extension == ".dll") {
                dllPaths.Add (linkPath);
              }
              else if (fileInfo.Extension == ".ini") {
                iniPaths.Add (linkPath);
              }
            }
          }
          catch (Exception ex) {
            log.Warn ("Cannot follow the link \"" + file.FullName + "\"", ex);
          }
        }
      }
      foreach (DirectoryInfo subDir in dir.GetDirectories ()) {
        dirPaths.Add (subDir);
      }

      // Keep all valid dlls, ignore duplicates
      IList<ItemDllLoader> listDllsTmp = new List<ItemDllLoader> ();
      foreach (string path in dllPaths) {
        var itemDll = new ItemDllLoader (ContextManager.GetItemContext (), path);
        if (itemDll.IsValid && !listDlls.Contains (itemDll)) {
          listDllsTmp.Add (itemDll);
        }
      }

      // Add config files to the corresponding dll
      foreach (string path in iniPaths) {
        var itemConfig = new ItemConfig (path);
        if (itemConfig.IsValid) {
          foreach (ItemDllLoader dll in listDllsTmp) {
            if (string.Equals (itemConfig.ItemId, dll.ID)) {
              dll.AddConfig (itemConfig, path);
            }
          }
        }
      }

      // Merge the temporary list
      foreach (var dll in listDllsTmp) {
        listDlls.Add (dll);
      }

      if (depth > 0) {
        // Navigate through all directories
        foreach (DirectoryInfo subDir in dirPaths) {
          GetDlls (listDlls, subDir, depth - 1);
        }
      }
    }

    /// <summary>
    /// Cf http://stackoverflow.com/questions/8660705/how-to-follow-a-lnk-file-programmatically
    /// </summary>
    /// <param name="lnkPath">path of the link</param>
    /// <returns>target path</returns>
    static string GetLnkTarget (string lnkPath)
    {
      throw new NotImplementedException ();
    }
    #endregion // Methods

    #region Instance
    static ItemManager Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
      static Nested () { }
      internal static readonly ItemManager instance = new ItemManager ();
    }
    #endregion // Instance
  }
}
