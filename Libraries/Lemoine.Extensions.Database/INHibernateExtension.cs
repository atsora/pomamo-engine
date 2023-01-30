// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Database
{
  /// <summary>
  /// Description of INHibernateExtension.
  /// </summary>
  public interface INHibernateExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Does the assembly contain some NHibernate mappings ?
    /// </summary>
    /// <returns></returns>
    bool ContainsMapping ();

    /// <summary>
    /// Update the NHibernate configuration if required
    /// </summary>
    /// <param name="configuration"></param>
    void UpdateConfiguration (ref NHibernate.Cfg.Configuration configuration);
  }
}
