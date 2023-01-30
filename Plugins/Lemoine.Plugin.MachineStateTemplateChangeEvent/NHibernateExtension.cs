// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Plugin.MachineStateTemplateChangeEvent
{
  /// <summary>
  /// Description of NHibernateExtension.
  /// </summary>
  public class NHibernateExtension: Lemoine.Extensions.Database.INHibernateExtension
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (NHibernateExtension).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public NHibernateExtension ()
    {
    }
    #endregion // Constructors

    #region INHibernateExtension implementation

    public bool ContainsMapping()
    {
      return true;
    }

    public void UpdateConfiguration(ref NHibernate.Cfg.Configuration configuration)
    {
    }
    #endregion

    #region IExtension implementation

    public bool UniqueInstance {
      get {
        return true;
      }
    }

    #endregion
  }
}
