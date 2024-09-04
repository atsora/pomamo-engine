// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Reflection;
using Lemoine.Info;
using Lemoine.Core.Log;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of IniFilePreferences.
  /// </summary>
  public sealed class IniFilePreferences
  {
    #region Members
    IniFile m_iniFile = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof(IniFilePreferences).FullName);
    
    /// <summary>
    /// The different field which are present in the configuration ini file
    /// </summary>
    public enum Field
    {
      /// <summary>
      /// Performance unit
      /// Possibilities: parts/hour, parts/minute, seconds/part or minutes/part
      /// </summary>
      PERFORMANCE_UNIT,
      
      /// <summary>
      /// Visibility of the right panel in the interface
      /// Possibilities: yes or no
      /// </summary>
      RIGHT_PANEL_VISIBILITY,
      
      /// <summary>
      /// Visibility of the left panel in the interface
      /// Possibilities: yes or no
      /// </summary>
      LEFT_PANEL_VISIBILITY,
      
      /// <summary>
      /// Last login if it has been remembered (encrypted)
      /// </summary>
      LOGIN,
      
      /// <summary>
      /// Last password if it has been remembered (encrypted)
      /// </summary>
      PASSWORD,
      
      /// <summary>
      /// When browsing the modifications, this number is the last that has been found as processed
      /// Browsing again modifications before this id is thus useless (save time)
      /// </summary>
      LAST_MODIFICATION_ID_OK,
      
      /// <summary>
      /// State of MainWindow
      /// </summary>
      WINDOW_STATE,
      
      /// <summary>
      /// Position of MainWindow
      /// </summary>
      WINDOW_POSITION
    }
    
    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    IniFilePreferences()
    {
      try {
        // Ini file in the directory
        var name = Assembly.GetExecutingAssembly ().FullName;
        if (name != "") {
          name = name.Split (',')[0];
        }

        m_iniFile = new IniFile(name, false);
      } catch (Exception e) {
        log.Error("Cannot create IniFilePreferences", e);
        throw;
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Retrieve a value
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public static string Get(Field field)
    {
      bool encrypted = (field == Field.LOGIN || field == Field.PASSWORD);
      
      // Value specified in the Ini file
      string value = encrypted ?
        Instance.m_iniFile.GetAndDecryptValue(GetSection(field), GetKey(field), "test") :
        Instance.m_iniFile.GetValue(GetSection(field), GetKey(field));
      
      // Default value
      if (value == "") {
        value = GetDefaultValue(field);
      }

      return value;
    }
    
    /// <summary>
    /// Store a value
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public static void Set(Field field, string value)
    {
      bool encrypted = (field == Field.LOGIN || field == Field.PASSWORD);
      
      if (encrypted) {
        Instance.m_iniFile.EncryptAndSetValue(GetSection(field), GetKey(field), "test", value);
      }
      else {
        Instance.m_iniFile.SetValue(GetSection(field), GetKey(field), value);
      }
    }
    
    /// <summary>
    /// Retrieve a value for an item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Get(GenericItem item, string key)
    {
      return Instance.m_iniFile.GetValue("Item_" + item.ID + "." + item.SubID, key);
    }
    
    /// <summary>
    /// Retrieve a value for an item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Get(IItem item, string key)
    {
      return Instance.m_iniFile.GetValue("Item_" + item.ID + "." + item.SubID, key);
    }
    
    /// <summary>
    /// Store a value for an item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set(GenericItem item, string key, string value)
    {
      Instance.m_iniFile.SetValue("Item_" + item.ID + "." + item.SubID, key, value);
    }
    
    /// <summary>
    /// Store a value for an item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set(IItem item, string key, string value)
    {
      Instance.m_iniFile.SetValue("Item_" + item.ID + "." + item.SubID, key, value);
    }
    
    /// <summary>
    /// Retrieve a value for an item and a specific user
    /// </summary>
    /// <param name="item"></param>
    /// <param name="userLogin"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Get(GenericItem item, string userLogin, string key)
    {
      return Instance.m_iniFile.GetValue("Item_" + item.ID + "." + item.SubID, key + "_" + userLogin);
    }
    
    /// <summary>
    /// Retrieve a value for an item and a specific user
    /// </summary>
    /// <param name="item"></param>
    /// <param name="userLogin"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Get(IItem item, string userLogin, string key)
    {
      return Instance.m_iniFile.GetValue("Item_" + item.ID + "." + item.SubID, key + "_" + userLogin);
    }
    
    /// <summary>
    /// Store a value for an item and a specific user
    /// </summary>
    /// <param name="item"></param>
    /// <param name="userLogin"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set(GenericItem item, string userLogin, string key, string value)
    {
      Instance.m_iniFile.SetValue("Item_" + item.ID + "." + item.SubID, key + "_" + userLogin, value);
    }
    
    /// <summary>
    /// Store a value for an item and a specific user
    /// </summary>
    /// <param name="item"></param>
    /// <param name="userLogin"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set(IItem item, string userLogin, string key, string value)
    {
      Instance.m_iniFile.SetValue("Item_" + item.ID + "." + item.SubID, key + "_" + userLogin, value);
    }
    
    static string GetSection(Field field)
    {
      string section = "";
      switch (field) {
        case Field.PERFORMANCE_UNIT:
          section = "General";
          break;
        case Field.RIGHT_PANEL_VISIBILITY:
          section = "Interface";
          break;
        case Field.LEFT_PANEL_VISIBILITY:
          section = "Interface";
          break;
        case Field.LOGIN:
          section = "General";
          break;
        case Field.PASSWORD:
          section = "General";
          break;
        case Field.LAST_MODIFICATION_ID_OK:
          section = "General";
          break;
        case Field.WINDOW_STATE:
          section = "Interface";
          break;
        case Field.WINDOW_POSITION:
          section = "Interface";
          break;
      }
      return section;
    }
    
    static string GetKey(Field field)
    {
      string key = "";
      switch (field) {
        case Field.PERFORMANCE_UNIT:
          key = "performance_unit";
          break;
        case Field.RIGHT_PANEL_VISIBILITY:
          key = "right_panel_visible";
          break;
        case Field.LEFT_PANEL_VISIBILITY:
          key = "left_panel_visible";
          break;
        case Field.LOGIN:
          key = "last_login";
          break;
        case Field.PASSWORD:
          key = "last_password";
          break;
        case Field.LAST_MODIFICATION_ID_OK:
          key = "last_modification_id_ok";
          break;
        case Field.WINDOW_STATE:
          key = "window_state";
          break;
        case Field.WINDOW_POSITION:
          key = "window_position";
          break;
      }
      return key;
    }
    
    static string GetDefaultValue(Field field)
    {
      string defaultValue = "";
      switch (field) {
        case Field.PERFORMANCE_UNIT:
          defaultValue = "parts/hour";
          break;
        case Field.RIGHT_PANEL_VISIBILITY:
          defaultValue = "yes";
          break;
        case Field.LEFT_PANEL_VISIBILITY:
          defaultValue = "yes";
          break;
        case Field.LOGIN:
          defaultValue = "";
          break;
        case Field.PASSWORD:
          defaultValue = "";
          break;
        case Field.LAST_MODIFICATION_ID_OK:
          defaultValue = "0";
          break;
        case Field.WINDOW_STATE:
          defaultValue = "";
          break;
        case Field.WINDOW_POSITION:
          defaultValue = "";
          break;
      }
      return defaultValue;
    }
    #endregion // Methods
    
    #region Instance
    static IniFilePreferences Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested() {}
      internal static readonly IniFilePreferences instance = new IniFilePreferences();
    }
    #endregion // Instance
  }
}
