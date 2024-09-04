// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Info;

namespace Lemoine.Settings
{
  /// <summary>
  /// Configuration read by an itemDll to create items
  /// </summary>
  public class ItemConfig
  {
    #region Members
    readonly IniFile m_iniFile;
    readonly IDictionary<string, string> m_values = new Dictionary<string, string>();
    #endregion // Members
    
    static readonly string IDENTIFICATION_SECTION_NAME = "Identification";
    static readonly string PARAMETERS_SECTION_NAME = "Parameters";
    static readonly string ITEM_ID_KEY_NAME = "item_id";
    static readonly string CONF_ID_KEY_NAME = "configuration_id";

    #region Getters / Setters
    /// <summary>
    /// Return true if the config is valid
    /// If false, this config must be ignored
    /// </summary>
    public bool IsValid { get; private set; }
    
    /// <summary>
    /// Id of the item configured by this config
    /// </summary>
    public string ItemId { get; private set; }
    
    /// <summary>
    /// Id of this config
    /// </summary>
    public int ConfId { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="path"></param>
    public ItemConfig(string path)
    {
      m_iniFile = new IniFile(path, true);
      CheckIfValid();
      if (IsValid) {
        LoadConfig ();
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Return true if key is defined
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsSet(string key)
    {
      return m_values.ContainsKey(key);
    }
    
    /// <summary>
    /// Get the value in key
    /// </summary>
    /// <param name="key"></param>
    /// <returns>a string, may be empty but never null</returns>
    public string GetValue(string key)
    {
      return IsSet(key) ? m_values[key] : "";
    }
    
    /// <summary>
    /// Get all keys within a section
    /// </summary>
    /// <returns>a list, may be empty but never null</returns>
    public ICollection<string> GetKeys()
    {
      return m_values.Keys;
    }
    
    void CheckIfValid()
    {
      IsValid = false;
      
      ICollection<string> sections = m_iniFile.GetSections();
      if (sections != null && sections.Contains(IDENTIFICATION_SECTION_NAME) &&
          sections.Contains(PARAMETERS_SECTION_NAME)) {
        ICollection<string> keys = m_iniFile.GetKeys(IDENTIFICATION_SECTION_NAME);
        if (keys != null && keys.Contains(ITEM_ID_KEY_NAME) && keys.Contains(CONF_ID_KEY_NAME)) {
          string strItemId = m_iniFile.GetValue(IDENTIFICATION_SECTION_NAME, ITEM_ID_KEY_NAME);
          string strConfId = m_iniFile.GetValue(IDENTIFICATION_SECTION_NAME, CONF_ID_KEY_NAME);
          try {
            ItemId = strItemId;
            ConfId = int.Parse(strConfId);
            IsValid |= !string.IsNullOrEmpty(ItemId) && ConfId > 0;
          } catch {
            // Nothing
          }
        }
      }
    }
    
    void LoadConfig()
    {
      m_values.Clear();
      ICollection<string> keys = m_iniFile.GetKeys(PARAMETERS_SECTION_NAME);
      foreach (string key in keys) {
        m_values[key] = m_iniFile.GetValue(PARAMETERS_SECTION_NAME, key);
      }
    }
    #endregion // Methods
  }
}
