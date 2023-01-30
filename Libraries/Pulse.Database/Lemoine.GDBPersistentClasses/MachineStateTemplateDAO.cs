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
using NHibernate.Transform;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateDAO">IMachineStateTemplateDAO</see>
  /// </summary>
  public class MachineStateTemplateDAO
    : VersionableNHibernateDAO<MachineStateTemplate, IMachineStateTemplate, int>
    , IMachineStateTemplateDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      // 2: Unattended
      IMachineStateTemplate unattended = new MachineStateTemplate (2, "MachineStateTemplateUnattended", false);
      InsertDefaultValue (unattended);
      // 1: Attended
      IMachineStateTemplate attended = new MachineStateTemplate (1, "MachineStateTemplateAttended", true);
      attended.OnSite = true;
      attended.SiteAttendanceChange = unattended;
      InsertDefaultValue (attended);
      // 3: On-site
      IMachineStateTemplate onSite = new MachineStateTemplate (3, "MachineStateTemplateOnSite", true);
      onSite.OnSite = true;
      onSite.SiteAttendanceChange = unattended;
      InsertDefaultValue (onSite);
      // 4: On-call
      IMachineStateTemplate onCall = new MachineStateTemplate (4, "MachineStateTemplateOnCall", true);
      onCall.OnSite = false;
      onCall.SiteAttendanceChange = onSite;
      InsertDefaultValue (onCall);
      // 5: Off
      IMachineStateTemplate off = new MachineStateTemplate (5, "MachineStateTemplateOff", false);
      InsertDefaultValue (off);
      // 7: Setup
      IMachineStateTemplate setUp = new MachineStateTemplate (7, "MachineStateTemplateSetUp", true);
      setUp.OnSite = true;
      setUp.SiteAttendanceChange = unattended;
      InsertDefaultValue (setUp);
      // 8: Quality check
      IMachineStateTemplate qualityCheck = new MachineStateTemplate (8, "MachineStateTemplateQualityCheck", false);
      InsertDefaultValue (qualityCheck);
      // 9: Production
      IMachineStateTemplate production = new MachineStateTemplate (9, "MachineStateTemplateProduction", true);
      production.OnSite = true;
      production.SiteAttendanceChange = unattended;
      InsertDefaultValue (production);
      // 10: Maintenance
      IMachineStateTemplate maintenance = new MachineStateTemplate (10, "MachineStateTemplateMaintenance", false);
      InsertDefaultValue (maintenance);
      
      ResetSequence (100);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="machineStateTemplate">not null</param>
    private void InsertDefaultValue (IMachineStateTemplate machineStateTemplate)
    {
      Debug.Assert (null != machineStateTemplate);
      
      try {
        if (null == FindById (machineStateTemplate.Id)) { // the config does not exist => create it
          log.InfoFormat ("InsertDefaultValue: " +
                          "add id={0} translationKey={1}",
                          machineStateTemplate.Id, machineStateTemplate.TranslationKey);
          // Use a raw SQL Command, else the Id is resetted
          string onSite = "NULL";
          if (machineStateTemplate.OnSite.HasValue) {
            onSite = machineStateTemplate.OnSite.Value ? "TRUE" : "FALSE";
          }
          string attendanceChange = "NULL";
          if (null != machineStateTemplate.SiteAttendanceChange) {
            attendanceChange = machineStateTemplate.SiteAttendanceChange.Id.ToString ();
          }
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
          {
            command.CommandText = string.Format (@"INSERT INTO MachineStateTemplate (MachineStateTemplateid, MachineStateTemplatetranslationkey, MachineStateTemplateuserrequired, MachineStateTemplateonsite, MachineStateTemplateidsiteattendancechange, machinestatetemplateshiftrequired)
VALUES ({0}, '{1}', {2}, {3}, {4}, FALSE)",
                                                 machineStateTemplate.Id,
                                                 machineStateTemplate.TranslationKey,
                                                 machineStateTemplate.UserRequired ? "TRUE" : "FALSE",
                                                 onSite,
                                                 attendanceChange);
            command.ExecuteNonQuery();
            command.CommandText = string.Format (@"INSERT INTO MachineStateTemplateItem (MachineStateTemplateid, machinestatetemplateitemorder, machineobservationstateid)
VALUES ({0}, 0, {1})",
                                                 machineStateTemplate.Id, machineStateTemplate.Id);
            command.ExecuteNonQuery();
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("InsertDefaultValue: " +
                         "inserting new machine observation state {0} " +
                         "failed with {1}",
                         machineStateTemplate,
                         ex);
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
                                               "machinestatetemplate", "machinestatetemplateid",
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
    
    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IMachineStateTemplateDAO.FindAllForConfig" />
    /// 
    /// Note: this is not registered to be cacheable because of the eager fetch
    /// </summary>
    /// <returns></returns>
    public IList<IMachineStateTemplate> FindAllForConfig()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<MachineStateTemplate>()
        .Fetch (SelectMode.Fetch, "Items")
        .Fetch (SelectMode.Fetch, "Items.Shift")
        .Fetch (SelectMode.Fetch, "Items.MachineObservationState")
        .Fetch (SelectMode.Fetch, "Stops")
        .SetResultTransformer(new DistinctRootEntityResultTransformer()) // Remove duplicate root entity
        .AddOrder(Order.Asc("Id"))
        .List<IMachineStateTemplate>();
    }
    
    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IMachineStateTemplateDAO.FindByCategory" />
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public IList<IMachineStateTemplate> FindByCategory (MachineStateTemplateCategory category)
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<MachineStateTemplate>()
        .Add (Restrictions.Eq ("Category", category))
        .AddOrder(Order.Asc("Id"))
        .List<IMachineStateTemplate>();
    }
    
    /// <summary>
    /// MakePersistent with a cascade option
    /// </summary>
    /// <param name="entity"></param>
    public override IMachineStateTemplate MakePersistent (IMachineStateTemplate entity)
    {
      IMachineStateTemplate result = base.MakePersistent (entity);
      foreach (var item in entity.Items) {
        NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdate (item);
      }
      foreach (var stop in entity.Stops) {
        NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdate (stop);
      }
      return result;
    }

    /// <summary>
    /// MakePersistent with a cascade option
    /// </summary>
    /// <param name="entity"></param>
    public override async Task<IMachineStateTemplate> MakePersistentAsync (IMachineStateTemplate entity)
    {
      IMachineStateTemplate result = await base.MakePersistentAsync (entity);
      foreach (var item in entity.Items) {
        await NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdateAsync (item);
      }
      foreach (var stop in entity.Stops) {
        await NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdateAsync (stop);
      }
      return result;
    }

    /// <summary>
    /// MakeTransient with a cascade action
    /// </summary>
    /// <param name="entity"></param>
    public override void MakeTransient(IMachineStateTemplate entity)
    {
      foreach (var item in entity.Items) {
        NHibernateHelper.GetCurrentSession ()
          .Delete (item);
      }
      foreach (var stop in entity.Stops) {
        NHibernateHelper.GetCurrentSession ()
          .Delete (stop);
      }
      entity.Items.Clear ();
      entity.Stops.Clear ();
      base.MakeTransient(entity);
    }

    /// <summary>
    /// MakeTransient with a cascade action
    /// </summary>
    /// <param name="entity"></param>
    public override async System.Threading.Tasks.Task MakeTransientAsync (IMachineStateTemplate entity)
    {
      foreach (var item in entity.Items) {
        await NHibernateHelper.GetCurrentSession ()
          .DeleteAsync (item);
      }
      foreach (var stop in entity.Stops) {
        await NHibernateHelper.GetCurrentSession ()
          .DeleteAsync (stop);
      }
      entity.Items.Clear ();
      entity.Stops.Clear ();
      await base.MakeTransientAsync (entity);
    }
  }
}
