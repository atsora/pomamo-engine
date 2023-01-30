// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// 
  /// </summary>
  public interface ICustomerDAO
    : IGenericUpdateDAO<ICustomer, int>
    , IMergeDAO<ICustomer>
  {
    /// <summary>
    /// Find Customer by Name
    /// </summary>
    /// <param name="name">not null or empty</param>
    /// <returns></returns>
    ICustomer FindByName (string name);

    /// <summary>
    /// Find Customer by Code
    /// </summary>
    /// <param name="code">not null or empty</param>
    /// <returns></returns>
    ICustomer FindByCode (string code);

    /// <summary>
    /// Find customers whose names match a pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    IList<ICustomer> FindByNameStartPattern (string pattern);
  }
}
