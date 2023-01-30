// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 003: Managing machining times project
  /// 
  /// 1. Add three new columns in sfkoperation:
  ///    - toolminlength
  ///    - progfeedrate
  ///    - progspindlespeed
  /// 
  /// 2. Add four new columns in sfkcamsystem:
  ///    - processhourskeyword
  ///    - opdescriptionkeyword
  ///    - toolnamekeyword
  ///    - toolminlengthkeyword
  /// </summary>
  [Migration(03)]
  public class ManagingMachiningTimes: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ManagingMachiningTimes).FullName);

    static readonly int DEFAULT_SIZE = 8;
    
    static readonly string SFKOPERATION_TABLE = "sfkoperation";
    static readonly string PROCESSID_COLUMN = "opprocessid";
    static readonly string TOOLMINLENGTH_COLUMN = "toolminlength";
    static readonly string PROGFEEDRATE_COLUMN = "progfeedrate";
    static readonly string PROGSPINDLESPEED_COLUMN = "progspindlespeed";
    
    static readonly string SFKCAMSYSTEM_TABLE = "sfkcamsystem";
    static readonly string PROCESSORDERKEYWORD_COLUMN = "camprocessorderkeyword";
    static readonly string PROCESSHOURSKEYWORD_COLUMN = "processhourskeyword";
    static readonly string OPDESCRIPTIONKEYWORD_COLUMN = "opdescriptionkeyword";
    static readonly string TOOLNAMEKEYWORD_COLUMN = "toolnamekeyword";
    static readonly string TOOLMINLENGTHKEYWORD_COLUMN = "toolminlengthkeyword";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Check the latest columns have already been set
      if (Database.TableExists (SFKOPERATION_TABLE)
          && !Database.ColumnExists (SFKOPERATION_TABLE,
                                     PROCESSID_COLUMN)) {
        log.ErrorFormat ("Up: column {0} does not exist " +
                         "in table {1}, abort",
                         PROCESSID_COLUMN,
                         SFKOPERATION_TABLE);
        throw new Exception (String.Format ("Column {0} does not exist " +
                                            "in table {1}, please run gdb_manager first",
                                            PROCESSID_COLUMN,
                                            SFKOPERATION_TABLE));
      }
      if (Database.TableExists (SFKCAMSYSTEM_TABLE)
          && !Database.ColumnExists (SFKCAMSYSTEM_TABLE,
                                     PROCESSORDERKEYWORD_COLUMN)) {
        log.ErrorFormat ("Up: column {0} does not exist " +
                         "in table {1}, abort",
                         PROCESSORDERKEYWORD_COLUMN,
                         SFKCAMSYSTEM_TABLE);
        throw new Exception (String.Format ("Column {0} does not exist " +
                                            "in table {1}",
                                            PROCESSORDERKEYWORD_COLUMN,
                                            SFKCAMSYSTEM_TABLE));
      }
      
      // Add the new columns in sfkoperation
      if (Database.TableExists (SFKOPERATION_TABLE)) {
        if (!Database.ColumnExists (SFKOPERATION_TABLE,
                                    TOOLMINLENGTH_COLUMN)) {
          Database.AddColumn (SFKOPERATION_TABLE,
                              TOOLMINLENGTH_COLUMN, DbType.Double,
                              DEFAULT_SIZE, ColumnProperty.NotNull, -1.0);
        }
        if (!Database.ColumnExists (SFKOPERATION_TABLE,
                                    PROGFEEDRATE_COLUMN)) {
          Database.AddColumn (SFKOPERATION_TABLE,
                              PROGFEEDRATE_COLUMN, DbType.Double,
                              DEFAULT_SIZE, ColumnProperty.NotNull, -1.0);
          
        }
        if (!Database.ColumnExists (SFKOPERATION_TABLE,
                                    PROGSPINDLESPEED_COLUMN)) {
          Database.AddColumn (SFKOPERATION_TABLE,
                              PROGSPINDLESPEED_COLUMN, DbType.Double,
                              DEFAULT_SIZE, ColumnProperty.NotNull, -1.0);
        }
      }
      
      // Add the new columns in sfkcamsystem
      if (Database.TableExists (SFKCAMSYSTEM_TABLE)) {
        if (!Database.ColumnExists (SFKCAMSYSTEM_TABLE,
                                    PROCESSHOURSKEYWORD_COLUMN)) {
          Database.AddColumn (SFKCAMSYSTEM_TABLE,
                              PROCESSHOURSKEYWORD_COLUMN, DbType.String);
        }
        if (!Database.ColumnExists (SFKCAMSYSTEM_TABLE,
                                    OPDESCRIPTIONKEYWORD_COLUMN)) {
          Database.AddColumn (SFKCAMSYSTEM_TABLE,
                              OPDESCRIPTIONKEYWORD_COLUMN, DbType.String);
        }
        if (!Database.ColumnExists (SFKCAMSYSTEM_TABLE,
                                    TOOLNAMEKEYWORD_COLUMN)) {
          Database.AddColumn (SFKCAMSYSTEM_TABLE,
                              TOOLNAMEKEYWORD_COLUMN, DbType.String);
        }
        if (!Database.ColumnExists (SFKCAMSYSTEM_TABLE,
                                    TOOLMINLENGTHKEYWORD_COLUMN)) {
          Database.AddColumn (SFKCAMSYSTEM_TABLE,
                              TOOLMINLENGTHKEYWORD_COLUMN, DbType.String);
        }
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Remove the new tables
      if (Database.TableExists (SFKOPERATION_TABLE)) {
        Database.RemoveColumn (SFKOPERATION_TABLE, PROGSPINDLESPEED_COLUMN);
        Database.RemoveColumn (SFKOPERATION_TABLE, PROGFEEDRATE_COLUMN);
        Database.RemoveColumn (SFKOPERATION_TABLE, TOOLMINLENGTH_COLUMN);
      }
      if (Database.TableExists (SFKCAMSYSTEM_TABLE)) {
        Database.RemoveColumn (SFKCAMSYSTEM_TABLE, PROCESSHOURSKEYWORD_COLUMN);
        Database.RemoveColumn (SFKCAMSYSTEM_TABLE, OPDESCRIPTIONKEYWORD_COLUMN);
        Database.RemoveColumn (SFKCAMSYSTEM_TABLE, TOOLNAMEKEYWORD_COLUMN);
        Database.RemoveColumn (SFKCAMSYSTEM_TABLE, TOOLMINLENGTHKEYWORD_COLUMN);
      }
    }
  }
}
