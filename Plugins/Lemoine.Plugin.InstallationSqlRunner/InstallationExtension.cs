// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.Config;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.InstallationSqlRunner
{
  public class InstallationExtension
    : MultipleInstanceConfigurableExtension<Configuration>
    , IInstallationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (InstallationExtension).FullName);

    bool? m_initializationResult = null;
    Configuration m_configuration;
    string m_sqlRequestsDirectory;
    string m_path;

    bool Initialize ()
    {
      if (m_initializationResult.HasValue) {
        return m_initializationResult.Value;
      }

      var initializationResult = TryInitialize ();
      m_initializationResult = initializationResult;
      return initializationResult;
    }

    bool TryInitialize ()
    {
      m_sqlRequestsDirectory = Lemoine.Info.PulseInfo.SqlRequestsDir;
      if (string.IsNullOrEmpty (m_sqlRequestsDirectory)) {
        log.Error ($"TryInitialize: sql requests directory does not exist => return false");
        return false;
      }

      if (null == m_configuration) {
        if (!LoadConfiguration (out m_configuration)) {
          log.Error ("TryInitialize: LoadConfiguration error");
          return false;
        }
      }

      if (m_configuration.MinPostgreSQLVersion.HasValue || m_configuration.MaxPostgreSQLVersion.HasValue) {
        var postgreSQLVersionNum = ModelDAOHelper.DAOFactory.GetPostgreSQLVersionNum ();
        if (m_configuration.MinPostgreSQLVersion.HasValue
          && postgreSQLVersionNum < m_configuration.MinPostgreSQLVersion.Value) {
          log.Info ($"TryInitialize: PostgreSQL version {postgreSQLVersionNum} is lesser than {m_configuration.MinPostgreSQLVersion} for {m_configuration.SqlFileName} => return false");
          return false;
        }
        if (m_configuration.MaxPostgreSQLVersion.HasValue
          && m_configuration.MaxPostgreSQLVersion.Value <= postgreSQLVersionNum) {
          log.Info ($"TryInitialize: PostgreSQL version {postgreSQLVersionNum} is greater than {m_configuration.MaxPostgreSQLVersion} for {m_configuration.SqlFileName} => return false");
          return false;
        }
      }

      var fileName = m_configuration.SqlFileName.EndsWith (".sql")
        ? m_configuration.SqlFileName
        : m_configuration.SqlFileName + ".sql";
      m_path = Path.Combine (m_sqlRequestsDirectory, fileName);
      if (!File.Exists (m_path)) {
        if (m_configuration.MissingFileOk) {
          log.Info ($"TryInitialize: {m_path} does not exist => skip it");
        }
        else {
          log.Error ($"TryInitialize: {m_path} does not exist => skip it");
        }
        return false;
      }

      return true;
    }

    public double Priority
    {
      get {
        if (!Initialize ()) {
          log.Info ("CheckConfig: Initialized failed");
          return 0.0;
        }
        return m_configuration.Priority;
      }
    }

    public bool CheckConfig ()
    {
      if (!Initialize ()) {
        log.Info ("CheckConfig: Initialized failed");
        return false;
      }

      string sqlString;
      try {
        sqlString = File.ReadAllText (m_path);
      }
      catch (Exception ex) {
        log.Error ($"CheckConfig: exception while reading {m_path}", ex);
        return false;
      }

      try {
        var database = new Lemoine.GDBMigration.TransformationProviderExt ();
        var separator = m_configuration.Separator;
        if (string.IsNullOrEmpty (separator)) {
          database.ExecuteNonQuery (sqlString, sqlErrorOk: m_configuration.SqlErrorOk);
        }
        else {
          database.ExecuteSetOfQueries (sqlString, separator, sqlErrorOk: m_configuration.SqlErrorOk);
        }
        return true;
      }
      catch (Exception ex) {
        if (m_configuration.SqlErrorOk) {
          log.Info ($"CheckConfig: exception", ex);
        }
        else {
          log.Error ($"CheckConfig: exception", ex);
        }
        return false;
      }
    }

    public bool RemoveConfig ()
    {
      // Do nothing
      return true;
    }
  }
}
