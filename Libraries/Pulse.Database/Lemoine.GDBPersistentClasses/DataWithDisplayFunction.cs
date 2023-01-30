// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Base class for some persistent classes
  /// 
  /// This allows a set of persistent classes to have a same base set of methods
  /// </summary>
  [Serializable]
  public abstract class DataWithDisplayFunction: BaseData, IDisplayable
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DataWithDisplayFunction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Display name that is retrieved with a display function
    /// </summary>
    [XmlIgnore]
    public virtual string Display
    {
      get; set;
    }
    #endregion // Getters / Setters
  }
}
