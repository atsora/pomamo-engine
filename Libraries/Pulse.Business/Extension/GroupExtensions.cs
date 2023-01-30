// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;

namespace Lemoine.Business.Extension
{
  /// <summary>
  /// Request class to get extensions (from cache) that depend on a Group
  /// </summary>
  public class GroupExtensions<T>
    : ParameterExtensions<T, IGroup>
    , IRequest<IEnumerable<T>>
    where T : Lemoine.Extensions.IExtension
  {
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="Group"></param>
    public GroupExtensions (IGroup Group)
      : base (Group, g => g.Id)
    {
    }

    /// <summary>
    /// Constructor with a filter lambda function
    /// 
    /// To be used when the Initialize method is available in the extension and returns true
    /// </summary>
    /// <param name="Group"></param>
    /// <param name="filter"></param>
    public GroupExtensions (IGroup Group, Func<T, IGroup, bool> filter)
      : base (Group, g => g.Id, filter)
    {
    }
    #endregion // Constructors
  }
}
