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
  /// Implementation of <see cref="Lemoine.ModelDAO.IReasonGroupDAO">IReasonGroupDAO</see>
  /// </summary>
  public class ReasonGroupDAO
    : VersionableNHibernateDAO<ReasonGroup, IReasonGroup, int>
    , IReasonGroupDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonGroupDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      { // Default
        IReasonGroup reasonGroup = new ReasonGroup ((int)ReasonGroupId.Default, "ReasonGroupDefault");
        reasonGroup.Color = "#FFFF00"; // Yellow
        reasonGroup.ReportColor = "#FFFF00";
        InsertDefaultValue (reasonGroup);
      }
      { // Motion
        IReasonGroup reasonGroup = new ReasonGroup ((int)ReasonGroupId.Motion, "ReasonGroupMotion");
        reasonGroup.Color = "#008000"; // Green
        reasonGroup.ReportColor = "#008000";
        InsertDefaultValue (reasonGroup);
      }
      { // Short
        IReasonGroup reasonGroup = new ReasonGroup ((int)ReasonGroupId.Short, "ReasonGroupShort");
        reasonGroup.Color = "#FFA500"; // Orange
        reasonGroup.ReportColor = "#FFA500";
        InsertDefaultValue (reasonGroup);
      }
      { // Idle
        IReasonGroup reasonGroup = new ReasonGroup ((int)ReasonGroupId.Idle, "ReasonGroupIdle");
        reasonGroup.Color = "#FFFF00"; // Yellow
        reasonGroup.ReportColor = "#FFFF00";
        InsertDefaultValue (reasonGroup);
      }      
      { // Unknown
        IReasonGroup reasonGroup = new ReasonGroup ((int)ReasonGroupId.Unknown, "ReasonGroupUnknown");
        reasonGroup.Color = "#D3D3D3"; // LightGray
        reasonGroup.ReportColor = "#D3D3D3";
        InsertDefaultValue (reasonGroup);
      }
      { // Auto
        IReasonGroup reasonGroup = new ReasonGroup ((int)ReasonGroupId.Auto, "ReasonGroupAuto");
        reasonGroup.Color = "#FFFF00"; // Yellow
        reasonGroup.ReportColor = "#FFFF00";
        InsertDefaultValue (reasonGroup);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reasonGroup">not null</param>
    private void InsertDefaultValue (IReasonGroup reasonGroup)
    {
      Debug.Assert (null != reasonGroup);
      
      try {
        IReasonGroup existingReasonGroup = FindById (reasonGroup.Id);
        if (null == existingReasonGroup) { // the config does not exist => create it
          log.InfoFormat ("InsertDefaultValue: " +
                          "add id={0} translationKey={1}",
                          reasonGroup.Id, reasonGroup.TranslationKey);
          // Use a raw SQL Command, else the Id is reset
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
          {
            command.CommandText = string.Format (@"INSERT INTO ReasonGroup (reasongroupid, reasongrouptranslationkey, reasongroupcolor)
VALUES ({0}, '{1}', '{2}')",
                                                 reasonGroup.Id, reasonGroup.TranslationKey,
                                                 reasonGroup.Color);
            command.ExecuteNonQuery();
          }
          ModelDAOHelper.DAOFactory.FlushData ();
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("InsertDefaultValue: " +
                         "inserting new reason group {0} " +
                         "failed with {1}",
                         reasonGroup,
                         ex);
      }
    }
    #endregion // DefaultValues
    
    /// <summary>
    /// Get all the items sorted by their Id
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IReasonGroup> FindAllSortedById ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonGroup> ()
        .AddOrder (Order.Asc ("Id"))
        .SetCacheable (true)
        .List<IReasonGroup> ();
    }

    /// <summary>
    /// FindAll implementation
    /// with an eager fetch of the corresponding reasons
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IReasonGroup> FindAllWithReasons ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonGroup> ()
        .Fetch (SelectMode.Fetch, "Reasons")
        .AddOrder(Order.Asc("Id"))
        // Note: without the following line, some rows are duplicated.Add Is it a bug ?
        .SetResultTransformer(NHibernate.Transform.Transformers.DistinctRootEntity)
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IReasonGroup> ();
    }
    
  }
}
