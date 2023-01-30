// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IPackagePluginAssociation.
  /// </summary>
  public interface IPackagePluginAssociation: IVersionable, IPluginInstance
  {
    /// <summary>
    /// Parameters
    /// </summary>
    string Parameters { get; set; }
    
    /// <summary>
    /// Plugin
    /// </summary>
    IPlugin Plugin { get; }
    
    /// <summary>
    /// Package
    /// </summary>
    IPackage Package { get; }
    
    /// <summary>
    /// Name (nullable)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Active
    /// </summary>
    bool Active { get; set; }

    /// <summary>
    /// Custom
    /// </summary>
    bool Custom { get; set; }
  }
}
