// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lemoine.Core.Log;

namespace Lemoine.Settings
{
  /// <summary>
  /// Link to an item dll
  /// </summary>
  public class ItemDllLoader : IEquatable<object>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ItemDllLoader).FullName);

    #region Members
    IItemDll m_itemDll = null;
    readonly List<IItem> m_items = new List<IItem> ();
    readonly string m_dllPath = "";
    readonly ItemContext m_context;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Return true if the dll is valid
    /// If false, this dll should be ignored
    /// </summary>
    public bool IsValid
    {
      get { return m_itemDll != null; }
    }

    /// <summary>
    /// Return the id of the items configured by the dll
    /// </summary>
    public string ID
    {
      get
      {
        if (IsValid) {
          return m_itemDll.GetType ().Namespace;
        }
        else {
          log.ErrorFormat ("ID: item is not valid => return an empty string");
          return "";
        }
      }
    }

    /// <summary>
    /// Return the list of items created by this dll
    /// </summary>
    public IList<IItem> Items { get { return m_items; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="context">Context in which items are created</param>
    /// <param name="dllPath">Path of the dll to load</param>
    public ItemDllLoader (ItemContext context, string dllPath)
    {
      m_context = context;
      m_dllPath = dllPath;
      Load ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a configuration to the dll
    /// </summary>
    /// <param name="itemConfig">configuration to add</param>
    /// <param name="iniPath">path to the config file</param>
    public void AddConfig (ItemConfig itemConfig, string iniPath)
    {
      if (IsValid && m_context != null) {
        try {
          IList<IItem> items = m_itemDll.CreateItems (m_context, itemConfig);
          if (items != null) {
            foreach (IItem item in items) {
              if (IsItemDisplayed (item)) {
                InitItem (item);
                item.IniPath = iniPath;
                m_items.Add (item);
              }
            }
          }
        }
        catch (Exception e) {
          log.Error ("AddConfig: " +
                     "Unable to create items with configuration " + iniPath,
                     e);
        }
      }
    }

    void Load ()
    {
      try {
        var assemblyLoader = new Core.Plugin.TargetSpecific.AssemblyLoader ();
        var assembly = assemblyLoader.LoadFromPath (m_dllPath);
        if (null == assembly) {
          if (m_context != null && m_context.Options.LoadLibraryFiles) {
            // The dlls are locked when the software is open
            // We have access to the debugger inside the libraries
            // but we cannot update the dlls within the software
            assembly = Assembly.LoadFile (m_dllPath);
          }
          else {
            // The dlls are not locked when the software is open
            // We don't have acces to the debugger inside the libaries
            // but we can update the dlls within the software
            assembly = Assembly.Load (File.ReadAllBytes (m_dllPath));
          }
        }

        // Create an instance of itemDll if possible
        Type[] exportedTypes = assembly.GetExportedTypes ();
        Type itemType = null;
        foreach (Type exportedType in exportedTypes) {
          if (typeof (IItemDll).IsAssignableFrom (exportedType)) {
            itemType = exportedType;
          }
        }

        if (itemType != null) {
          m_itemDll = (IItemDll)Activator.CreateInstance (itemType);

          // Create items unrelated to configuration
          if (m_context != null) {
            IList<IItem> items = m_itemDll.CreateItems (m_context);
            if (items != null) {
              foreach (IItem item in items) {
                InitItem (item);
                if (IsItemDisplayed (item)) {
                  m_items.Add (item);
                }
              }
            }
          }
        }
      }
      catch (Exception ex) {
        log.Warn ("Unable to load dll " + m_dllPath, ex);
        m_itemDll = null;
        m_items.Clear ();
      }
    }

    void InitItem (IItem item)
    {
      item.Context = m_context;
      item.DllPath = m_dllPath;
      item.Initialize ();
    }

    bool IsItemDisplayed (IItem item)
    {
      // Flag ONLY_ADMIN_AND_SUPER_ADMIN
      if ((item.Flags & LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN) != 0 &&
          m_context.UserCategory != LemSettingsGlobal.UserCategory.ADMINISTRATOR &&
          m_context.UserCategory != LemSettingsGlobal.UserCategory.SUPER_ADMIN) {
        log.InfoFormat ("IsItemDisplayed: " +
                        "{0} not displayed because the current user is not an admin",
                        item.Title);
        return false;
      }

      // Flag ONLY_SUPER_ADMIN
      if ((item.Flags & LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN) != 0 &&
          m_context.UserCategory != LemSettingsGlobal.UserCategory.SUPER_ADMIN) {
        log.InfoFormat ("IsItemDisplayed: " +
                        "{0} not displayed because the current user is not a super admin",
                        item.Title);
        return false;
      }

      // Flag ONLY_LCTR
      if ((item.Flags & LemSettingsGlobal.ItemFlag.ONLY_LCTR) != 0 && !m_context.IsLctr) {
        log.WarnFormat ("IsItemDisplayed: " +
                        "{0} not displayed because the current computer is not lctr",
                        item.Title);
        return false;
      }

      // Flag ONLY_LPOST
      if ((item.Flags & LemSettingsGlobal.ItemFlag.ONLY_LPOST) != 0 && !m_context.IsLpost) {
        log.InfoFormat ("IsItemDisplayed: " +
                        "{0} not displayed because the current computer is not an lpost",
                        item.Title);
        return false;
      }

      return true;
    }
    #endregion // Methods

    #region Equatable implementation
    /// <summary>
    /// Return true of 2 ItemDllLoader are the same
    /// (related to the same kind of items)
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      var other = obj as ItemDllLoader;
      if (other == null) {
        return false;
      }

      return string.Equals (this.ID, other.ID);
    }

    /// <summary>
    /// HashCode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * this.ID.GetHashCode ();
      }
      return hashCode;
    }

    /// <summary>
    /// ==
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator == (ItemDllLoader lhs, ItemDllLoader rhs)
    {
      if (ReferenceEquals (lhs, rhs)) {
        return true;
      }

      if (ReferenceEquals (lhs, null) || ReferenceEquals (rhs, null)) {
        return false;
      }

      return lhs.Equals (rhs);
    }

    /// <summary>
    /// !=
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator != (ItemDllLoader lhs, ItemDllLoader rhs)
    {
      return !(lhs == rhs);
    }
    #endregion // Equatable implementation
  }
}
