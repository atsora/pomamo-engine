// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of the context, based in which the items will be created or not
  /// Items may also be configured accordingly
  /// </summary>
  public class ItemContext
  {
    #region Getters / Setters
    /// <summary>
    /// User login
    /// </summary>
    public string UserLogin { get; set; }
    
    /// <summary>
    /// Category of the user being logged
    /// </summary>
    public LemSettingsGlobal.UserCategory UserCategory { get; set; }
    
    /// <summary>
    /// Arguments passed to the software
    /// </summary>
    public Options Options { get; set; }
    
    /// <summary>
    /// Return true if the item is shown in a view
    /// Usefull for some configurators which can also be a view
    /// </summary>
    public bool ViewMode { get; set; }
    
    /// <summary>
    /// Return true if the current computer is an LCTR
    /// </summary>
    public bool IsLctr { get; set; }
    
    /// <summary>
    /// Return true if the current computer is an LPOST
    /// </summary>
    public bool IsLpost { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ItemContext()
    {
      Options = null;
      UserLogin = "";
      UserCategory = LemSettingsGlobal.UserCategory.END_USER;
      IsLctr = false;
      IsLpost = false;
    }
    #endregion // Constructors
  }
}
