// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;


namespace Lemoine.JobControls
{
  /// <summary>
  /// Deprecated: use now directly Lemoine.Info.ConfigSet
  /// </summary>
  internal class OperationExplorerConfigHelper
  {
    #region Members
    static readonly bool DEFAULT_PART_AT_THE_TOP = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationExplorerConfigHelper).FullName);

    #region Getters / Setters

    /// <summary>
    /// Must part appear at the top of the tree (instead of workorder or job) ?
    /// 
    /// Default value is false
    /// </summary>
    /// <returns></returns>
    public static bool PartAtTheTop
    {
      get {
        string configKey = ConfigKeys.GetOperationExplorerConfigKey (OperationExplorerConfigKey.PartAtTheTop);
        return Lemoine.Info.ConfigSet
          .LoadAndGet<bool> (configKey,
                             DEFAULT_PART_AT_THE_TOP);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private OperationExplorerConfigHelper ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region Instance
    static OperationExplorerConfigHelper Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly OperationExplorerConfigHelper instance = new OperationExplorerConfigHelper ();
    }
    #endregion // Instance
  }
}
