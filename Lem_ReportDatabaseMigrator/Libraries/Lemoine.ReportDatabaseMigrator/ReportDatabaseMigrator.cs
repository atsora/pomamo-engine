// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Info;
using Npgsql;

namespace Lemoine.ReportDatabaseMigrator
{
  /// <summary>
  /// Description of MyClass.
  /// </summary>
  public class ReportDatabaseMigrator
  {
    #region Members
    readonly ConnectionParameters m_connection;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReportDatabaseMigrator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Basic constructor. Uses default DSN
    /// </summary>
    public ReportDatabaseMigrator ()
    {
      m_connection = new ConnectionParameters();
      // Load default connection
      m_connection.LoadGDBParameters();
    }
    
    /// <summary>
    /// Constructor initialising DSN
    /// </summary>
    public ReportDatabaseMigrator (string dsnName)
    {
      m_connection = new ConnectionParameters(dsnName);
      // Load default connection
      m_connection.LoadParameters();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Execute a SQL query
    /// </summary>
    public void ExecuteSQLstring (string query)
    {
      try {
        var connectionString = m_connection.GetConnectionString ("Lem_ReportDatabaseMigrator");

        using (var conn = new NpgsqlConnection (connectionString)) {
          conn.Open ();

          using (var cmd = new NpgsqlCommand (query, conn)) {
            cmd.ExecuteNonQuery ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ("ExecuteSQLString: exception", ex);
        throw;
      }
    }
    #endregion // Methods
  }
}
