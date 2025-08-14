// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Migrator.Framework;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Extend Migrator.Framework.ITransformationProvider
  /// </summary>
  public class TransformationProviderExt
  {
    readonly ILog log = LogManager.GetLogger (typeof (TransformationProviderExt).FullName);

    /// <summary>
    /// Npgsql configuration key to use to set a command timeout
    /// 
    /// In the documentation, it is referenced Command Timeout
    /// The code property is named CommandTimeout
    /// 
    /// With Npgsql 4, both work
    /// </summary>
    static readonly string NPGSQL_COMMAND_TIMEOUT_KEY_KEY = "Database.Npgsql.CommandTimeout.Key";
    static readonly string NPGSQL_COMMAND_TIMEOUT_KEY_DEFAULT = "Command Timeout";

    #region Members
    Migrator.Framework.Migration m_migration = null;
    ITransformationProvider m_database;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="migration">not null</param>
    public TransformationProviderExt (Migration migration)
    {
      Debug.Assert (null != migration);

      m_migration = migration;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="database">not null</param>
    public TransformationProviderExt (ITransformationProvider database)
    {
      Debug.Assert (null != database);

      m_database = database;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connectionString"></param>
    public TransformationProviderExt (string connectionString)
    {
      // If not set, set Command Timeout to 0 (no command time out)
      var connectionStringWithCommandTimeout = connectionString;
      var npgsqlCommandTimeoutKey = Lemoine.Info.ConfigSet
        .LoadAndGet (NPGSQL_COMMAND_TIMEOUT_KEY_KEY, NPGSQL_COMMAND_TIMEOUT_KEY_DEFAULT);
      if (!connectionStringWithCommandTimeout.Contains (npgsqlCommandTimeoutKey)) {
        connectionStringWithCommandTimeout += $"{npgsqlCommandTimeoutKey}=0;";
      }
      m_database = Migrator.ProviderFactory.Create ("Postgre", connectionStringWithCommandTimeout);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public TransformationProviderExt ()
      : this (Lemoine.Info.GDBConnectionParameters.GDBConnectionString)
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Reference to the main Migrator.ITransformationProvider
    /// </summary>
    public ITransformationProvider Database
    {
      get {
        if (null != m_migration) {
          return m_migration.Database;
        }
        else {
          return m_database;
        }
      }
    }
    #endregion // Getters / Setters

    #region Methods from Migrator.Framework.ITransformationProvider
    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.TableExists(string)"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool TableExists (string name)
    {
      return Database.TableExists (name);
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.ColumnExists(string, string)"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool ColumnExists (string tableName, string columnName)
    {
      return Database.ColumnExists (tableName, columnName);
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.AddTable(string, Column[])"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="columns"></param>
    public void AddTable (string name, params Column[] columns)
    {
      Database.AddTable (name, columns);
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.AddColumn(string, Column)"/>
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void AddColumn (string table, Column column)
    {
      Database.AddColumn (table, column);
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.AddColumn(string, string, System.Data.DbType)"/>
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    /// <param name="type"></param>
    public void AddColumn (string table, string column, System.Data.DbType type)
    {
      Database.AddColumn (table, column, type);
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.RenameColumn(string, string, string)"/>
    /// </summary>
    public void RenameColumn (string tableName, string oldColumnName, string newColumnName)
    {
      Database.RenameColumn (tableName, oldColumnName, newColumnName);
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.ExecuteNonQuery(string)"/>
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="sqlErrorOk">In case of error, the exception is logged in Debug priority instead of Error</param>
    public void ExecuteNonQuery (string sql, bool sqlErrorOk = false)
    {
      try {
        if (log.IsDebugEnabled) {
          log.Debug ($"ExecuteNonQuery: about to execute {sql}");
        }
        Database.ExecuteNonQuery (sql);
      }
      catch (Exception ex) {
        if (!sqlErrorOk) {
          log.Error ($"ExecuteNonQuery: exception running {sql}", ex);
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"ExecuteNonQuery: exception running {sql}", ex);
        }
        throw;
      }
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.GenerateForeignKey(string, string, string, string, ForeignKeyConstraint)"/>
    /// </summary>
    /// <param name="foreignTable"></param>
    /// <param name="foreignColumn"></param>
    /// <param name="primaryTable"></param>
    /// <param name="primaryColumn"></param>
    /// <param name="constraint"></param>
    public void GenerateForeignKey (string foreignTable, string foreignColumn,
      string primaryTable, string primaryColumn,
      Migrator.Framework.ForeignKeyConstraint constraint)
    {
      Database.GenerateForeignKey (foreignTable, foreignColumn,
        primaryTable, primaryColumn,
        constraint);
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.Insert(string, string[], string[])"/>
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columns"></param>
    /// <param name="values"></param>
    public void Insert (string table, string[] columns, string[] values)
    {
      Database.Insert (table, columns, values);
    }

    /// <summary>
    /// <see cref="Migrator.Framework.ITransformationProvider.RemoveTable(string)"/>
    /// </summary>
    /// <param name="table"></param>
    public void RemoveTable (string table)
    {
      if (Database.TableExists (table)) {
        Database.RemoveTable (table);
      }
      else {
        log.WarnFormat ("RemoveTable: {0} did not exist", table);
      }
    }
    #endregion // Methods from Migrator.Framework.ITransformationProvider

    #region New methods
    /// <summary>
    /// Execute a set of queries separated by ';' which returns no results
    /// 
    /// Be careful not to use the semi-colon ';' in your SQL request because it is used as a request separator
    /// and semi-colons in strings are not detected
    /// 
    /// TODO: this method to allow to easily patch a system
    /// It can be removed in the next version
    /// </summary>
    /// <param name="queries"></param>
    public void ExecuteSetOfQueries (string queries)
    {
      ExecuteSetOfQueries (queries, ";");
    }

    /// <summary>
    /// Execute a set of queries separated by ';' which returns no results
    /// 
    /// Be careful not to use the semi-colon ';' in your SQL request because it is used as a request separator
    /// and semi-colons in strings are not detected
    /// </summary>
    /// <param name="queries"></param>
    /// <param name="separator"></param>
    /// <param name="sqlErrorOk">In case of error, the exception is logged in Debug priority instead of Error</param>
    public void ExecuteSetOfQueries (string queries, string separator = ";", bool sqlErrorOk = false)
    {
      if (string.IsNullOrEmpty (separator)) {
        log.Warn ($"ExecuteSetOfQueries: empty separator => execute {queries} directly");
        ExecuteNonQuery (queries, sqlErrorOk: sqlErrorOk);
        return;
      }

      string[] queryList = queries.Split (new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
      string partialRequest = null;
      string escapeString = null;
      bool comment = false;
      var commentRegex = new Regex ("(/\\*)|(\\*/)", RegexOptions.Compiled);
      var escapeRegex = new Regex ("([$].*[$])|(/\\*)|(\\*/)", RegexOptions.Compiled);
      foreach (var query in queryList) {
        if (null != partialRequest) {
          partialRequest += separator + query;
          if ((null != escapeString) && query.Contains (escapeString)) { // End
            if (log.IsDebugEnabled) {
              log.Debug ($"ExecuteSetOfQueries: end of escapement, execute {partialRequest}");
            }
            ExecuteNonQuery (partialRequest, sqlErrorOk: sqlErrorOk);
            (partialRequest, escapeString) = (null, null);
          }
          else if (comment) {
            var matches = commentRegex.Matches (query);
            foreach (Match match in matches) {
              comment = match.Value.Equals ("/*");
            }
            if (!comment) {
              if (log.IsDebugEnabled) {
                log.Debug ($"ExecuteSetOfQueries: end of comment, execute {partialRequest}");
              }
              ExecuteNonQuery (partialRequest, sqlErrorOk: sqlErrorOk);
              partialRequest = null;
            }
          }
        }
        else { // partialRequest == null
          var matches = escapeRegex.Matches (query);
          if (0 < matches.Count) {
            foreach (Match match in matches) {
              if (match.Value.Equals ("/*")) {
                comment = true;
              }
              else if (match.Value.Equals ("*/")) {
                comment = false;
              }
              else if (!comment) {
                if (null != escapeString) {
                  if (match.Value.Equals (escapeString, StringComparison.InvariantCultureIgnoreCase)) {
                    escapeString = null;
                  }
                }
                else {
                  escapeString = match.Value;
                }
              }
            }
            if (comment || (null != escapeString)) {
              partialRequest = query;
              if (log.IsDebugEnabled) {
                log.Debug ($"ExecuteSetOfQueries: comment={comment} or escape={escapeString} found in {query}");
              }
            }
            else {
              ExecuteNonQuery (query, sqlErrorOk: sqlErrorOk);
            }
          }
          else {
            ExecuteNonQuery (query, sqlErrorOk: sqlErrorOk);
          }
        }
      }
      if (null != partialRequest) {
        log.Fatal ($"ExecuteSetOfQueries: no second escapeString {escapeString} found in {partialRequest}");
      }
    }

    /// <summary>
    /// Remove a column (cascade)
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    public void RemoveColumnCascade (string tableName, string columnName)
    {
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0} DROP COLUMN {1} CASCADE
",
        tableName, columnName));
    }

    #region Make columns
    /// <summary>
    /// Convert the type of a column to text
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void MakeColumnText (string table, string column)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE TEXT",
                                               table, column));
    }

    /// <summary>
    /// Make a column case insensitive
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void MakeColumnCaseInsensitive (string table, string column)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE CITEXT",
                                               table, column));
    }

    /// <summary>
    /// Convert a column to a tsrange type
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void MakeColumnTsRange (string table, string column)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE TSRANGE USING '[,)'::tsrange",
                                               table, column));
    }

    /// <summary>
    /// Convert a column to a tsrange type
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void MakeColumnDateRange (string table, string column)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE DATERANGE USING '[,)'::daterange",
                                               table, column));
    }

    /// <summary>
    /// Convert a column to a jsonb type
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void MakeColumnJsonB (string table, string column)
    {
      Database.ExecuteNonQuery (
        string.Format ("ALTER TABLE {0} ALTER COLUMN {1} SET DATA TYPE JSONB USING {1}::text::jsonb",
                       table, column));
    }
    #endregion // Make columns

    #region Remove constraints
    /// <summary>
    /// Remove a constraint given its name,
    /// check if the table is partitioned
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="name">Constraint name</param>
    public void RemoveConstraint (string table, string name)
    {
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (string.Format ("SELECT pgfkpart.drop_unique_constraint ('public', '{0}', '{1}')",
                                                 table, name));
      }
      else {
        Database.ExecuteNonQuery (string.Format ("ALTER TABLE {0} DROP CONSTRAINT IF EXISTS {1};",
                                               table, name));
      }
    }

    /// <summary>
    /// Remove a unique constraint,
    /// check if the table is partitioned
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>"
    public void RemoveUniqueConstraint (string table, params string[] columnNames)
    {
      RemoveConstraint (table, BuildName (table, "unique", columnNames));
    }

    /// <summary>
    /// Remove a non-overlapping constraint,
    /// check if the table is partitioned
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void RemoveNoOverlapConstraint (string table, params string[] columnNames)
    {
      RemoveConstraint (table, BuildName (table, "nooverlap", columnNames));
    }

    /// <summary>
    /// Remove a color constraint
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void RemoveConstraintColor (string table, string column)
    {
      Database.ExecuteNonQuery (string.Format ("ALTER TABLE {0} DROP CONSTRAINT IF EXISTS {1};",
                                             table, BuildName (column, "color")));
    }
    #endregion // Remove constraints

    #region Add constraints
    /// <summary>
    /// Add a unique deferable initially deferred constraint
    /// </summary>
    /// <param name="name"></param>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void AddNamedUniqueConstraint (string name, string table, params string[] columnNames)
    {
      RemoveConstraint (table, name);
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
  ADD CONSTRAINT {2} UNIQUE({1})
  DEFERRABLE INITIALLY DEFERRED;",
                                               table,
                                               string.Join (", ", columnNames),
                                               name));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add a unique deferable initially deferred constraint
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void AddUniqueConstraint (string table, params string[] columnNames)
    {
      AddNamedUniqueConstraint (BuildName (table, "unique", columnNames), table, columnNames);
    }

    /// <summary>
    /// Add a non-overlapping deferable initially deferred constraint
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void AddNoOverlapConstraintV1 (string table, string column)
    {
      CheckPostgresqlVersion ();

      RemoveNoOverlapConstraint (table, column);
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
  ADD CONSTRAINT {2} EXCLUDE USING gist ({1} WITH &&)
  DEFERRABLE INITIALLY DEFERRED;",
                                               table, column,
                                               BuildName (table, "nooverlap", column)));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add a non-overlapping deferable initially deferred constraint
    /// An index is made with columnNames
    /// For each index, rangecolumn won't overlap
    /// </summary>
    /// <param name="table"></param>
    /// <param name="rangecolumn"></param>
    /// <param name="columnNames"></param>
    public void AddNoOverlapConstraint (string table,
                                          string rangecolumn,
                                          params string[] columnNames)
    {
      RemoveNoOverlapConstraint (table);

      // Prepare the index
      string strTmp = "";
      foreach (string columnName in columnNames) {
        strTmp += columnName + " WITH =, ";
      }

      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
  ADD CONSTRAINT {3} EXCLUDE USING gist ({1}{2} WITH &&)
  DEFERRABLE INITIALLY DEFERRED;",
                                               table,
                                               strTmp,
                                               rangecolumn,
                                               BuildName (table, "nooverlap")
                                              ));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
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
    public void AddNoOverlapConstraintCondition (string table,
                                                 string condition,
                                                 string rangecolumn,
                                                 params string[] columnNames)
    {
      RemoveNoOverlapConstraint (table);

      // Prepare the index
      string strTmp = "";
      foreach (string columnName in columnNames) {
        strTmp += columnName + " WITH =, ";
      }

      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
  ADD CONSTRAINT {3} EXCLUDE USING gist ({1}{2} WITH &&)
  WHERE ( {4} )
  DEFERRABLE INITIALLY DEFERRED;",
                                               table,
                                               strTmp,
                                               rangecolumn,
                                               BuildName (table, "nooverlap"),
                                               condition
                                              ));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
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
    public void AddConstraintNameTranslationKey (string table,
                                                   string nameColumn,
                                                   string translationKeyColumn)
    {
      Database.AddCheckConstraint (BuildName (table, "name_translationkey"),
                                   table,
                                   string.Format (@"(({0} IS NOT NULL) OR ({1} IS NOT NULL))",
                                                  nameColumn,
                                                  translationKeyColumn));
    }

    /// <summary>
    /// Add a check constraint so that the column is NULL or a web color
    /// #000000
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void AddConstraintNullColor (string table, string column)
    {
      string constraintName = BuildName (column, "color");
      string constraint =
        string.Format (@"({0} IS NULL)
 OR ({0} SIMILAR TO '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]')",
                       column); // Note: there are some problems using {{6}}
      Database.AddCheckConstraint (constraintName,
                                   table,
                                   constraint);
    }

    /// <summary>
    /// Add a check constraint so that the column is a web color
    /// #000000
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void AddConstraintColor (string table, string column)
    {
      string constraintName = BuildName (column, "color");
      string constraint =
        string.Format (@"{0} SIMILAR TO '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'",
                       column); // Note: there are some problems using {{6}}
      Database.AddCheckConstraint (constraintName, table, constraint);
    }

    /// <summary>
    /// Add a constraint
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void AddConstraintPositive (string table, string column)
    {
      string constraintName = BuildName (column, "positive");
      string constraint =
        string.Format (@"{0} >= 0",
                       column);
      Database.AddCheckConstraint (constraintName, table, constraint);
    }

    /// <summary>
    /// Check a range has a duration strictly positive (not 0s and not empty)
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void AddConstraintRangeStrictlyPositiveDuration (string table, string column)
    {
      string constraintName = string.Format ("{0}_{1}_posduration", table, column);
      Database.AddCheckConstraint (constraintName,
                                   table,
                                   string.Format (@"
NOT isempty({0})
AND (lower_inf({0}) OR upper_inf({0}) OR (upper({0})<>lower({0})))",
                                                  column));
    }

    /// <summary>
    /// Add the constraint a range is not empty
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void AddConstraintRangeNotEmpty (string table, string column)
    {
      string constraintName = string.Format ("{0}_{1}_notempty", table, column);
      Database.AddCheckConstraint (constraintName,
                                   table,
                                   string.Format (@"NOT isempty({0})",
                                                  column));
    }

    /// <summary>
    /// Add a non negative constraint to a column
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void AddNonNegativeConstraint (string table, string column)
    {
      Database.AddCheckConstraint (BuildName (table, "nonnegative", column),
                                   table,
                                   string.Format ("{0} >= 0", column));
    }

    /// <summary>
    /// Add the constraint one of the two given columns must not be null
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column1"></param>
    /// <param name="column2"></param>
    public void AddOneNotNullConstraint (string table, string column1, string column2)
    {
      Database.AddCheckConstraint (BuildName (table, "constraint", column1, column2),
                                   table,
                                   string.Format (@"(({0} IS NOT NULL) OR ({1} IS NOT NULL))",
                                                  column1, column2));

    }

    /// <summary>
    /// Set a NOT NULL property to a column
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void SetNotNull (string table, string column)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL",
                                               table,
                                               column));
    }

    /// <summary>
    /// Drop a NOT NULL property to a column
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void DropNotNull (string table, string column)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} DROP NOT NULL",
                                               table,
                                               column));
    }

    /// <summary>
    /// Drop a a default value
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void DropDefault (string table, string column)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT",
                                               table,
                                               column));
    }
    #endregion // Add constraints

    #region Add / Remove index
    /// <summary>
    /// Add an index on columns
    /// </summary>
    /// <param name="index"></param>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    void AddGenericIndex (string index, string table, params string[] columnNames)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE INDEX {3} ON {1} USING {0} ({2})",
                                               index,
                                               table, string.Join (", ", columnNames),
                                               BuildName (table, "idx", columnNames)));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add an index on a single column with a condition
    /// </summary>
    /// <param name="index">Kind of index</param>
    /// <param name="table"></param>
    /// <param name="column"></param>
    /// <param name="condition"></param>
    void AddGenericIndexCondition (string index, string table, string column, string condition)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE INDEX {4} ON {1} USING {0} ({2}) WHERE {3}",
                                               index,
                                               table, column,
                                               condition,
                                               BuildName (table, "idx", column, "w")));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add an index on columns
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void AddIndex (string table, params string[] columnNames)
    {
      AddGenericIndex ("btree", table, columnNames);
    }

    /// <summary>
    /// Add an index on columns
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void AddGistIndex (string table, params string[] columnNames)
    {
      AddGenericIndex ("gist", table, columnNames);
    }

    /// <summary>
    /// Add a gist index on a single column with a condition
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    /// <param name="condition"></param>
    public void AddGistIndexCondition (string table, string column, string condition)
    {
      AddGenericIndexCondition ("gist", table, column, condition);
    }

    /// <summary>
    /// Add an index on one column
    /// </summary>
    /// <param name="name"></param>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void AddNamedIndex (string name, string table, params string[] columnNames)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE INDEX {2} ON {0} USING btree ({1})",
                                               table, string.Join (", ", columnNames),
                                               BuildName (name, "idx")));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add an index on columns with a condition and a name (without _idx)
    /// </summary>
    /// <param name="name">Name of the index without the _idx suffix</param>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="columnNames"></param>
    public void AddNamedIndexCondition (string name,
                                          string table,
                                          string condition,
                                          params string[] columnNames)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE INDEX {0} ON {1} USING btree ({2}) WHERE {3}",
                                               BuildName (name, "idx"),
                                               table, string.Join (", ", columnNames),
                                               condition));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add an index on a single column with a condition and a name (without _idx)
    /// </summary>
    /// <param name="name">Name of the index without the _idx suffix</param>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="columnNames"></param>
    public void AddNamedGistIndexCondition (string name,
                                              string table,
                                              string condition,
                                              params string[] columnNames)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE INDEX {0} ON {1} USING gist ({2}) WHERE {3}",
                                               BuildName (name, "idx"),
                                               table, string.Join (", ", columnNames),
                                               condition));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add an index on columns with a condition
    /// </summary>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="columnNames"></param>
    public void AddIndexCondition (string table,
                                     string condition,
                                     params string[] columnNames)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE INDEX {3} ON {0} USING btree ({1}) WHERE {2}",
                                               table,
                                               string.Join (", ", columnNames),
                                               condition,
                                               BuildName (table, "idx", columnNames)));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add a unique index on columns (not deferrable!)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void AddNamedUniqueIndex (string name, string table, params string[] columnNames)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE UNIQUE INDEX {2} ON {0} USING btree ({1})",
                                               table, String.Join (", ", columnNames),
                                               BuildName (name, "idx")));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Add a unique index on columns (not deferrable!)
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void AddUniqueIndex (string table, params string[] columnNames)
    {
      AddNamedUniqueIndex (BuildName (table, "idx", columnNames), table, columnNames);
    }

    /// <summary>
    /// Add a unique index on three columns with a condition
    /// (not deferrable!)
    /// </summary>
    /// <param name="table"></param>
    /// <param name="condition"></param>
    /// <param name="columnNames"></param>
    public void AddUniqueIndexCondition (string table,
                                           string condition,
                                           params string[] columnNames)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE UNIQUE INDEX {3} ON {0} USING btree ({1}) WHERE {2}",
                                               table, string.Join (", ", columnNames),
                                               condition,
                                               BuildName (table, "idx", columnNames)));
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.dispatch_index ('public', '{0}')", table));
      }
    }

    /// <summary>
    /// Remove an index, check if the table is partitioned
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="table"></param>
    public void RemoveRealNameIndex (string indexName, string table)
    {
      string name = indexName;
      if (!name.EndsWith ("_idx")) {
        name = BuildName (indexName, "idx");
      }
      if (IsPartitioned (table)) {
        Database.ExecuteNonQuery (
          string.Format (@"SELECT pgfkpart.drop_index ('public', '{0}', '{1}')", table, name));
      }
      else {
        Database.ExecuteNonQuery (string.Format (@"DROP INDEX IF EXISTS {0}", name));
      }
    }

    /// <summary>
    /// Remove an index with an automatically built name, check if the table is partitioned
    /// </summary>
    /// <param name="name"></param>
    /// <param name="table"></param>
    public void RemoveNamedIndex (string name, string table)
    {
      RemoveRealNameIndex (BuildName (name, "idx"), table);
    }

    /// <summary>
    /// Remove an index, check if the table is partitioned
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnNames"></param>
    public void RemoveIndex (string table, params string[] columnNames)
    {
      string indexName = BuildName (table, "idx", columnNames);
      RemoveRealNameIndex (indexName, table);
    }

    /// <summary>
    /// Remove an index with a condition
    /// </summary>
    /// <param name="table"></param>
    /// <param name="column"></param>
    public void RemoveIndexCondition (string table, string column)
    {
      RemoveIndex (table, column, "w");
    }
    #endregion // Add / Remove index

    #region Sequences
    /// <summary>
    /// Reset the sequence of an ID
    /// </summary>
    /// <param name="table"></param>
    /// <param name="idColumn"></param>
    public void ResetSequence (string table, string idColumn)
    {
      Database.ExecuteNonQuery (string.Format (@"
SELECT SETVAL('{0}_{1}_seq', (SELECT MAX({1}) FROM {0}) + 1);",
                                               table, idColumn));
    }

    /// <summary>
    /// Set the sequence of an ID to a given value
    /// </summary>
    /// <param name="table"></param>
    /// <param name="idColumn"></param>
    /// <param name="sequenceValue"></param>
    public void SetSequence (string table, string idColumn, int sequenceValue)
    {
      Database.ExecuteNonQuery (string.Format (@"
SELECT SETVAL('{0}_{1}_seq', {2});",
                                               table, idColumn,
                                               sequenceValue));
    }

    /// <summary>
    /// Set a minimum value to a sequence of an ID considering the existing values
    /// </summary>
    /// <param name="table"></param>
    /// <param name="idColumn"></param>
    /// <param name="minSequenceValue"></param>
    public void SetMinSequence (string table, string idColumn, int minSequenceValue)
    {
      Database.ExecuteNonQuery (string.Format (@"
SELECT SETVAL('{0}_{1}_seq', (SELECT MAX({1}) FROM (SELECT {1} + 1 AS {1} FROM {0} UNION SELECT {2} AS {1}) a));",
                                               table, idColumn,
                                               minSequenceValue));
    }

    /// <summary>
    /// Associate a column to an existing sequence
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="sequenceName"></param>
    public void SetSequence (string tableName, string columnName, string sequenceName)
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DEFAULT nextval('{2}'::regclass)",
                                               tableName,
                                               columnName,
                                               sequenceName));
    }

    /// <summary>
    /// Create a sequence
    /// </summary>
    /// <param name="sequenceName"></param>
    public void AddSequence (string sequenceName)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE SEQUENCE {0}
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 1
  CACHE 1;",
                                               sequenceName));
    }

    /// <summary>
    /// Remove a sequence
    /// </summary>
    /// <param name="sequenceName"></param>
    public void RemoveSequence (string sequenceName)
    {
      Database.ExecuteNonQuery (string.Format (@"DROP SEQUENCE IF EXISTS {0} CASCADE", sequenceName));
    }
    #endregion // Sequences

    #region Modification table
    /// <summary>
    /// Set a table as a modification
    /// </summary>
    /// <param name="table"></param>
    public void SetModificationTable (string table)
    {
      Database.ExecuteNonQuery ($"""
        CREATE RULE {table}_delete AS
        ON DELETE TO {table}
        DO ALSO DELETE FROM modification
          WHERE modificationid = OLD.modificationid;
        """);
    }

    /// <summary>
    /// Set a table as a machinemodification
    /// </summary>
    /// <param name="table"></param>
    public void SetMachineModificationTable (string table)
    {
      Database.ExecuteNonQuery ($"""
        CREATE RULE {table}_delete AS
        ON DELETE TO {table}
        DO ALSO DELETE FROM machinemodification
        WHERE modificationid = OLD.modificationid;
        """);
    }

    /// <summary>
    /// Set a table as a globalmodification
    /// </summary>
    /// <param name="table"></param>
    public void SetGlobalModificationTable (string table)
    {
      Database.ExecuteNonQuery ($"""
        CREATE RULE {table}_delete AS
        ON DELETE TO {table}
        DO ALSO DELETE FROM globalmodification
        WHERE modificationid = OLD.modificationid;
        """);
    }

    /// <summary>
    /// Remove a modification table
    /// </summary>
    /// <param name="persistentClassName">Camel case persistent class name (not lower case table name)</param>
    public void RemoveModificationTable (string persistentClassName)
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM modification
WHERE modificationreferencedtable='{0}'",
                                               persistentClassName));
      Database.RemoveTable (persistentClassName);
    }
    #endregion // Modification table

    #region Partitioning
    /// <summary>
    /// Is the specified table partitioned ?
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public bool IsPartitioned (string tableName)
    {
      try {
        using (System.Data.IDataReader reader = Database.ExecuteQuery (@"SELECT schema_name
FROM information_schema.schemata
WHERE schema_name = 'pgfkpart'")) {
          if (!reader.Read ()) {
            return false;
          }
        }

        object result = Database.ExecuteScalar ($@"SELECT Count(*)
from pgfkpart.partition
where table_schema='public' and table_name='{tableName}'");
        return result.Equals ((Int64)1);

      }
      catch (Exception) {
        // should not happen (connection lost)
        return false;
      }
    }

    /// <summary>
    /// Partition a table
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="tableName"></param>
    /// <param name="partitioningTableName"></param>
    /// <returns>success</returns>
    public bool PartitionTable (string schemaName, string tableName, string partitioningTableName)
    {
      try {
        using (System.Data.IDataReader reader = Database.ExecuteQuery (@"SELECT schema_name
FROM information_schema.schemata
WHERE schema_name = 'pgfkpart'")) {
          if (!reader.Read ()) {
            log.Warn ("PartitionTable: pgfkpart is not installed");
            return false;
          }
        }

        string directory = "";
        bool? isWindows = IsWindows ();
        if (isWindows.HasValue) {
          if (isWindows.Value) {
            directory = ", 'C:\\tmp\\pgfkpart'";
          }
        }
        else {
          log.Warn ("PartitionTable: isWindows is unknown");
        }
        Database.ExecuteNonQuery ("SELECT pgfkpart.partition_with_fk ('" + schemaName + "', '" +
                                 tableName + "', 'public', '" + partitioningTableName + "', FALSE" +
                                 directory + ")");

        return true;
      }
      catch (Exception ex) {
        log.Error ($"PartitionTable: failed with", ex);
        throw;
      }
    }

    /// <summary>
    /// Partition a table
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="partitioningTableName"></param>
    /// <returns>success</returns>
    public bool PartitionTable (string tableName, string partitioningTableName)
    {
      return PartitionTable ("public", tableName, partitioningTableName);
    }

    /// <summary>
    /// Unpartition a table
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns>success</returns>
    public bool UnpartitionTable (string tableName)
    {
      try {
        using (System.Data.IDataReader reader = Database.ExecuteQuery (@"SELECT schema_name
FROM information_schema.schemata
WHERE schema_name = 'pgfkpart'")) {
          if (!reader.Read ()) {
            return false;
          }
        }

        bool? isWindows = IsWindows ();
        if (isWindows != null) {
          String directory = (bool)isWindows ? ", 'C:\\tmp\\pgfkpart'" : "";
          Database.ExecuteNonQuery ("SELECT pgfkpart.unpartition_with_fk ('public', '" +
                                   tableName + "'" + directory + ")");
        }
        else {
          return false;
        }

        return true;
      }
      catch (Exception) {
        // should not happen (connection lost)
        return false;
      }
    }
    #endregion // Partitioning

    #region Timestamp trigger
    /// <summary>
    /// Add a time stamp trigger to a table
    /// </summary>
    /// <param name="tableName"></param>
    public void AddTimeStampTrigger (string tableName)
    {
      AddTimeStampTrigger (tableName, tableName + "timestamp");
    }

    /// <summary>
    /// Add a time stamp trigger to a table
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    public void AddTimeStampTrigger (string tableName, string columnName)
    {
      Database.ExecuteNonQuery (string.Format (@"CREATE OR REPLACE FUNCTION {0}_timestamp_update () RETURNS TRIGGER AS $$
BEGIN
  NEW.{1} := CURRENT_TIMESTAMP AT TIME ZONE 'GMT';
  RETURN NEW;
END
$$ LANGUAGE plpgsql",
                                               tableName, columnName));
      Database.ExecuteNonQuery (string.Format (@"CREATE TRIGGER {0}_insert_update BEFORE INSERT OR UPDATE
ON {0}
FOR EACH ROW
EXECUTE PROCEDURE {0}_timestamp_update ();",
                                               tableName));
    }

    /// <summary>
    /// Remove the time stamp trigger
    /// </summary>
    /// <param name="tableName"></param>
    public void RemoveTimeStampTrigger (string tableName)
    {
      Database.ExecuteNonQuery (string.Format ("DROP FUNCTION IF EXISTS {0}_timestamp_update () CASCADE",
                                               tableName));
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
    public void AddVirtualColumn (string tableName, string columnName, string returnType, string sql)
    {
      Database.ExecuteNonQuery (string.Format (@"
CREATE OR REPLACE FUNCTION public.{1}({0})
  RETURNS {2} AS
$BODY${3}$BODY$
  LANGUAGE sql IMMUTABLE
  COST 100;
",
                                               tableName, columnName, returnType, sql));
    }

    /// <summary>
    /// Drop a virtual column
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    public void DropVirtualColumn (string tableName, string columnName)
    {
      Database.ExecuteNonQuery (string.Format (@"
DROP FUNCTION public.{1}({0});
",
                                               tableName, columnName));
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
    public string BuildName (string table, string suffix, params string[] columnNames)
    {
      Debug.Assert (suffix.Length < 62);

      // Base name
      string name = table;
      if (columnNames.Length > 0) {
        string columns = string.Join ("_", columnNames);
        name = (columnNames.Length < 5) ?
          string.Format ("{0}_{1}", table, columns) :
          string.Format ("{0}_{1:X8}", table, columns.GetHashCode ());
      }

      // Append the suffix
      int maxLength = 62 - suffix.Length;
      if (name.Length > maxLength) {
        name = name.Remove (maxLength);
      }

      name += "_" + suffix;

      return name.ToLowerInvariant ();
    }

    /// <summary>
    /// Convert a table to a log table
    /// </summary>
    /// <param name="tableName"></param>
    public void ConvertToLogTable (string tableName)
    {
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN datetime
SET DEFAULT now() AT TIME ZONE 'UTC';",
                                               tableName));
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN logid
SET DEFAULT nextval('log_logid_seq'::regclass)",
                                               tableName));
      Database.ExecuteNonQuery (string.Format (@"
CREATE INDEX {0}_datetime_idx
ON {0} (datetime);",
                                               tableName));
    }

    /// <summary>
    /// Return true if the database is running on Windows (based on the PostgreSQL version)
    /// "... Compiled by Visual C++ ..."
    /// </summary>
    /// <returns></returns>
    public bool? IsWindows ()
    {
      bool? result = null;

      try {
        using (System.Data.IDataReader reader = Database.ExecuteQuery ("SELECT version();")) {
          if (reader.Read ()) {
            result = reader.GetString (0).Contains ("Visual");
          }
        }
      }
      catch (Exception) {
        // should not happen (connection lost)
      }

      return result;
    }

    /// <summary>
    /// Get the version of postgresql as an int
    /// <item>X.Y.Z => 1000000 * X + 1000 * Y + Z</item>
    /// <item>X.Y => 1000000 * X + 1000 * Y</item>
    /// </summary>
    /// <returns></returns>
    public int GetPostgresqlVersion ()
    {
      int result = 0;

      using (System.Data.IDataReader reader = Database.ExecuteQuery ("SELECT version();")) {
        if (reader.Read ()) {
          string[] strTmp = reader.GetString (0).Split (' ', ',')[1].Split ('.');
          Debug.Assert (2 <= strTmp.Length); // Length == 2 for version >= 10, else 3
          result = 1000000 * Int32.Parse (strTmp[0]) +
            1000 * Int32.Parse (strTmp[1]);
          if (3 <= strTmp.Length) {
            result += Int32.Parse (strTmp[2]);
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Some versions of PostgreSQL are bugged. Check it is not one of them
    /// </summary>
    public void CheckPostgresqlVersion ()
    {
      // Note: on versions < 9.3.7 and < 9.4.2 there is a bug
      //       that may drive to some problems with the nooverlap constraint
      // http://git.postgresql.org/gitweb/?p=postgresql.git;a=commit;h=20781765f77c1fb6465aba97d211636ce92e7a0e
      // If such a version is detected, raise an exception
      int version = GetPostgresqlVersion ();
      if (((9003000 <= version) && (version < 9003007))
          || ((9004000 <= version) && (version < 9004002))) {
        log.FatalFormat ("There is a bug in PostgreSQL version {0} " +
                         "=> please upgrade it first to >= 9.3.7 or >= 9.4.2 or >= 9.5.0");
        throw new Exception ("Bug in PostgreSQL. Please upgrade it.");
      }
    }
    #endregion // Other

    #region Inheritance
    /// <summary>
    /// Make a table inherit from another one
    /// </summary>
    /// <param name="table"></param>
    /// <param name="parent"></param>
    /// <param name="primaryKey"></param>
    public void Inherit (string table, string parent, string primaryKey)
    {
      Database.ExecuteNonQuery (string.Format (@"
CREATE TABLE {0}
(
  CONSTRAINT {0}_pkey PRIMARY KEY ({2})
)
INHERITS ({1})
",
                                              table, parent, primaryKey));
    }
    #endregion // Inheritance
    #endregion // Methods
  }
}
