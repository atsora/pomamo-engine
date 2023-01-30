// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;

namespace LauncherTemplate
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, ILauncher
  {
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Launcher"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "This item serves as a template for all launchers.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "templates" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "launcher"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Templates"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return ""; } }
    
    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;
      }
    }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(Int32)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        return dic;
      }
    }
    
    /// <summary>
    /// Path of the software to open
    /// </summary>
    public String SoftwarePath { get { return "notepad.exe"; } }
    
    /// <summary>
    /// Argument to send, may be null
    /// </summary>
    public IList<String> Arguments {
      get {
        var args = new List<string>();
        args.Add("");
        return args;
      }
    }
    #endregion // Getters / Setters
  }
}
