// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IDocDAO">IDocDAO</see>
  /// </summary>
  public class DocDAO
    : VersionableNHibernateDAO<Doc, IDoc, int>
    , IDocDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DocDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DocDAO ()
      : base ()
    {
    }

    /// <summary>
    /// <see cref="IDocDAO"/>
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>nullable</returns>
    public IDoc FindByPath (string path)
    {
      Debug.Assert (!string.IsNullOrEmpty (path));

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Doc> ()
        .Add (Restrictions.Eq ("Path", path))
        .SetCacheable (true)
        .UniqueResult<IDoc> ();
    }

    /// <summary>
    /// <see cref="IDocDAO"/>
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>nullable</returns>
    public async Task<IDoc> FindByPathAsync (string path)
    {
      Debug.Assert (!string.IsNullOrEmpty (path));

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Doc> ()
        .Add (Restrictions.Eq ("Path", path))
        .SetCacheable (true)
        .UniqueResultAsync<IDoc> ();
    }

    /// <summary>
    /// <see cref="IDocDAO"/>
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>not null</returns>
    public IDoc FindOrCreateByPath (string path)
    {
      Debug.Assert (!string.IsNullOrEmpty (path));

      var doc = FindByPath (path);
      if (doc is not null) {
        return doc;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"FindOrCreateByPath: create a new doc for path {path}");
      }
      var newDoc = ModelDAOHelper.ModelFactory.CreateDoc (path);
      return MakePersistent (newDoc);
    }
  }
}
