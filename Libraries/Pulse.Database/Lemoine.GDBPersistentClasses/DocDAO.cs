// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    /// <summary>
    /// <see cref="IDocDAO"/>
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>not null</returns>
    public async Task<IDoc> FindOrCreateByPathAsync (string path)
    {
      Debug.Assert (!string.IsNullOrEmpty (path));

      var doc = await FindByPathAsync (path);
      if (doc is not null) {
        return doc;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"FindOrCreateByPathAsync: create a new doc for path {path}");
      }
      var newDoc = ModelDAOHelper.ModelFactory.CreateDoc (path);
      return await MakePersistentAsync (newDoc);
    }

    /// <summary>
    /// <see cref="IDocDAO"/>
    ///
    /// The @> operator on the array is required for the gin index to be used
    /// </summary>
    /// <param name="tag">not null and not empty</param>
    /// <returns>not null, ordered by id</returns>
    public async Task<IList<IDoc>> FindByTagAsync (string tag)
    {
      if (string.IsNullOrEmpty (tag)) {
        log.Fatal ("FindByTagAsync: tag is null or empty");
        throw new ArgumentNullException (nameof (tag));
      }

      return await NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery ("""
SELECT {doc.*}
FROM doc {doc}
WHERE {doc}.doctags @> ARRAY[:tag]
ORDER BY {doc}.docid
""")
        .AddEntity ("doc", typeof (Doc))
        .SetString ("tag", tag.Trim ())
        .ListAsync<IDoc> ();
    }

    /// <summary>
    /// <see cref="IDocDAO"/>
    /// </summary>
    /// <param name="search">nullable or empty: text the path contains, whatever the case</param>
    /// <param name="tag">nullable or empty: tag the document carries (case sensitive, gin index)</param>
    /// <param name="kindId">nullable: id of the kind of the document</param>
    /// <param name="maxResults">maximum number of returned documents, strictly positive</param>
    /// <returns>not null, ordered by path</returns>
    public async Task<IList<IDoc>> FindBySearchAsync (string search, string tag, int? kindId, int maxResults)
    {
      if (maxResults <= 0) {
        log.Fatal ($"FindBySearchAsync: invalid maxResults {maxResults}");
        throw new ArgumentOutOfRangeException (nameof (maxResults));
      }

      // Native SQL: the @> operator on the array is required for the gin index to be used,
      // and a criteria SQL restriction does not bind a parameter inside ARRAY[...]
      var conditions = new List<string> ();
      if (!string.IsNullOrWhiteSpace (search)) {
        conditions.Add ("{doc}.docpath ILIKE :search");
      }
      if (!string.IsNullOrWhiteSpace (tag)) {
        conditions.Add ("{doc}.doctags @> ARRAY[:tag]");
      }
      if (kindId.HasValue) {
        conditions.Add ("{doc}.dockindid = :kindId");
      }
      var where = conditions.Any ()
        ? $"WHERE {string.Join (" AND ", conditions)}"
        : "";
      var query = NHibernateHelper.GetCurrentSession ()
        .CreateSQLQuery ($"""
SELECT {"{doc.*}"}
FROM doc {"{doc}"}
{where}
ORDER BY {"{doc}"}.docpath
""")
        .AddEntity ("doc", typeof (Doc));
      if (!string.IsNullOrWhiteSpace (search)) {
        query.SetString ("search", $"%{EscapeLike (search.Trim ())}%");
      }
      if (!string.IsNullOrWhiteSpace (tag)) {
        query.SetString ("tag", tag.Trim ());
      }
      if (kindId.HasValue) {
        query.SetInt32 ("kindId", kindId.Value);
      }
      return await query
        .SetMaxResults (maxResults)
        .ListAsync<IDoc> ();
    }

    /// <summary>
    /// Escape the special characters of a LIKE pattern (default escape character \)
    /// </summary>
    /// <param name="text">not null</param>
    /// <returns></returns>
    static string EscapeLike (string text) => text
      .Replace ("\\", "\\\\")
      .Replace ("%", "\\%")
      .Replace ("_", "\\_");
  }
}
