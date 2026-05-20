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

    // Note: check the keys Database.Default.MachineObservationState.* 
    // are also correctly set before activating some of the keys here,
    // else the default values will not be inserted because of the consistency check

    /// <summary>
    /// Configuration key to insert default values that are more mold shops
    /// or shops that were machines are running without any people:
    /// Unattended, Attended, OnSite, OnCall
    /// 
    /// It must be on for unit tests databases
    /// </summary>
    static readonly string UNATTENDED_TO_ON_CALL_KEY = "Database.Default.MachineStateTemplate.UnattendedToOnCall";
    static readonly bool UNATTENDED_TO_ON_CALL_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default values Production and NoProduction
    /// that are applicable to most production shops
    /// 
    /// On by default
    /// </summary>
    static readonly string PRODUCTION_NO_PRODUCTION_KEY = "Database.Default.MachineStateTemplate.ProductionNoProduction";
    static readonly bool PRODUCTION_NO_PRODUCTION_DEFAULT = true;

    /// <summary>
    /// Configuration key to insert default value Off
    /// </summary>
    static readonly string OFF_KEY = "Database.Default.MachineStateTemplate.Off";
    static readonly bool OFF_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value SetUp
    /// </summary>
    static readonly string SETUP_KEY = "Database.Default.MachineStateTemplate.SetUp";
    static readonly bool SETUP_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value QualityCheck
    /// </summary>
    static readonly string QUALITY_CHECK_KEY = "Database.Default.MachineStateTemplate.QualityCheck";
    static readonly bool QUALITY_CHECK_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value Maintenance
    /// </summary>
    static readonly string MAINTENANCE_KEY = "Database.Default.MachineStateTemplate.Maintenance";
    static readonly bool MAINTENANCE_DEFAULT = false;

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (UNATTENDED_TO_ON_CALL_KEY, UNATTENDED_TO_ON_CALL_DEFAULT)) {
        // 2: Unattended
        IMachineStateTemplate unattended = new MachineStateTemplate ((int)MachineObservationStateId.Unattended, "MachineStateTemplateUnattended", false);
        InsertDefaultValue (unattended);
        // 1: Attended
        IMachineStateTemplate attended = new MachineStateTemplate ((int)MachineObservationStateId.Attended, "MachineStateTemplateAttended", true);
        attended.OnSite = true;
        attended.SiteAttendanceChange = unattended;
        InsertDefaultValue (attended);
        // 3: On-site
        IMachineStateTemplate onSite = new MachineStateTemplate ((int)MachineObservationStateId.OnSite, "MachineStateTemplateOnSite", true);
        onSite.OnSite = true;
        onSite.SiteAttendanceChange = unattended;
        InsertDefaultValue (onSite);
        // 4: On-call
        IMachineStateTemplate onCall = new MachineStateTemplate ((int)MachineObservationStateId.OnCall, "MachineStateTemplateOnCall", true);
        onCall.OnSite = false;
        onCall.SiteAttendanceChange = onSite;
        InsertDefaultValue (onCall);
      }

      if (Lemoine.Info.ConfigSet.LoadAndGet (PRODUCTION_NO_PRODUCTION_KEY, PRODUCTION_NO_PRODUCTION_DEFAULT)) {
        // 15: NoProduction
        IMachineStateTemplate noProduction = new MachineStateTemplate ((int)MachineObservationStateId.NoProduction, "MachineStateTemplateNoProduction", true);
        noProduction.OnSite = false;
        InsertDefaultValue (noProduction);
        // 9: Production
        IMachineStateTemplate production = new MachineStateTemplate ((int)MachineObservationStateId.Production, "MachineStateTemplateProduction", true);
        production.OnSite = true;
        production.SiteAttendanceChange = noProduction;
        InsertDefaultValue (production);
      }

      if (Lemoine.Info.ConfigSet.LoadAndGet (OFF_KEY, OFF_DEFAULT)) {
        // 5: Off
        IMachineStateTemplate off = new MachineStateTemplate ((int)MachineObservationStateId.Off, "MachineStateTemplateOff", false);
        InsertDefaultValue (off);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (SETUP_KEY, SETUP_DEFAULT)) {
        // 7: Setup
        IMachineStateTemplate setUp = new MachineStateTemplate ((int)MachineObservationStateId.SetUp, "MachineStateTemplateSetUp", true);
        setUp.OnSite = true;
        InsertDefaultValue (setUp);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (QUALITY_CHECK_KEY, QUALITY_CHECK_DEFAULT)) {
        // 8: Quality check
        IMachineStateTemplate qualityCheck = new MachineStateTemplate ((int)MachineObservationStateId.QualityCheck, "MachineStateTemplateQualityCheck", false);
        InsertDefaultValue (qualityCheck);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (MAINTENANCE_KEY, MAINTENANCE_DEFAULT)) {
        // 10: Maintenance
        IMachineStateTemplate maintenance = new MachineStateTemplate ((int)MachineObservationStateId.Maintenance, "MachineStateTemplateMaintenance", false);
        InsertDefaultValue (maintenance);
      }

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
          log.Info ($"InsertDefaultValue: add id={machineStateTemplate.Id} translationKey={machineStateTemplate.TranslationKey}");
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
            command.CommandText = $"""
INSERT INTO MachineStateTemplateItem (MachineStateTemplateid, machinestatetemplateitemorder, machineobservationstateid)              
VALUES ({machineStateTemplate.Id}, 0, {machineStateTemplate.Id})
""";
            command.ExecuteNonQuery();
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"InsertDefaultValue: inserting new machine observation state {machineStateTemplate} failed", ex);
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
        log.Error ($"ResetSequence: resetting the sequence failed", ex);
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
