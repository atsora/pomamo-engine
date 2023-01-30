// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lemoine.WebMiddleware.Assemblies
{
  /// <summary>
  /// Interface of classes that return the list of assemblies
  /// that contain web service contract and handlers
  /// </summary>
  public interface IServiceAssembliesResolver
  {
    /// <summary>
    /// Get the assemblies
    /// </summary>
    /// <returns></returns>
    IEnumerable<Assembly> GetServiceAssemblies ();
  }
}
