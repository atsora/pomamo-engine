// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateRightDAO">IMachineStateTemplateRightDAO</see>
  /// </summary>
  public class MachineStateTemplateRightDAO
    : VersionableNHibernateDAO<MachineStateTemplateRight, IMachineStateTemplateRight, int>
    , IMachineStateTemplateRightDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateRightDAO).FullName);

    /// <summary>
    /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateRightDAO">GetDefault</see>
    /// </summary>
    /// <param name="role">not null</param>
    /// <returns></returns>
    public RightAccessPrivilege GetDefault (IRole role)
    {
      Debug.Assert (null != role);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities (which may cause some LazyInitialization problems)

      IMachineStateTemplateRight defaultRight = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineStateTemplateRight> ()
        .Add (Restrictions.IsNull ("MachineStateTemplate"))
        .Add (Restrictions.Eq ("Role.Id", role.Id))
        .SetCacheable (true)
        .UniqueResult <IMachineStateTemplateRight> ();
      if (null == defaultRight) { // Not found
        log.DebugFormat ("GetDefault: " +
                         "no default MachineStateTemplateRight found " +
                         "=> return Granted");
        return RightAccessPrivilege.Granted;
      }
      else {
        return defaultRight.AccessPrivilege;
      }
    }
    
    /// <summary>
    /// Get the list of templates for a specified role and access privilege
    /// without considering the default value
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="role">not null</param>
    /// <param name="accessPrivilege"></param>
    /// <returns></returns>
    IList<IMachineStateTemplate> GetWithoutDefault (IRole role, RightAccessPrivilege accessPrivilege)
    {
      Debug.Assert (null != role);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities (which may cause some LazyInitialization problems)

      return NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("MachineStateTemplateRightWithoutDefault")
        .SetParameter ("roleId", role.Id)
        .SetParameter ("accessPrivilege", accessPrivilege)
        .SetCacheable (true)
        .List<IMachineStateTemplate> ();
    }
    
    /// <summary>
    /// Get the granted machine state templates for the specified role
    /// </summary>
    /// <param name="role">not null</param>
    /// <returns></returns>
    public IList<IMachineStateTemplate> GetGranted (IRole role)
    {
      // TODO: make it generic for all Rights
      
      Debug.Assert (null != role);
      
      if (GetDefault (role).Equals (RightAccessPrivilege.Granted)) { // Default is Granted
        IList<IMachineStateTemplate> allMachineStateTemplates = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindAll ();
        IList<IMachineStateTemplate> deniedTemplates = GetWithoutDefault (role, RightAccessPrivilege.Denied);
        IList<IMachineStateTemplate> machineStateTemplates = new List<IMachineStateTemplate> ();
        foreach (IMachineStateTemplate machineStateTemplate in allMachineStateTemplates) {
          if (!deniedTemplates.Contains (machineStateTemplate)) {
            machineStateTemplates.Add (machineStateTemplate);
          }
        }
        return machineStateTemplates;
      }
      else { // Default is Denied
        return GetWithoutDefault (role, RightAccessPrivilege.Granted);
      }
    }
    
    /// <summary>
    /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateRightDAO">FindWithForConfig</see>
    /// </summary>
    public IList<IMachineStateTemplateRight> FindWithForConfig(IList<IRole> roles){
      IList<IMachineStateTemplateRight> machineStateTemplateRights;
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IMachineStateTemplateRight> ()
        .Fetch (SelectMode.Fetch, "MachineStateTemplate")
        .Fetch (SelectMode.Fetch, "Role")
        .AddOrder(Order.Asc("Id"));
      
      //Criteria Build
      if( roles.Count > 1) {
        Junction mModeDisjunction = Restrictions.Disjunction();
        foreach(IRole role in roles){
          mModeDisjunction.Add (Restrictions.Eq ("Role", role));
        }
        criteria.Add(mModeDisjunction);
        
        machineStateTemplateRights = criteria.List<IMachineStateTemplateRight> ();
      }
      else {
        criteria.Add (Restrictions.Eq ("Role", roles[0]));
        machineStateTemplateRights = criteria.List<IMachineStateTemplateRight> ();
      }
      
      return machineStateTemplateRights;
    }
    
  }
}
