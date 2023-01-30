// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Package
{
  /// <summary>
  /// Description of a package
  /// </summary>
  public class PackageDescription
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PackageDescription).FullName);

    #region Getters / Setters
    /// <summary>
    /// Identifier (unique)
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// Name to display
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Associated tags
    /// </summary>
    public IList<string> Tags { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Extra files
    /// 
    /// destination: source
    /// </summary>
    public IDictionary<string, string> ExtraFiles { get; set; }

    /// <summary>
    /// Associated plugins
    /// </summary>
    public IList<PluginDescription> Plugins { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PackageDescription ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
