(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open Lemoine.Core.Log
open Lemoine.Extensions.Interfaces
open Lemoine.Extensions.Plugin
open Lemoine.GDBMigration
open Lemoine.Model
open Migrator.Framework
open System.Data
open Pulse.Extensions.Plugin

type Plugin() =
  inherit PluginNoConfig()
  let mutable database = null
  
  let log = LogManager.GetLogger "Lemoine.Plugin.MachiningProcessMilestones.Plugin"

  let tableName = "mpmilestone"

  let raisePluginVersionNotSupported x =
    let message = sprintf "The plugin version % is not supported" x in
    begin
      log.Error message;
      raise (invalidOp message)
    end

  let createTable (database: TransformationProviderExt) =
    begin
      database.AddTable (tableName, 
        new Column (tableName + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
        new Column (tableName + "version", DbType.Int32, ColumnProperty.NotNull, 1),
        new Column ("machineid", DbType.Int32, ColumnProperty.NotNull),
        new Column (tableName + "datetime", DbType.DateTime, ColumnProperty.NotNull),
        new Column (tableName + "message", DbType.String, ColumnProperty.NotNull),
        new Column (tableName + "creationdatetime", DbType.DateTime, ColumnProperty.NotNull)
      );
      database.MakeColumnText (tableName, tableName + "message");
      database.ExecuteNonQuery (sprintf """
ALTER TABLE %s
ALTER COLUMN %screationdatetime 
SET DEFAULT now() AT TIME ZONE 'UTC';""" tableName tableName);
      database.GenerateForeignKey (tableName, ColumnName.MACHINE_ID,
        TableName.MACHINE, ColumnName.MACHINE_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
    end

  let removeTable (database: TransformationProviderExt) =
    database.RemoveTable tableName

  let installSpecificVersion database = function
  | 1 -> createTable database
  | x -> raisePluginVersionNotSupported x

  member private this.Database = begin
    match database with
    | null -> database <- new TransformationProviderExt()
    | d -> ();
    database
    end

  override this.Name = "Machining Process Milestones"
  override this.Description = "Web services for the Machining Process Milestones project"
  override this.Version = 1
  override this.Uninstall () = removeTable this.Database

  override this.InstallVersion v =
    match v with
    | 0 -> ()
    | x -> installSpecificVersion this.Database x; this.InstallVersion (x-1)

  interface IPluginDll with
     override this.Name = this.Name
     override this.Description = this.Description
     override this.Version = this.Version
     override this.Uninstall () = this.Uninstall ()

  interface IFlaggedPlugin with
     override this.Flags = PluginFlag.Config |||PluginFlag.Web |||PluginFlag.NHibernateExtension
