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
  /// Migration 208: Add MachineFilterItem Table
  /// </summary>
  [Migration(208)]
  public class AddMachineFilterItemTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineFilterItemTable).FullName);
    
    static readonly string MACHINE_FILTER_CELL = "machinefiltercell";
    static readonly string MACHINE_FILTER_COMPANY = "machinefiltercompany";
    static readonly string MACHINE_FILTER_DEPARTMENT = "machinefilterdepartment";
    static readonly string MACHINE_FILTER_CATEGORY = "machinefiltermachinecategory";
    static readonly string MACHINE_FILTER_SUBCATEGORY = "machinefiltermachinesubcategory";
    static readonly string MACHINE_FILTER_MACHINE = "machinefiltermachine";
    static readonly string MACHINE_FILTER_ORDER_COLUMN = "machinefilteritemorder";
    static readonly string MACHINE_FILTER_RULE_COLUM = "machinefilteritemrule";
    static readonly string MACHINE_FILTER_ID_COLUMN = TableName.MACHINE_FILTER + "id";
    static readonly string MACHINE_FILTER_ITEM_ID_COLUMN = TableName.MACHINE_FILTER_ITEM + "id";
    static readonly String[] listOfTable = new string[]{ "machinefiltercell","machinefiltercompany","machinefilterdepartment","machinefiltermachine","machinefiltermachinecategory","machinefiltermachinesubcategory"};
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      this.CreateTable();
      this.SaveExistingData();
      this.UpdateAllMachineFilterTable();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      this.RestoreAllMachineFilterTable();
      this.RestoreOldData();
      this.RemoveTable();
    }
    
    #region UP
    
    /// <summary>
    /// Create New MachineFilterItem Table
    /// </summary>
    protected void CreateTable()
    {
      Database.AddTable (TableName.MACHINE_FILTER_ITEM,
                         new Column (TableName.MACHINE_FILTER_ITEM + "id", DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (MACHINE_FILTER_ID_COLUMN, DbType.Int32, ColumnProperty.Null), //Null for deleting/updating reason.
                         new Column (MACHINE_FILTER_ORDER_COLUMN, DbType.Int32, ColumnProperty.Null), //Null for deleting/updating reason.
                         new Column (MACHINE_FILTER_RULE_COLUM, DbType.Int32, ColumnProperty.Null) //Null for deleting/updating reason.
                        );
      Database.GenerateForeignKey (TableName.MACHINE_FILTER_ITEM, MACHINE_FILTER_ID_COLUMN,TableName.MACHINE_FILTER, MACHINE_FILTER_ID_COLUMN);
    }
    
    /// <summary>
    /// Import Existing Data into MachineFilterItem Table
    /// </summary>
    protected void SaveExistingData()
    {
      if(Database.TableExists(TableName.MACHINE_FILTER_ITEM)){

        String baseSqlQuery = "INSERT INTO machinefilteritem ("+MACHINE_FILTER_ITEM_ID_COLUMN+",machinefilterid, "+MACHINE_FILTER_ORDER_COLUMN+","+MACHINE_FILTER_RULE_COLUM+")" +
          " SELECT {0}."+MACHINE_FILTER_ITEM_ID_COLUMN+", machinefilter.machinefilterid, {0}."+MACHINE_FILTER_ORDER_COLUMN+", {0}."+MACHINE_FILTER_RULE_COLUM+" FROM machinefilter, {0} WHERE machinefilter.machinefilterid = {0}.machinefilterid;";
        
        //Fill MachineFilterItem Table with MF.Id & MFI.Order
        foreach(String tableName in listOfTable){
          String sqlQuery = String.Format(baseSqlQuery,tableName);
          Database.ExecuteNonQuery(@sqlQuery);
        }
        
        //Make link between MFI and MFIX
        Database.AddForeignKey("fk_machinefiltercell_machinefilteritem", MACHINE_FILTER_CELL, MACHINE_FILTER_ITEM_ID_COLUMN, TableName.MACHINE_FILTER_ITEM, MACHINE_FILTER_ITEM_ID_COLUMN);
        Database.AddForeignKey("fk_machinefiltercompany_machinefilteritem", MACHINE_FILTER_COMPANY, MACHINE_FILTER_ITEM_ID_COLUMN, TableName.MACHINE_FILTER_ITEM, MACHINE_FILTER_ITEM_ID_COLUMN);
        Database.AddForeignKey("fk_machinefilterdepartment_machinefilteritem", MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ITEM_ID_COLUMN, TableName.MACHINE_FILTER_ITEM, MACHINE_FILTER_ITEM_ID_COLUMN);
        Database.AddForeignKey("fk_machinefiltercategory_machinefilteritem", MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ITEM_ID_COLUMN, TableName.MACHINE_FILTER_ITEM, MACHINE_FILTER_ITEM_ID_COLUMN);
        Database.AddForeignKey("fk_machinefiltersubcategory_machinefilteritem", MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ITEM_ID_COLUMN, TableName.MACHINE_FILTER_ITEM, MACHINE_FILTER_ITEM_ID_COLUMN);
        Database.AddForeignKey("fk_machinefiltermachine_machinefilteritem", MACHINE_FILTER_MACHINE, MACHINE_FILTER_ITEM_ID_COLUMN, TableName.MACHINE_FILTER_ITEM, MACHINE_FILTER_ITEM_ID_COLUMN);
      }
    }
    
    /// <summary>
    /// Remove old Data
    /// </summary>
    protected void UpdateAllMachineFilterTable()
    {
      if(Database.ColumnExists(MACHINE_FILTER_CELL, MACHINE_FILTER_ID_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_CELL, MACHINE_FILTER_ID_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_COMPANY, MACHINE_FILTER_ID_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_COMPANY, MACHINE_FILTER_ID_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ID_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ID_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ID_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ID_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ID_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ID_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_MACHINE, MACHINE_FILTER_ID_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_MACHINE, MACHINE_FILTER_ID_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_CELL, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_CELL,MACHINE_FILTER_ORDER_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_COMPANY, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_COMPANY,MACHINE_FILTER_ORDER_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_DEPARTMENT,MACHINE_FILTER_ORDER_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_CATEGORY,MACHINE_FILTER_ORDER_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_SUBCATEGORY,MACHINE_FILTER_ORDER_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_MACHINE, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.RemoveColumn(MACHINE_FILTER_MACHINE,MACHINE_FILTER_ORDER_COLUMN);
      }

      if (Database.ColumnExists(MACHINE_FILTER_CELL, MACHINE_FILTER_RULE_COLUM)) {
        Database.RemoveColumn(MACHINE_FILTER_CELL,MACHINE_FILTER_RULE_COLUM);
      }

      if (Database.ColumnExists(MACHINE_FILTER_COMPANY, MACHINE_FILTER_RULE_COLUM)) {
        Database.RemoveColumn(MACHINE_FILTER_COMPANY,MACHINE_FILTER_RULE_COLUM);
      }

      if (Database.ColumnExists(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_RULE_COLUM)) {
        Database.RemoveColumn(MACHINE_FILTER_DEPARTMENT,MACHINE_FILTER_RULE_COLUM);
      }

      if (Database.ColumnExists(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_RULE_COLUM)) {
        Database.RemoveColumn(MACHINE_FILTER_CATEGORY,MACHINE_FILTER_RULE_COLUM);
      }

      if (Database.ColumnExists(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_RULE_COLUM)) {
        Database.RemoveColumn(MACHINE_FILTER_SUBCATEGORY,MACHINE_FILTER_RULE_COLUM);
      }

      if (Database.ColumnExists(MACHINE_FILTER_MACHINE, MACHINE_FILTER_RULE_COLUM)) {
        Database.RemoveColumn(MACHINE_FILTER_MACHINE,MACHINE_FILTER_RULE_COLUM);
      }
    }
    
    #endregion
    
    #region DOWN
    
    /// <summary>
    /// Restore Table &amp; Data Structure
    /// </summary>
    protected void RestoreAllMachineFilterTable()
    {
      if(!Database.ColumnExists(MACHINE_FILTER_CELL, MACHINE_FILTER_ID_COLUMN)){
        Database.AddColumn(MACHINE_FILTER_CELL, MACHINE_FILTER_ID_COLUMN, DbType.Int32);
        Database.GenerateForeignKey(MACHINE_FILTER_CELL, MACHINE_FILTER_ID_COLUMN, TableName.MACHINE_FILTER, MACHINE_FILTER_ID_COLUMN);
      }
      if(!Database.ColumnExists(MACHINE_FILTER_COMPANY, MACHINE_FILTER_ID_COLUMN)){
        Database.AddColumn(MACHINE_FILTER_COMPANY, MACHINE_FILTER_ID_COLUMN, DbType.Int32);
        Database.GenerateForeignKey(MACHINE_FILTER_COMPANY, MACHINE_FILTER_ID_COLUMN, TableName.MACHINE_FILTER, MACHINE_FILTER_ID_COLUMN);
      }
      if(!Database.ColumnExists(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ID_COLUMN)){
        Database.AddColumn(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ID_COLUMN, DbType.Int32);
        Database.GenerateForeignKey(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ID_COLUMN, TableName.MACHINE_FILTER, MACHINE_FILTER_ID_COLUMN);
      }
      if(!Database.ColumnExists(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ID_COLUMN)){
        Database.AddColumn(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ID_COLUMN, DbType.Int32);
        Database.GenerateForeignKey(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ID_COLUMN, TableName.MACHINE_FILTER, MACHINE_FILTER_ID_COLUMN);
      }
      if(!Database.ColumnExists(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ID_COLUMN)){
        Database.AddColumn(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ID_COLUMN, DbType.Int32);
        Database.GenerateForeignKey(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ID_COLUMN, TableName.MACHINE_FILTER, MACHINE_FILTER_ID_COLUMN);
      }
      if(!Database.ColumnExists(MACHINE_FILTER_MACHINE, MACHINE_FILTER_ID_COLUMN)){
        Database.AddColumn(MACHINE_FILTER_MACHINE, MACHINE_FILTER_ID_COLUMN, DbType.Int32);
        Database.GenerateForeignKey(MACHINE_FILTER_MACHINE, MACHINE_FILTER_ID_COLUMN, TableName.MACHINE_FILTER, MACHINE_FILTER_ID_COLUMN);
      }
      
      if(!Database.ColumnExists(MACHINE_FILTER_CELL, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.AddColumn(MACHINE_FILTER_CELL,MACHINE_FILTER_ORDER_COLUMN, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_COMPANY, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.AddColumn(MACHINE_FILTER_COMPANY,MACHINE_FILTER_ORDER_COLUMN, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.AddColumn(MACHINE_FILTER_DEPARTMENT,MACHINE_FILTER_ORDER_COLUMN, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.AddColumn(MACHINE_FILTER_CATEGORY,MACHINE_FILTER_ORDER_COLUMN, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.AddColumn(MACHINE_FILTER_SUBCATEGORY,MACHINE_FILTER_ORDER_COLUMN, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_MACHINE, MACHINE_FILTER_ORDER_COLUMN)) {
        Database.AddColumn(MACHINE_FILTER_MACHINE,MACHINE_FILTER_ORDER_COLUMN, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_CELL, MACHINE_FILTER_RULE_COLUM)) {
        Database.AddColumn(MACHINE_FILTER_CELL,MACHINE_FILTER_RULE_COLUM, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_COMPANY, MACHINE_FILTER_RULE_COLUM)) {
        Database.AddColumn(MACHINE_FILTER_COMPANY,MACHINE_FILTER_RULE_COLUM, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_RULE_COLUM)) {
        Database.AddColumn(MACHINE_FILTER_DEPARTMENT,MACHINE_FILTER_RULE_COLUM, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_RULE_COLUM)) {
        Database.AddColumn(MACHINE_FILTER_CATEGORY,MACHINE_FILTER_RULE_COLUM, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_RULE_COLUM)) {
        Database.AddColumn(MACHINE_FILTER_SUBCATEGORY,MACHINE_FILTER_RULE_COLUM, DbType.Int32);
      }

      if (!Database.ColumnExists(MACHINE_FILTER_MACHINE, MACHINE_FILTER_RULE_COLUM)) {
        Database.AddColumn(MACHINE_FILTER_MACHINE,MACHINE_FILTER_RULE_COLUM, DbType.Int32);
      }
    }
    
    /// <summary>
    /// Reload old Data into/as old Structure
    /// </summary>
    protected void RestoreOldData()
    {
      //Insert Old Data
      if(Database.ColumnExists(MACHINE_FILTER_CELL, MACHINE_FILTER_ID_COLUMN) &&
         Database.ColumnExists(MACHINE_FILTER_COMPANY, MACHINE_FILTER_ID_COLUMN) &&
         Database.ColumnExists(MACHINE_FILTER_DEPARTMENT, MACHINE_FILTER_ID_COLUMN) &&
         Database.ColumnExists(MACHINE_FILTER_CATEGORY, MACHINE_FILTER_ID_COLUMN) &&
         Database.ColumnExists(MACHINE_FILTER_SUBCATEGORY, MACHINE_FILTER_ID_COLUMN) &&
         Database.ColumnExists(MACHINE_FILTER_MACHINE, MACHINE_FILTER_ID_COLUMN) &&
         Database.TableExists(TableName.MACHINE_FILTER_ITEM)){
        
        String baseUpdateSqlQuery = "UPDATE {0} SET "+MACHINE_FILTER_ID_COLUMN+"=subquery.machinefilterid, "+MACHINE_FILTER_ORDER_COLUMN+"=subquery.machinefilteritemorder, "+MACHINE_FILTER_RULE_COLUM+"=subquery.machinefilteritemrule FROM" +
          @" (SELECT machinefilteritem.machinefilterid, machinefilteritem.machinefilteritemorder
            , machinefilteritem.machinefilteritemrule, machinefilteritem.machinefilteritemid FROM 
              machinefilteritem, {0} WHERE machinefilteritem.machinefilteritemid = {0}.machinefilteritemid) as subquery 
            WHERE {0}.machinefilteritemid = subquery.machinefilteritemid;";
        String baseAlterSqlQuery = "ALTER TABLE {0} ALTER COLUMN machinefilterid SET NOT NULL;";
        foreach(String tableName in listOfTable){
          String updateSqlQuery = String.Format(baseUpdateSqlQuery,tableName);
          String alterSqlQuery = String.Format(baseAlterSqlQuery,tableName);
          Database.ExecuteNonQuery(updateSqlQuery);
          Database.ExecuteNonQuery(alterSqlQuery);
        }
      }
    }
    
    /// <summary>
    /// Remove added Table
    /// </summary>
    protected void RemoveTable()
    {
      foreach(String tableName in listOfTable){
        Database.RemoveForeignKey(tableName,String.Format("fk_{0}_machinefilteritem",tableName));
      }
      
      if(Database.TableExists(TableName.MACHINE_FILTER_ITEM)) {
        Database.RemoveTable (TableName.MACHINE_FILTER_ITEM);
      }
    }
    #endregion
  }
}
