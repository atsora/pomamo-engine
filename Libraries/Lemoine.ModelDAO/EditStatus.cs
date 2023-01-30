// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;

namespace Lemoine.Model
{
  /// <summary>
  /// Status of a record in the database
  /// It helps updating values at the client
  /// (we only override default values that have not been edited)
  /// </summary>
  public enum EditStatus {
    /// <summary>
    /// Someone added this value
    /// </summary>
    MANUAL_INPUT = 0,
    
    /// <summary>
    /// The DAO layer added this default value and noone changed it
    /// It is thus possible to update it with a newer version
    /// </summary>
    DEFAULT_VALUE = 1,
    
    /// <summary>
    /// The DAO layer added this default value which has been updated by someone else
    /// It is not possible anymore to update it with a newer version
    /// </summary>
    DEFAULT_VALUE_EDITED = 2,
    
    /// <summary>
    /// The DAO layer added this default value which has been deleted by someone else
    /// The record is kept in the database so that we know this record must not be written again
    /// </summary>
    DEFAULT_VALUE_DELETED = 3,
  };
}
