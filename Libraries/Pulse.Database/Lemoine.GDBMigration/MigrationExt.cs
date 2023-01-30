// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Migrator.Framework;
using Lemoine.Core.Log;
using System.Data;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Extended Migration class
  /// that contains some new methods to help the migration of the database
  /// </summary>
  public abstract class MigrationExt: Migration
  {
    readonly ILog log = LogManager.GetLogger(typeof (MigrationExt).FullName);

    TransformationProviderExt m_transformationProviderExt;

    /// <summary>
    /// Constructor
    /// </summary>
    public MigrationExt ()
      : base ()
    {
      m_transformationProviderExt = new TransformationProviderExt (this);
    }

    /// <summary>
    /// Associated TransformationProviderExt
    /// </summary>
    TransformationProviderExt TransformationProviderExt {
      get { return m_transformationProviderExt;  }
    }

    #region Schemas
    /// <summary>
    /// Remove a schema it it exists
    /// </summary>
    /// <param name="schemaName"></param>
    internal void RemoveSchema (string schemaName)
    {
      m_transformationProviderExt.ExecuteNonQuery ($"DROP SCHEMA IF EXISTS {schemaName} CASCADE;");
    }
    #endregion // Schemas

    #region Tables
    /// <summary>
    /// Remove a table if it exists
    /// </summary>
    /// <param name="tableName"></param>
    internal void RemoveTable (string tableName)
    {
      try {
        if (m_transformationProviderExt.TableExists (tableName)) {
          m_transformationProviderExt.RemoveTable (tableName);
        }
        else {
          log.Info ($"RemoveTable: table={tableName} did not exist");
        }
      }
      catch (Exception ex) {
        log.Error ($"RemoveTable: table={tableName}, exception", ex);
        throw;
      }
    }
    #endregion // Tables

    /// <summary>
    /// Remove a column (cascade)
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    internal void RemoveColumnCascade (string tableName, string columnName)
    {
      m_transformationProviderExt.RemoveColumnCascade (tableName, columnName);
    }

    #region Make columns
    /// <summary>
    /// Convert the type of a column to text
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void MakeColumnText (string table, string column)
    {
      m_transformationProviderExt.MakeColumnText (table, column);
    }
    
    /// <summary>
    /// Make a column case insensitive
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void MakeColumnCaseInsensitive (string table, string column)
    {
      m_transformationProviderExt.MakeColumnCaseInsensitive (table, column);
    }
    
    /// <summary>
    /// Convert a column to a tsrange type
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void MakeColumnTsRange (string table, string column)
    {
      m_transformationProviderExt.MakeColumnTsRange (table, column);
    }
    
    /// <summary>
    /// Convert a column to a tsrange type
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void MakeColumnDateRange (string table, string column)
    {
      m_transformationProviderExt.MakeColumnDateRange (table, column);
    }
    
    /// <summary>
    /// Convert a column to a json type (or jsonb if postgresql version >= 9.4.0)
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void MakeColumnJson(string table, string column)
    {
      if (GetPostgresqlVersion () < 9004000) {
        throw new Exception ("Please upgrade postgreSQL (version >= 9.4) so that a jsonb column can be created.");
      }

      m_transformationProviderExt.MakeColumnJsonB(table, column);
    }
    #endregion // Make columns
    
    #region Remove constraints
    /// <summary>
    /// Remove a constraint given its name,
    /// check if the table is partitioned
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="name">Constraint name</param>
    internal void RemoveConstraint (string table, string name)
    {
      m_transformationProviderExt.RemoveConstraint (table, name);
    }
    
    /// <summary>
    /// Remove a unique constraint,
    /// check if the table is partitioned
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>"
    internal void RemoveUniqueConstraint (string table, params string[] columnNames)
    {
      m_transformationProviderExt.RemoveUniqueConstraint (table, columnNames);
    }
    
    /// <summary>
    /// Remove a non-overlapping constraint,
    /// check if the table is partitioned
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void RemoveNoOverlapConstraint (string table, params string[] columnNames)
    {
      m_transformationProviderExt.RemoveNoOverlapConstraint (table, columnNames);
    }
    
    /// <summary>
    /// Remove a color constraint
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void RemoveConstraintColor (string table, string column)
    {
      m_transformationProviderExt.RemoveConstraintColor (table, column);
    }
    #endregion // Remove constraints

    #region Add constraints
    /// <summary>
    /// Add a unique deferable initially deferred constraint
    /// </summary>
    /// <param name="name"></param>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void AddNamedUniqueConstraint (string name, string table, params string[] columnNames)
    {
      m_transformationProviderExt.AddNamedUniqueConstraint (name, table, columnNames);
    }
    
    /// <summary>
    /// Add a unique deferable initially deferred constraint
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void AddUniqueConstraint (string table, params string[] columnNames)
    {
      m_transformationProviderExt.AddUniqueConstraint (table, columnNames);
    }
    
    /// <summary>
    /// Add a non-overlapping deferable initially deferred constraint
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void AddNoOverlapConstraintV1 (string table, string column)
    {
      m_transformationProviderExt.AddNoOverlapConstraintV1 (table, column);
    }
    
    /// <summary>
    /// Add a non-overlapping deferable initially deferred constraint
    /// An index is made with columnNames
    /// For each index, rangecolumn won't overlap
    /// </summary>
    /// <param name="table"></param>
    /// <param name="rangecolumn"></param>
    /// <param name="columnNames"></param>
    internal void AddNoOverlapConstraint (string table,
                                          string rangecolumn,
                                          params string[] columnNames)
    {
      m_transformationProviderExt.AddNoOverlapConstraint (table, rangecolumn, columnNames);
    }

    /// <summary>
    /// Add a non-overlapping deferable initially deferred constraint
    /// An index is made with columnNames
    /// For each index, rangecolumn won't overlap
    /// </summary>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="rangecolumn"></param>
    /// <param name="columnNames"></param>
    internal void AddNoOverlapConstraintCondition (string table,
                                                   string condition,
                                                   string rangecolumn,
                                                   params string[] columnNames)
    {
      m_transformationProviderExt
        .AddNoOverlapConstraintCondition (table, condition, rangecolumn, columnNames);
    }

    /// <summary>
    /// Add a constraint with a default name
    /// between a name column and its
    /// corresponding translation key column,
    /// so that not both of them are null
    /// </summary>
    /// <param name="table"></param>
    /// <param name="nameColumn"></param>
    /// <param name="translationKeyColumn"></param>
    internal void AddConstraintNameTranslationKey (string table,
                                                   string nameColumn,
                                                   string translationKeyColumn)
    {
      m_transformationProviderExt.AddConstraintNameTranslationKey (table, nameColumn, translationKeyColumn);
    }
    
    /// <summary>
    /// Add a check constraint so that the column is NULL or a web color
    /// #000000
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void AddConstraintNullColor (string table, string column)
    {
      m_transformationProviderExt.AddConstraintNullColor (table, column);
    }
    
    /// <summary>
    /// Add a check constraint so that the column is a web color
    /// #000000
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void AddConstraintColor (string table, string column)
    {
      m_transformationProviderExt.AddConstraintColor (table, column);
    }
    
    internal void AddConstraintPositive (string table, string column)
    {
      m_transformationProviderExt.AddConstraintPositive (table, column);
    }
    
    /// <summary>
    /// Check a range has a duration strictly positive (not 0s and not empty)
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void AddConstraintRangeStrictlyPositiveDuration (string table, string column)
    {
      m_transformationProviderExt.AddConstraintRangeStrictlyPositiveDuration (table, column);
    }
    
    internal void AddConstraintRangeNotEmpty (string table, string column)
    {
      m_transformationProviderExt.AddConstraintRangeNotEmpty (table, column);
    }
    
    /// <summary>
    /// Add a non negative constraint to a column
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void AddNonNegativeConstraint (string table, string column)
    {
      m_transformationProviderExt.AddNonNegativeConstraint (table, column);
    }
    
    /// <summary>
    /// Add the constraint one of the two given columns must not be null
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column1"></param>
    /// <param name="column2"></param>
    internal void AddOneNotNullConstraint (string table, string column1, string column2)
    {
      m_transformationProviderExt.AddOneNotNullConstraint (table, column1, column2);
    }
    
    /// <summary>
    /// Set a NOT NULL property to a column
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void SetNotNull (string table, string column)
    {
      m_transformationProviderExt.SetNotNull (table, column);
    }

    /// <summary>
    /// Drop a NOT NULL property to a column
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void DropNotNull (string table, string column)
    {
      m_transformationProviderExt.DropNotNull (table, column);
    }

    /// <summary>
    /// Drop a default value
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void DropDefault (string table, string column)
    {
      m_transformationProviderExt.DropDefault (table, column);
    }
    #endregion // Add constraints

    #region Add / Remove index
    /// <summary>
    /// Add an index on columns
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void AddIndex (string table, params string[] columnNames)
    {
      m_transformationProviderExt.AddIndex (table, columnNames);
    }
    
    /// <summary>
    /// Add an index on columns
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void AddGistIndex (string table, params string[] columnNames)
    {
      m_transformationProviderExt.AddGistIndex (table, columnNames);
    }
    
    /// <summary>
    /// Add a gist index on a single column with a condition
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    /// <param name="condition"></param>
    internal void AddGistIndexCondition (string table, string column, string condition)
    {
      m_transformationProviderExt.AddGistIndexCondition (table, column, condition);
    }
    
    /// <summary>
    /// Add an index on one column
    /// </summary>
    /// <param name="name"></param>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void AddNamedIndex (string name, string table, params string[] columnNames)
    {
      m_transformationProviderExt.AddNamedIndex (name, table, columnNames);
    }
    
    /// <summary>
    /// Add an index on columns with a condition and a name (without _idx)
    /// </summary>
    /// <param name="name">Name of the index without the _idx suffix</param>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="columnNames"></param>
    internal void AddNamedIndexCondition (string name,
                                          string table,
                                          string condition,
                                          params string[] columnNames)
    {
      m_transformationProviderExt.AddNamedIndexCondition (name, table, condition, columnNames);
    }

    /// <summary>
    /// Add an index on a single column with a condition and a name (without _idx)
    /// </summary>
    /// <param name="name">Name of the index without the _idx suffix</param>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="columnNames"></param>
    internal void AddNamedGistIndexCondition (string name,
                                              string table,
                                              string condition,
                                              params string[] columnNames)
    {
      m_transformationProviderExt.AddNamedGistIndexCondition (name, table, condition, columnNames);
    }
    
    /// <summary>
    /// Add an index on columns with a condition
    /// </summary>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="columnNames"></param>
    internal void AddIndexCondition (string table,
                                     string condition,
                                     params string[] columnNames)
    {
      m_transformationProviderExt.AddIndexCondition (table, condition, columnNames);
    }

    /// <summary>
    /// Add a unique index on columns (not deferrable!)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void AddNamedUniqueIndex (string name, string table, params string[] columnNames)
    {
      m_transformationProviderExt.AddNamedUniqueIndex (name, table, columnNames);
    }

    /// <summary>
    /// Add a unique index on columns (not deferrable!)
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void AddUniqueIndex (string table, params string[] columnNames)
    {
      m_transformationProviderExt.AddUniqueIndex (table, columnNames);
    }

    /// <summary>
    /// Add a unique index on three columns with a condition
    /// (not deferrable!)
    /// </summary>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="columnNames"></param>
    internal void AddUniqueIndexCondition (string table,
                                           string condition,
                                           params string[] columnNames)
    {
      m_transformationProviderExt.AddUniqueIndexCondition (table, condition, columnNames);
    }

    /// <summary>
    /// Remove an index (with its real name), check if the table is partitioned
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="table"></param>
    internal void RemoveRealNameIndex (string indexName, string table)
    {
      m_transformationProviderExt.RemoveRealNameIndex (indexName, table);
    }

    /// <summary>
    /// Remove an index with an automatically built name, check if the table is partitioned
    /// </summary>
    /// <param name="name"></param>
    /// <param name="table"></param>
    internal void RemoveNamedIndex (string name, string table)
    {
      m_transformationProviderExt.RemoveNamedIndex (name, table);
    }

    /// <summary>
    /// Remove an index, check if the table is partitioned
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    internal void RemoveIndex (string table, params string[] columnNames)
    {
      m_transformationProviderExt.RemoveIndex (table, columnNames);
    }
    
    /// <summary>
    /// Remove an index with a condition
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    internal void RemoveIndexCondition (string table, string column)
    {
      m_transformationProviderExt.RemoveIndexCondition (table, column);
    }
    #endregion // Add / Remove index

    #region Sequences
    /// <summary>
    /// Reset the sequence of an ID
    /// </summary>
    /// <param name="table"></param>
    /// <param name="idColumn"></param>
    internal void ResetSequence (string table, string idColumn)
    {
      m_transformationProviderExt.ResetSequence (table, idColumn);
    }
    
    /// <summary>
    /// Set the sequence of an ID to a given value
    /// </summary>
    /// <param name="table"></param>
    /// <param name="idColumn"></param>
    /// <param name="sequenceValue"></param>
    internal void SetSequence (string table, string idColumn, int sequenceValue)
    {
      m_transformationProviderExt.SetSequence (table, idColumn, sequenceValue);
    }

    /// <summary>
    /// Set a minimum value to a sequence of an ID considering the existing values
    /// </summary>
    /// <param name="table"></param>
    /// <param name="idColumn"></param>
    /// <param name="minSequenceValue"></param>
    internal void SetMinSequence (string table, string idColumn, int minSequenceValue)
    {
      m_transformationProviderExt.SetMinSequence (table, idColumn, minSequenceValue);
    }
    
    /// <summary>
    /// Associate a column to an existing sequence
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="sequenceName"></param>
    internal void SetSequence (string tableName, string columnName, string sequenceName)
    {
      m_transformationProviderExt.SetSequence (tableName, columnName, sequenceName);
    }
    
    /// <summary>
    /// Create a sequence
    /// </summary>
    /// <param name="sequenceName"></param>
    internal void AddSequence (string sequenceName)
    {
      m_transformationProviderExt.AddSequence (sequenceName);
    }

    /// <summary>
    /// Remove a sequence
    /// </summary>
    /// <param name="sequenceName"></param>
    internal void RemoveSequence (string sequenceName)
    {
      m_transformationProviderExt.RemoveSequence (sequenceName);
    }
    #endregion // Sequences
    
    #region Modification table
    /// <summary>
    /// Set a table as a modification
    /// </summary>
    /// <param name="table"></param>
    internal void SetModificationTable (string table)
    {
      m_transformationProviderExt.SetModificationTable (table);
    }
    
    /// <summary>
    /// Set a table as a machinemodification
    /// </summary>
    /// <param name="table"></param>
    internal void SetMachineModificationTable (string table)
    {
      m_transformationProviderExt.SetMachineModificationTable (table);
    }
    
    /// <summary>
    /// Set a table as a globalmodification
    /// </summary>
    /// <param name="table"></param>
    internal void SetGlobalModificationTable (string table)
    {
      m_transformationProviderExt.SetGlobalModificationTable (table);
    }
    
    /// <summary>
    /// Remove a modification table
    /// </summary>
    /// <param name="persistentClassName">Camel case persistent class name (not lower case table name)</param>
    internal void RemoveModificationTable (string persistentClassName)
    {
      m_transformationProviderExt.RemoveModificationTable (persistentClassName);
    }
    #endregion // Modification table
    
    #region Partitioning
    internal bool IsPartitioned (string tableName)
    {
      return m_transformationProviderExt.IsPartitioned (tableName);
    }
    
    // Partition a table, return true on success
    internal bool PartitionTable(string tableName, string partitioningTableName)
    {
      return m_transformationProviderExt.PartitionTable (tableName, partitioningTableName);
    }
    
    // Unpartition a table, return true on success
    internal bool UnpartitionTable(string tableName)
    {
      return m_transformationProviderExt.UnpartitionTable (tableName);
    }
    #endregion // Partitioning
    
    #region Timestamp trigger
    /// <summary>
    /// Add a time stamp trigger to a table
    /// </summary>
    /// <param name="tableName"></param>
    protected void AddTimeStampTrigger (string tableName)
    {
      m_transformationProviderExt.AddTimeStampTrigger (tableName);
    }
    
    /// <summary>
    /// Add a time stamp trigger to a table
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    protected void AddTimeStampTrigger (string tableName, string columnName)
    {
      m_transformationProviderExt.AddTimeStampTrigger (tableName, columnName);
    }
    
    /// <summary>
    /// Remove the time stamp trigger
    /// </summary>
    /// <param name="tableName"></param>
    protected void RemoveTimeStampTrigger (string tableName)
    {
      m_transformationProviderExt.RemoveTimeStampTrigger (tableName);
    }
    #endregion // Timestamp trigger
    
    #region Virtual column
    /// <summary>
    /// Add a virtual column
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="returnType"></param>
    /// <param name="sql"></param>
    protected void AddVirtualColumn (string tableName, string columnName, string returnType, string sql)
    {
      m_transformationProviderExt.AddVirtualColumn (tableName, columnName, returnType, sql);
    }
    
    /// <summary>
    /// Drop a virtual column
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    protected void DropVirtualColumn (string tableName, string columnName)
    {
      m_transformationProviderExt.DropVirtualColumn (tableName, columnName);
    }
    #endregion // Virtual column
    
    #region Other
    /// <summary>
    /// Build a name from the parameter that is not longer
    /// than 64 characters
    /// </summary>
    /// <param name="table"></param>
    /// <param name="suffix"></param>
    /// <param name="columnNames"></param>
    /// <returns></returns>
    internal string BuildName(string table, string suffix, params string[] columnNames)
    {
      return m_transformationProviderExt.BuildName (table, suffix, columnNames);
    }

    /// <summary>
    /// Convert a table to a log table
    /// </summary>
    /// <param name="tableName"></param>
    protected void ConvertToLogTable (string tableName)
    {
      m_transformationProviderExt.ConvertToLogTable (tableName);
    }
    
    // Return true if the database is running on Windows (based on the PostgreSQL version)
    // "... Compiled by Visual C++ ..."
    internal bool? IsWindows()
    {
      return m_transformationProviderExt.IsWindows ();
    }
    
    /// <summary>
    /// Get the version of postgresql as an int
    /// X.Y.Z => 1000000 * X + 1000 * Y + Z
    /// </summary>
    /// <returns></returns>
    public int GetPostgresqlVersion()
    {
      return m_transformationProviderExt.GetPostgresqlVersion ();
    }
    
    /// <summary>
    /// Some versions of PostgreSQL are bugged. Check it is not one of them
    /// </summary>
    protected void CheckPostgresqlVersion ()
    {
      m_transformationProviderExt.CheckPostgresqlVersion ();
    }

    /// <summary>
    /// Get the type of a column
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public string GetColumnType (string tableName, string columnName)
    {
      string result = "";
      string query = "SELECT data_type FROM information_schema.columns WHERE " +
        "table_name='" + tableName + "' AND column_name='" + columnName + "';";
      using (IDataReader reader = Database.ExecuteQuery (query)) {
        if (reader.Read ()) {
          result = reader.GetString (0);
        }
      }
      return result;
    }
    #endregion // Other
    
    #region Inheritance
    /// <summary>
    /// Make a table inherit from another one
    /// </summary>
    /// <param name="table"></param>
    /// <param name="parent"></param>
    /// <param name="primaryKey"></param>
    protected void Inherit (string table, string parent, string primaryKey)
    {
      m_transformationProviderExt.Inherit (table, parent, primaryKey);
    }
    #endregion // Inheritance
  }
}
