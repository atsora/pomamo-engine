// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;

namespace Lemoine.Settings
{
  /// <summary>
  /// Variables / enums shared globally
  /// </summary>
  public static class LemSettingsGlobal
  {
    static readonly string SYNC_SETTINGS_DIRECTORY_DEFAULT = "settings_synchronized";
    static readonly string SYNC_SETTINGS_DIRECTORY_KEY = "Settings.SyncItemsDirectory";

    static readonly string WIKI_ADDRESS_KEY = "Settings.WikiAddress";
    static readonly string WIKI_ADDRESS_DEFAULT = "/pmwiki.php?n=Use.LemSettings";

    /// <summary>
    /// Name of the directory that contains the LemSettings items, in pfrdata or user directory
    /// </summary>
    public static string GetSyncSettingsDirectory ()
    {
      return Lemoine.Info.ConfigSet
        .LoadAndGet<string> (SYNC_SETTINGS_DIRECTORY_KEY, SYNC_SETTINGS_DIRECTORY_DEFAULT);
    }

    /// <summary>
    /// Title of this software
    /// </summary>
    public static readonly string SOFTWARE_TITLE = "Settings";
    
    /// <summary>
    /// Default category when it's not defined
    /// </summary>
    public static readonly string DEFAULT_CATEGORY = "Other";
    
    /// <summary>
    /// Default subcategory when it's not defined
    /// </summary>
    public static readonly string DEFAULT_SUBCATEGORY = "Other";
    
    /// <summary>
    /// Depth of the search when dll and ini files are listed
    /// </summary>
    public static readonly int SEARCH_DIRECTORY_DEPTH = 5;

    /// <summary>
    /// Web address of the wiki for LemSettings
    /// </summary>
    public static readonly string WIKI_ADDRESS = Lemoine.Info.ConfigSet
      .LoadAndGet (WIKI_ADDRESS_KEY, WIKI_ADDRESS_DEFAULT);
    
    #region Colors
    /// <summary>
    /// Color of the category headers
    /// </summary>
    public static readonly Color COLOR_CATEGORY = Color.FromArgb(255, 0, 81, 137);
    
    /// <summary>
    /// Color of the subcategory headers
    /// </summary>
    public static readonly Color COLOR_SUBCATEGORY = Color.FromArgb(255, 102, 148, 206);
    
    /// <summary>
    /// Color when an item is hovered
    /// </summary>
    public static readonly Color COLOR_ITEM_HOVER = Color.LightGreen;
    
    /// <summary>
    /// Color used in list, when every second row has a second color
    /// </summary>
    public static readonly Color COLOR_ALTERNATE_ROW = Color.Cyan;
    
    /// <summary>
    /// Color of an error
    /// </summary>
    public static readonly Color COLOR_ERROR = Color.OrangeRed;
    
    /// <summary>
    /// Color of a warning
    /// </summary>
    public static readonly Color COLOR_WARNING = Color.Goldenrod;
    
    /// <summary>
    /// Color when everything is ok, no errors or warnings
    /// </summary>
    public static readonly Color COLOR_OK = Color.LimeGreen;
    #endregion // Colors
    
    #region Enums
    /// <summary>
    /// Enum defining the interaction between an item with a particular type
    /// </summary>
    [Flags]
    public enum InteractionType
    {
      /// <summary>
      /// The item displays or write the type as a principal subject or in a notable way
      /// </summary>
      PRINCIPAL = 1,
      
      /// <summary>
      /// The item read the type, may display it but not as a principal subject
      /// A secondary type cannot be edited, use principal instead
      /// </summary>
      SECONDARY = 2
    }
    
    /// <summary>
    /// Describes the category of the user
    /// </summary>
    public enum UserCategory
    {
      /// <summary>
      /// A super administrator can do everything (and even more)
      /// </summary>
      SUPER_ADMIN = 1,
      
      /// <summary>
      /// The administrator installs and maintains the system at the client
      /// </summary>
      ADMINISTRATOR = 2,
      
      /// <summary>
      /// The end user represents the client
      /// Consider he has no skill and makes stupid decisions
      /// </summary>
      END_USER = 3
    }
    
    /// <summary>
    /// Series of properties that characterizes a page
    /// Some properties can be used in combination
    /// </summary>
    [Flags]
    public enum PageFlag
    {
      /// <summary>
      /// Nothing special
      /// </summary>
      NONE = 0,
      
      /// <summary>
      /// Display a red header focusing the attention of the user
      /// </summary>
      DANGEROUS_ACTIONS = 1,
      
      /// <summary>
      /// Don't log after a validation (when nothing happens in the database)
      /// For configurators only
      /// </summary>
      DONT_LOG_VALIDATION = 2,
      
      /// <summary>
      /// Don't show a popup saying that the operation has been successfully applied
      /// For configurators only
      /// </summary>
      DONT_SHOW_SUCCESS_INFORMATION = 4,
      
      /// <summary>
      /// For configurators and views, enable the step of validation
      /// This provides two different behaviours:
      /// - either each input of the user is directly processed (validation not needed)
      /// - or each input is taken into account during the validation step
      /// Note: the first viewpage has no validation step, whatever the result here
      /// </summary>
      WITH_VALIDATION = 8,
      
      /// <summary>
      /// SuperAdmin can ignore errors except if this flag is on
      /// </summary>
      IGNORE_IMPOSSIBLE = 16,
      
      /// <summary>
      /// Display a red header for admin rights
      /// </summary>
      ADMIN_RIGHT_REQUIRED = 32
    }
    
    /// <summary>
    /// Series of properties that characterizes an item
    /// Some properties can be used in combination
    /// </summary>
    [Flags]
    public enum ItemFlag
    {
      /// <summary>
      /// Nothing special
      /// </summary>
      NONE = 0,
      
      /// <summary>
      /// The item won't be displayed (only accessible via other items)
      /// </summary>
      HIDDEN = 1,
      
      /// <summary>
      /// For configurators, which can also be opened as a view
      /// Automatically set for views, removed for wizard
      /// </summary>
      VIEW_MODE_ALLOWED = 2,
      
      /// <summary>
      /// For launchers, the process is open in a modal window. LemSettings waits until it ends.
      /// </summary>
      PROCESS_MODAL = 4,
      
      /// <summary>
      /// The item is marked as a favorite
      /// </summary>
      FAVORITE = 8,
      
      /// <summary>
      /// The item appears only for the Admin and SuperAdmin
      /// </summary>
      ONLY_ADMIN_AND_SUPER_ADMIN = 16,
      
      /// <summary>
      /// The item appears only for SuperAdmin
      /// </summary>
      ONLY_SUPER_ADMIN = 32,
      
      /// <summary>
      /// The item appears only on the LCTR computer
      /// </summary>
      ONLY_LCTR = 64,
      
      /// <summary>
      /// The item appears only on a LPOST computer
      /// </summary>
      ONLY_LPOST = 128
    }
    #endregion // Enums
  }
}
