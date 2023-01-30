// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of DisplayHelper.
  /// </summary>
  internal sealed class DisplayHelper
  {
    #region Members
    IDictionary<string, IDictionary<string, string>> m_tableDisplayPattern =
      new Dictionary<string, IDictionary<string, string>> (StringComparer.InvariantCultureIgnoreCase);
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (DisplayHelper).FullName);

    #region Getters / Setters
    /// <summary>
    /// Map Table -> Display pattern
    /// </summary>
    public static IDictionary<string, IDictionary<string, string>> TableDisplayPattern {
      get { return Instance.m_tableDisplayPattern; }
      set { Instance.m_tableDisplayPattern = value; }
    }
    #endregion
    
    #region Methods
    /// <summary>
    /// Get the pattern for the specified table and variant
    /// </summary>
    /// <param name="table">not null</param>
    /// <param name="variant"></param>
    /// <param name="pattern"></param>
    /// <returns>The exact table and variant were found</returns>
    public static bool TryGetPattern (string table, string variant, out string pattern)
    {
      Debug.Assert (!string.IsNullOrEmpty (table));
      
      IDictionary<string, string> tableDictionary;
      if (!TableDisplayPattern.TryGetValue (table, out tableDictionary)) {
        pattern = null;
        return false;
      }
      else {
        return tableDictionary.TryGetValue (variant ?? "", out pattern);
      }
    }
    #endregion // Methods

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private DisplayHelper()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineCategoryConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("DisplayHelper.Initialization",
                                                                               TransactionLevel.ReadCommitted))
      {
        IList<IDisplay> displays =
          daoFactory.DisplayDAO.
          FindAll ();
        foreach (IDisplay display in displays) {
          IDictionary<string, string> tableDictionary;
          if (!m_tableDisplayPattern.TryGetValue (display.Table, out tableDictionary)) {
            tableDictionary = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);
            m_tableDisplayPattern [display.Table] = tableDictionary;
          }
          string variant = display.Variant ?? "";
          tableDictionary [variant] = display.Pattern;
        }
      }
    }
    #endregion

    #region Instance
    static DisplayHelper Instance
    {
      get { return Nested.instance; }
    }
    
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static readonly DisplayHelper instance = new DisplayHelper ();
    }
    #endregion
  }
}
