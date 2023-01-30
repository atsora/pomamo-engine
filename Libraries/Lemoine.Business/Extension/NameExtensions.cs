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

namespace Lemoine.Business.Extension
{
  /// <summary>
  /// Request class to get extensions (from cache) that depend on a name
  /// </summary>
  public class NameExtensions<T>
    : ParameterExtensions<T, string>
    , IRequest<IEnumerable<T>>
    where T : Lemoine.Extensions.IExtension, Lemoine.Extensions.Extension.Categorized.INamed
  {
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    public NameExtensions (string name)
      : base (name, m => m, FilterName)
    {
    }

    /// <summary>
    /// Alternatice constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="filter"></param>
    public NameExtensions (string name, Func<T, bool> filter)
      : base (name, m => m, (ext, n) => FilterName (ext, n) && filter (ext))
    { 
    }

    static bool FilterName (T extension, string name)
    {
      return string.Equals (extension.Name, name, StringComparison.InvariantCultureIgnoreCase);
    }
    #endregion // Constructors
  }
}
