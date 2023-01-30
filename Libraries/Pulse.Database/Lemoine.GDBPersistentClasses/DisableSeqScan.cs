// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Disable the sequential scan for a request
  /// 
  /// Useful to lock only a single row
  /// </summary>
  internal class DisableSeqScan: IDisposable, IAsyncDisposable
  {
    static readonly bool DEFAULT_ACTIVATION = false;
    
    readonly bool m_active;
    
    ILog log = LogManager.GetLogger(typeof (DisableSeqScan).FullName);

    /// <summary>
    /// Constructor: disable the sequential scan
    /// </summary>
    public DisableSeqScan ()
    {
      m_active = true;
      
      Initialize ();
    }

    /// <summary>
    /// Constructor: disable the sequential scan
    /// only if the option DisableSeqScan.tableName.Activate is true
    /// </summary>
    /// <param name="tableName"></param>
    public DisableSeqScan (string tableName)
    {
      m_active = IsActive (tableName);
      
      Initialize ();
    }
    
    bool IsActive (string tableName)
    {
      return Lemoine.Info.ConfigSet.LoadAndGet<bool> (string.Format ("DisableSeqScan.{0}.Activate",
                                                                     tableName),
                                                      DEFAULT_ACTIVATION);
    }
    
    void Initialize ()
    {
      if (m_active) {
        var connection = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.GetConnection ();
        using (var command = connection.CreateCommand ())
        {
          command.CommandText = "set enable_seqscan to off;";
          try {
            command.ExecuteNonQuery();
          }
          catch (Exception ex) {
            log.ErrorFormat ("Initialize: " +
                             "request error in SQL query {0} \n" +
                             "Exception: {1}",
                             command.CommandText,
                             ex);
          }
        }
      }
    }
    
    /// <summary>
    /// Dispose: restore the default parameters
    /// </summary>
    public void Dispose ()
    {
      if (m_active) {
        var connection = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.GetConnection ();
        using (var command = connection.CreateCommand ())
        {
          command.CommandText = "reset enable_seqscan;";
          try {
            command.ExecuteNonQuery();
          }
          catch (Exception ex) {
            log.ErrorFormat ("Dispose: " +
                             "request error in SQL query {0} \n" +
                             "Exception: {1}",
                             command.CommandText,
                             ex);
          }
        }
      }
    }

    /// <summary>
    /// Dispose: restore the default parameters
    /// </summary>
    public async ValueTask DisposeAsync ()
    {
      if (m_active) {
        var connection = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.GetConnection ();
        using (var command = connection.CreateCommand ()) {
          command.CommandText = "reset enable_seqscan;";
          try {
            await command.ExecuteNonQueryAsync ();
          }
          catch (Exception ex) {
            log.Error ($"DisposeAsync: request error in SQL query {command.CommandText}", ex);
          }
        }
      }
    }
  }

}
