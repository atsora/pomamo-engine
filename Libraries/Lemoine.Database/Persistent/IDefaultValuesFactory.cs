// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using NHibernate;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// 
  /// </summary>
  public interface IDefaultValuesFactory
  {
    /// <summary>
    /// Create a <see cref="IDefaultValues"/> with no cache
    /// </summary>
    /// <param name="sessionFactory"></param>
    /// <returns></returns>
    IDefaultValues CreateNocache (ISessionFactory sessionFactory);
  }
}
