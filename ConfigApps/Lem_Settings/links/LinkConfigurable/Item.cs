// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Resources;
using Lemoine.Settings;

namespace LinkConfigurable
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, ILink
  {
    #region Members
    readonly string m_title;
    readonly string m_description;
    readonly string m_category;
    readonly string m_subcategory;
    readonly string m_url;
    readonly ICollection<string> m_keywords;
    readonly string m_iconPath;
    readonly LemSettingsGlobal.ItemFlag m_flags = 0;
    readonly IDictionary<Type, LemSettingsGlobal.InteractionType> m_types =
      new Dictionary<Type, LemSettingsGlobal.InteractionType>();
    #endregion
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return m_title; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description { get { return m_description; } }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords { get { return m_keywords; } }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "internet"; } }
    
    /// <summary>
    /// Image displayed as an icon
    /// </summary>
    new public Image Image { // Override existing Image property
      get {
        Image image = null;
        try {
          string path = m_iconPath;
          if (!File.Exists(path)) {
            path = Path.Combine(Path.GetDirectoryName(IniPath), m_iconPath);
          }

          if (File.Exists(path)) {
            image = Image.FromFile(path);
          }
        } catch {}
        
        if (image == null) {
          // Default image
          var rm = new ResourceManager(GetType().ToString(), GetType().Assembly);
          image = (Image)rm.GetObject(IconName) ?? new Bitmap(10, 10);
        }
        
        return image;
      }
    }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return m_category; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return m_subcategory; } }
    
    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags { get { return m_flags; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types { get { return m_types; } }
    
    /// <summary>
    /// Url to open
    /// </summary>
    public String UrlLink { get { return m_url; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public Item(ItemConfig config) : base()
    {
      SubID = this.GetType().Name + config.ConfId;
      
      // Load configuration
      m_title = config.GetValue("name");
      m_description = config.GetValue("description");
      m_category = config.GetValue("category");
      m_subcategory = config.GetValue("subcategory");
      m_url = config.GetValue("url");
      m_iconPath = config.GetValue("iconpath");
      
      // Flags
      switch (config.GetValue("accessibility")) {
        case "1":
          m_flags |= LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;
          break;
        case "2":
          m_flags |= LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;
          break;
      }
      switch (config.GetValue("enabled").ToLower()) {
        case "false": case "0": case "no":
          m_flags |= LemSettingsGlobal.ItemFlag.HIDDEN;
          break;
      }
      m_flags |= LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED;
      
      // Keywords
      m_keywords = new List<string>(config.GetValue("keywords").Split(
        new char [] { ',', ' ', ';'}, StringSplitOptions.RemoveEmptyEntries));
      m_keywords.Add("url");
      m_keywords.Add("internet");
      
      // Types
      var strTypes = config.GetValue("types").Split(
        new char [] {' ', ',', ';'}, StringSplitOptions.RemoveEmptyEntries);
      foreach (var strType in strTypes) {
        var split = strType.Split(':');
        if (split.Length == 2) {
          var type = Type.GetType(split[1].Trim(' ') + ", " + split[0].Trim(' '), false);
          if (type != null) {
            m_types[type] = LemSettingsGlobal.InteractionType.PRINCIPAL;
          }
        }
      }
    }
    #endregion // Constructors
  }
}
