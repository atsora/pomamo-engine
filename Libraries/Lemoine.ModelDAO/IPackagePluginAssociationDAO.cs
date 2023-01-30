// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IPackagePluginAssociation.
  /// </summary>
  public interface IPackagePluginAssociationDAO : IGenericDAO<IPackagePluginAssociation, int>
  {
    /// <summary>
    /// Find the configuration corresponding to a package, a plugin and a name
    /// </summary>
    /// <param name="package"></param>
    /// <param name="plugin"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    IPackagePluginAssociation FindByPackagePluginName (IPackage package, IPlugin plugin, string name);

    /// <summary>
    /// Find the configuration corresponding to a package and a plugin
    /// </summary>
    /// <param name="package"></param>
    /// <param name="plugin"></param>
    /// <returns></returns>
    IList<IPackagePluginAssociation> FindByPackageAndPlugin (IPackage package, IPlugin plugin);

    /// <summary>
    /// Find all associations related to a package
    /// with an early fetch of the plugins
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    IList<IPackagePluginAssociation> FindByPackage (IPackage package);

    /// <summary>
    /// Find all associations related to a plugin
    /// with an early fetch of the package
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    IList<IPackagePluginAssociation> FindByPlugin (IPlugin plugin);

    /// <summary>
    /// Find all associations
    /// with an early fetch of the package and the plugin.
    /// </summary>
    /// <returns></returns>
    IList<IPackagePluginAssociation> FindAllWithPackagePlugin ();

    /// <summary>
    /// Find all associations
    /// with an early fetch of the package and the plugin
    /// asynchronously
    /// </summary>
    /// <returns></returns>
    Task<IList<IPackagePluginAssociation>> FindAllWithPackagePluginAsync ();
  }
}
