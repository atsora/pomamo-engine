// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Extension;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Pulse.Extensions.Extension
{
  /// <summary>
  /// Extensions to <see cref="IConfigurable" >
  /// </summary>
  public static class ConfigurableExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConfigurableExtensions).FullName);

    /// <summary>
    /// For the unit tests
    /// </summary>
    /// <param name="configuration"></param>
    public static void SetTestConfiguration (this IConfigurable extension, string configuration)
    {
      IPackage package = ModelDAOHelper.ModelFactory
        .CreatePackage ("Test");
      IPlugin plugin = ModelDAOHelper.ModelFactory
        .CreatePlugin ("Test");
      IPackagePluginAssociation association = ModelDAOHelper.ModelFactory
        .CreatePackagePluginAssociation (package, plugin, null);
      association.Parameters = configuration;
      extension.AddConfigurationContext (association);
    }

  }
}
