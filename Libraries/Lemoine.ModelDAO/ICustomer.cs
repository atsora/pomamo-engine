// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Customer interface
  /// </summary>
  public interface ICustomer : IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<ICustomer>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Full name of the customer as used in the shop
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Code given to the customer
    /// </summary>
    string Code { get; set; }

    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing PUSLE data with an external database
    /// </summary>
    string ExternalCode { get; set; }

    /// <summary>
    /// Check if the customer is undefined
    /// 
    /// A customer is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    bool IsUndefined ();
  }
}
