// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using System.Security.Cryptography;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IApplicationStateDAO">IApplicationStateDAO</see>
  /// </summary>
  public class ApplicationStateDAO
    : VersionableNHibernateDAO<ApplicationState, IApplicationState, int>
    , IApplicationStateDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ApplicationStateDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("ApplicationState.InsertDefaultValues")) {
          { // config.update
            var applicationState = new ApplicationState (ApplicationStateKey.ConfigUpdate.ToKey (),
                                                         DateTime.UtcNow);
            InsertDefaultValue (applicationState);
          }
          { // Salt
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create ()) {
              rng.GetBytes (salt);
            }
            var applicationState = new ApplicationState ("user.salt",
                                                         Convert.ToBase64String (salt));
            InsertDefaultValue (applicationState);
          }
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationState">not null</param>
    private void InsertDefaultValue (IApplicationState applicationState)
    {
      Debug.Assert (null != applicationState);

      try {
        if (null == GetNoCache (applicationState.Key)) { // the config does not exist => create it
          log.Info ($"InsertDefaultValue: add key={applicationState.Key} value={applicationState.Value}");
          MakePersistent (applicationState);
        }
      }
      catch (Exception ex) {
        log.Error ($"InsertDefaultValue: inserting new application state {applicationState} failed", ex);
      }
    }

    private void ResetSequence (int minId)
    {
      try {
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = string.Format (@"
WITH maxid AS (SELECT MAX({1}) AS maxid FROM {0})
SELECT SETVAL('{0}_{1}_seq', CASE WHEN (SELECT maxid FROM maxid) < {2} THEN {2} ELSE (SELECT maxid FROM maxid) + 1 END);",
                                               "role", "roleid",
                                               minId);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          command.ExecuteNonQuery ();
        }
      }
      catch (Exception ex) {
        log.Error ($"ResetSequence: resetting the sequence failed", ex);
      }
    }
    #endregion // DefaultValues

    /// <summary>
    /// Get the applicationState for the specified key
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IApplicationState GetApplicationState (string key)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ApplicationState> ()
        .Add (Restrictions.Eq ("Key", key))
        .SetCacheable (true)
        .UniqueResult<IApplicationState> ();
    }

    /// <summary>
    /// Get the applicationState for the specified key without any cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IApplicationState GetNoCache (string key)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ApplicationState> ()
        .Add (Restrictions.Eq ("Key", key))
        .UniqueResult<IApplicationState> ();
    }

    /// <summary>
    /// Update directly an application state
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void Update (string key, object v)
    {
      var query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("ApplicationStateUpdate");
      query.SetParameter ("key", key);
      query.SetParameter ("v", v);
      query.ExecuteUpdate ();
    }
  }
}
