// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IPackagePluginAssociationDAO">IPackagePluginAssociationDAO</see>
  /// </summary>
  public class PackagePluginAssociationDAO
    : VersionableNHibernateDAO<PackagePluginAssociation, IPackagePluginAssociation, int>
    , IPackagePluginAssociationDAO
  {
    /// <summary>
    /// Find the configuration corresponding to a package, a plugin and a name
    /// </summary>
    /// <param name="package"></param>
    /// <param name="plugin"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public IPackagePluginAssociation FindByPackagePluginName (IPackage package, IPlugin plugin, string name)
    {
      if (string.IsNullOrEmpty (name)) {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<PackagePluginAssociation> ()
          .Add (Restrictions.Eq ("Package", package))
          .Add (Restrictions.Eq ("Plugin", plugin))
          .Add (Restrictions.IsNull ("Name"))
          .UniqueResult<IPackagePluginAssociation> ();
      }
      else { // Not empty
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<PackagePluginAssociation> ()
          .Add (Restrictions.Eq ("Package", package))
          .Add (Restrictions.Eq ("Plugin", plugin))
          .Add (Restrictions.Eq ("Name", name))
          .UniqueResult<IPackagePluginAssociation> ();
      }
    }

    /// <summary>
    /// Find the configurations corresponding to a package and a plugin
    /// </summary>
    /// <param name="package"></param>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public IList<IPackagePluginAssociation> FindByPackageAndPlugin (IPackage package, IPlugin plugin)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<PackagePluginAssociation> ()
        .Add (Restrictions.Eq ("Package", package))
        .Add (Restrictions.Eq ("Plugin", plugin))
        .List<IPackagePluginAssociation> ();
    }

    /// <summary>
    /// Find all associations related to a package
    /// with an early fetch of the plugin
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    public IList<IPackagePluginAssociation> FindByPackage (IPackage package)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<PackagePluginAssociation> ()
        .Add (Restrictions.Eq ("Package", package))
        .Fetch (SelectMode.Fetch, "Plugin")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IPackagePluginAssociation> ();
    }

    /// <summary>
    /// Find all associations related to a plugin
    /// with an early fetch of the package.
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public IList<IPackagePluginAssociation> FindByPlugin (IPlugin plugin)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<PackagePluginAssociation> ()
        .Add (Restrictions.Eq ("Plugin", plugin))
        .Fetch (SelectMode.Fetch, "Package")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IPackagePluginAssociation> ();
    }

    /// <summary>
    /// Find all associations
    /// with an early fetch of the package and the plugin.
    /// </summary>
    /// <returns></returns>
    public IList<IPackagePluginAssociation> FindAllWithPackagePlugin ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<PackagePluginAssociation> ()
        .Fetch (SelectMode.Fetch, "Package")
        .Fetch (SelectMode.Fetch, "Plugin")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IPackagePluginAssociation> ();
    }

    /// <summary>
    /// Find all associations
    /// with an early fetch of the package and the plugin
    /// asynchronously
    /// </summary>
    /// <returns></returns>
    public async Task<IList<IPackagePluginAssociation>> FindAllWithPackagePluginAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<PackagePluginAssociation> ()
        .Fetch (SelectMode.Fetch, "Package")
        .Fetch (SelectMode.Fetch, "Plugin")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .ListAsync<IPackagePluginAssociation> ();
    }
  }
}
