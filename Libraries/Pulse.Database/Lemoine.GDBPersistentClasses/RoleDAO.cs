// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IRoleDAO">IRoleDAO</see>
  /// </summary>
  public class RoleDAO
    : VersionableNHibernateDAO<Role, IRole, int>
    , IRoleDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RoleDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      { // 1: Operator
        IRole role = new Role (1, "RoleOperator");
        role.WebAppKey = "operator";
        InsertDefaultValue (role);
      }
      { // 2: Set-up
        IRole role = new Role (2, "RoleSetUp");
        role.WebAppKey = "setup";
        InsertDefaultValue (role);
      }
      { // 3: Quality
        IRole role = new Role (3, "RoleQuality");
        role.WebAppKey = "quality";
        InsertDefaultValue (role);
      }
      { // 4: Supervisor
        IRole role = new Role (4, "RoleSupervisor");
        role.WebAppKey = "supervisor";
        InsertDefaultValue (role);
      }
      { // 5: Manager
        IRole role = new Role (5, "RoleManager");
        role.WebAppKey = "manager";
        InsertDefaultValue (role);
      }
      { // 6: Administrator
        IRole role = new Role (6, "RoleAdministrator");
        role.WebAppKey = "administrator";
        InsertDefaultValue (role);
      }
      
      ResetSequence (100);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="role">not null</param>
    private void InsertDefaultValue (IRole role)
    {
      Debug.Assert (null != role);
      
      try {
        if (null == FindById (role.Id)) { // the config does not exist => create it
          log.Info ($"InsertDefaultValue: add id={role.Id} translationKey={role.TranslationKey} webappkey={role.WebAppKey}");
          // Use a raw SQL Command, else the Id is resetted
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
          {
            command.CommandText = $@"INSERT INTO role (roleid, roletranslationkey, rolewebappkey)
VALUES ({role.Id}, '{role.TranslationKey}', '{role.WebAppKey??""}')";
            command.ExecuteNonQuery();
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"InsertDefaultValue: inserting new role {role} failed", ex);
      }
    }
    
    private void ResetSequence (int minId)
    {
      try {
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
        {
          command.CommandText = string.Format (@"
WITH maxid AS (SELECT MAX({1}) AS maxid FROM {0})
SELECT SETVAL('{0}_{1}_seq', CASE WHEN (SELECT maxid FROM maxid) < {2} THEN {2} ELSE (SELECT maxid FROM maxid) + 1 END);",
                                               "role", "roleid",
                                               minId);
          command.ExecuteNonQuery();
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("ResetSequence: " +
                         "resetting the sequence failed with {0}",
                         ex);
      }
    }
    #endregion // DefaultValues
  }
}
