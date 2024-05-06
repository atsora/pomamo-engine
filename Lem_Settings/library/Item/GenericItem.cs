// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Resources;
using Lemoine.Core.Log;

namespace Lemoine.Settings
{
  /// <summary>
  /// Base class for all items
  /// </summary>
  public abstract class GenericItem : IComparable
  {
    ILog log = LogManager.GetLogger<GenericItem> ();

    [Flags]
    enum KeywordLocation
    {
      IN_KEYWORDS = 1,
      IN_TITLE = 2,
      IN_SUBCATEGORY = 4,
      IN_CATEGORY = 8,
      IN_DESCRIPTION = 16
    }

    #region Members
    readonly IDictionary<string, KeywordLocation> m_keywords = new Dictionary<string, KeywordLocation> ();
    readonly IDictionary<string, int> m_keywordsWeight = new Dictionary<string, int> ();
    DateTime m_accessedDate;
    LemSettingsGlobal.ItemFlag m_favoriteFlag = LemSettingsGlobal.ItemFlag.NONE;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// ID related to the dll which created the item
    /// Set by the dll loader
    /// </summary>
    public string ID { get; protected set; }

    /// <summary>
    /// Sub ID (a dll possibly creates several items, this sub id identifies them)
    /// </summary>
    public string SubID { get; protected set; }

    /// <summary>
    /// Version related to the dll which created the item
    /// Set by the dll loader
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public abstract ICollection<string> Keywords { get; }

    /// <summary>
    /// Full list of keywords associated with their score, based on the title,
    /// the description, the category, the subcategory and the additional keywords
    /// Set by GenericItem
    /// </summary>
    public IDictionary<string, int> FullKeywords { get { return m_keywordsWeight; } }

    /// <summary>
    /// Image displayed as an icon
    /// </summary>
    public Image Image
    {
      get {
        try {
          var rm = new ResourceManager (GetType ().ToString (), GetType ().Assembly);
          return (Image)rm.GetObject (IconName) ?? new Bitmap (10, 10);
        }
        catch (Exception ex) {
          log.Error ($"Image: unknown picture {IconName}", ex);
          return new Bitmap (10, 10);
        }
      }
    }

    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected abstract string IconName { get; }

    /// <summary>
    /// Default category
    /// </summary>
    public abstract string Category { get; }

    /// <summary>
    /// Default subcategory
    /// </summary>
    public abstract string Subcategory { get; }

    /// <summary>
    /// Flags that characterize the item (extended)
    /// </summary>
    public LemSettingsGlobal.ItemFlag Flags
    {
      get {
        var flags = ItemFlags | m_favoriteFlag;
        if (this is IView) {
          flags |= LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED;
        }
        else if (this is IWizard) {
          flags &= ~LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED;
        }

        return flags;
      }
      set {
        m_favoriteFlag = value & LemSettingsGlobal.ItemFlag.FAVORITE;
        IniFilePreferences.Set (this, ContextManager.UserLogin, "favorite", m_favoriteFlag == 0 ? "0" : "1");
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public virtual LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.NONE;

    /// <summary>
    /// Context of the application
    /// Set by the dll loader
    /// </summary>
    public ItemContext Context { get; set; }

    /// <summary>
    /// Path of the dll having generated the item
    /// Set by the dll loader
    /// </summary>
    public string DllPath { get; set; }

    /// <summary>
    /// Path of the ini file having configured the item
    /// Set by the dll loader
    /// </summary>
    public string IniPath { get; set; }

    /// <summary>
    /// Score (in % based on an empirical formula)
    /// on how much the item matches with a series of keyword
    /// Set by ItemManager when we search for items
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Last use of the item
    /// Initialized by GenericItem, set by MainForm
    /// </summary>
    public DateTime LastUsed
    {
      get { return m_accessedDate; }
      set {
        m_accessedDate = value;
        IniFilePreferences.Set (this, ContextManager.UserLogin, "accessed_date", m_accessedDate.ToString ());
      }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected GenericItem ()
    {
      ID = this.GetType ().Namespace;
      SubID = this.GetType ().Name;
    }

    #region Methods
    /// <summary>
    /// Compare two items
    /// (used to sort them alphabetically)
    /// </summary>
    /// <param name="otherItem"></param>
    /// <returns></returns>
    public int CompareTo (object otherItem)
    {
      // A null value means that this object is greater
      if (otherItem as GenericItem == null) {
        return 1;
      }
      else {
        return this.Title.CompareTo ((otherItem as GenericItem).Title);
      }
    }

    /// <summary>
    /// Store a value in the .ini file, under a section specific to the item
    /// </summary>
    /// <param name="key"></param>
    /// <param name="valueToStore"></param>
    protected void StoreInConfig (string key, string valueToStore)
    {
      IniFilePreferences.Set (this, key, valueToStore);
    }

    /// <summary>
    /// Recall a value from the .ini file, under a section specific to the item
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected string GetFromConfig (string key)
    {
      return IniFilePreferences.Get (this, key);
    }

    /// <summary>
    /// Preparation of the item
    /// </summary>
    public void Initialize ()
    {
      GatherItemKeywords ();
      InitLastAccess ();
      InitUserFlags ();
    }

    void InitUserFlags ()
    {
      string str = IniFilePreferences.Get (this, ContextManager.UserLogin, "favorite");
      if (String.IsNullOrEmpty (str)) {
        m_favoriteFlag = LemSettingsGlobal.ItemFlag.NONE;
      }
      else if (str == "1") {
        m_favoriteFlag = LemSettingsGlobal.ItemFlag.FAVORITE;
      }
    }

    void InitLastAccess ()
    {
      // Last access
      string dateStr = IniFilePreferences.Get (this, ContextManager.UserLogin, "accessed_date");
      DateTime dateTime;
      if (!String.IsNullOrEmpty (dateStr) && DateTime.TryParse (dateStr, out dateTime)) {
        m_accessedDate = dateTime;
      }
      else {
        m_accessedDate = new DateTime (1970, 1, 1);
      }
    }

    void GatherItemKeywords ()
    {
      // Title, description and additional keywords
      AddItemKeywords (KeywordSearch.SplitIntoWords (Title), KeywordLocation.IN_TITLE);
      AddItemKeywords (KeywordSearch.SplitIntoWords (Description), KeywordLocation.IN_DESCRIPTION);
      AddItemKeywords (Keywords, KeywordLocation.IN_KEYWORDS);

      // Category and subcategory
      AddItemKeywords (KeywordSearch.SplitIntoWords (Category), KeywordLocation.IN_CATEGORY);
      AddItemKeywords (KeywordSearch.SplitIntoWords (Subcategory), KeywordLocation.IN_SUBCATEGORY);

      // Compute the weight for each keyword
      foreach (var element in m_keywords) {
        m_keywordsWeight[element.Key] = ComputeWeight (element.Value);
      }
    }

    void AddItemKeywords (ICollection<string> keywords, KeywordLocation location)
    {
      keywords = KeywordSearch.FormatKeywords (keywords);
      foreach (string keyword in keywords) {
        if (m_keywords.ContainsKey (keyword)) {
          m_keywords[keyword] |= location;
        }
        else {
          m_keywords[keyword] = location;
        }
      }
    }

    int ComputeWeight (KeywordLocation location)
    {
      return (location & KeywordLocation.IN_KEYWORDS) != 0 ? 5 : 0 +
        (location & KeywordLocation.IN_TITLE) != 0 ? 4 : 0 +
        (location & KeywordLocation.IN_SUBCATEGORY) != 0 ? 3 : 0 +
        (location & KeywordLocation.IN_CATEGORY) != 0 ? 2 : 0 +
        (location & KeywordLocation.IN_DESCRIPTION) != 0 ? 1 : 0;
    }

    /// <summary>
    /// Get a list of items for proposing the next possible steps to the user
    /// once the current wizard is successfully finished
    /// The value of the dictionary to return is an infinive text such as "monitor the machine"
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <returns>See the description above, can be null</returns>
    public virtual IDictionary<string, string> GetPossibleNextItems (ItemData data) { return null; }
    #endregion // Methods
  }
}
