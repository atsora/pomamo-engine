// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.GDBUtils
{
  /// <summary>
  /// Details on an ExceptionDetails.
  /// 
  /// For PostgreSQL, this is taken from NpgsqlException or PostgresException
  /// </summary>
  public class DatabaseExceptionDetails
    : Lemoine.Core.ExceptionManagement.IDatabaseExceptionDetails
  {
    #region Members
    readonly string m_type;
    readonly bool m_postgresException;
    readonly bool m_adoException;
    readonly Exception m_databaseException;
    readonly string m_databaseExceptionMessage;
    readonly string m_sqlState = "";
    readonly string m_detail;
    readonly string m_errorSql;
    readonly string m_file;
    readonly string m_hint;
    readonly string m_line;
    readonly int m_position;
    readonly string m_routine;
    readonly string m_severity;
    readonly string m_where;
    readonly string m_columnName;
    readonly string m_constraintName;
    readonly string m_dataTypeName;
    readonly int m_internalPosition;
    readonly string m_internalQuery;
    readonly bool m_transient;
    readonly string m_messageText;
    readonly string m_schemaName;
    readonly string m_tableName;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Database exception source
    /// </summary>
    public Exception DatabaseException
    {
      get { return m_databaseException; }
    }
    
    /// <summary>
    /// Base message
    /// </summary>
    public string BaseMessage
    {
      get { return m_databaseExceptionMessage; }
    }
    
    /// <summary>
    /// Code / SqlState (previously Code)
    /// </summary>
    public string Code
    {
      get { return m_sqlState; }
    }
    
    /// <summary>
    /// Details
    /// </summary>
    public string Detail
    {
      get { return m_detail; }
    }
    
    /// <summary>
    /// Error SQL
    /// </summary>
    public string ErrorSql
    {
      get { return m_errorSql; }
    }
    
    /// <summary>
    /// File
    /// </summary>
    public string File
    {
      get { return m_file; }
    }
    
    /// <summary>
    /// Hint
    /// </summary>
    public string Hint
    {
      get { return m_hint; }
    }
    
    /// <summary>
    /// Line
    /// </summary>
    public string Line
    {
      get { return m_line; }
    }
    
    /// <summary>
    /// Position
    /// </summary>
    public string Position
    {
      get { return m_position.ToString (); }
    }
    
    /// <summary>
    /// Routine
    /// </summary>
    public string Routine
    {
      get { return m_routine; }
    }
    
    /// <summary>
    /// Severity
    /// </summary>
    public string Severity
    {
      get { return m_severity; }
    }
    
    /// <summary>
    /// Where
    /// </summary>
    public string Where
    {
      get { return m_where; }
    }
    #endregion // Getters / Setters
    
    static readonly ILog log = LogManager.GetLogger(typeof (DatabaseExceptionDetails).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal DatabaseExceptionDetails ()
    { }
    
    /// <summary>
    /// Constructor from NpgsqlException
    /// </summary>
    /// <param name="npgsqlException">not null</param>
    internal DatabaseExceptionDetails (Npgsql.NpgsqlException npgsqlException)
    {
      Debug.Assert (null != npgsqlException);

      m_postgresException = false;
      m_adoException = false;
      m_type = "NpgsqlException";
      m_databaseException = npgsqlException;
      m_databaseExceptionMessage = npgsqlException.Message;
      m_transient = npgsqlException.IsTransient;
    }

    /// <summary>
    /// Constructor from PostgresException
    /// </summary>
    /// <param name="postgresException">not null</param>
    internal DatabaseExceptionDetails (Npgsql.PostgresException postgresException)
    {
      Debug.Assert (null != postgresException);

      m_postgresException = true;
      m_adoException = false;
      m_type = "PostgresException";
      m_databaseException = postgresException;
      m_databaseExceptionMessage = postgresException.Message;
      m_sqlState = postgresException.SqlState;
      m_detail = postgresException.Detail;
      if (null != postgresException.Statement) {
        m_errorSql = postgresException.Statement.SQL;
      }
      m_file = postgresException.File;
      m_hint = postgresException.Hint;
      m_line = postgresException.Line;
      m_position = postgresException.Position;
      m_routine = postgresException.Routine;
      m_severity = postgresException.Severity;
      m_where = postgresException.Where;
      m_columnName = postgresException.ColumnName;
      m_constraintName = postgresException.ConstraintName;
      m_dataTypeName = postgresException.DataTypeName;
      m_internalPosition = postgresException.InternalPosition;
      m_internalQuery = postgresException.InternalQuery;
      m_transient = postgresException.IsTransient;
      m_messageText = postgresException.MessageText;
      m_schemaName = postgresException.SchemaName;
      m_tableName = postgresException.TableName;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="adoException"></param>
    internal DatabaseExceptionDetails (NHibernate.ADOException adoException)
    {
      Debug.Assert (null != adoException);

      m_postgresException = false;
      m_adoException = true;
      m_type = "NHibernate.ADOException";
      m_databaseException = adoException;
      m_databaseExceptionMessage = adoException.Message;
      m_errorSql = adoException.SqlString;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Object.ToString()"></see>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (m_postgresException) {
        return $"Type: {m_type} Message: {m_databaseExceptionMessage} Detail: {m_detail} Severity: {m_severity} Code: {m_sqlState} ErrorSql: {m_errorSql}";
      }
      else if (m_adoException) {
        return $"Type: {m_type} Message: {m_databaseExceptionMessage} ErrorSql: {m_errorSql}";
      }
      else {
        return $"Type: {m_type} Message: {m_databaseExceptionMessage} Transient: {m_transient}";
      }
    }
  }
}
