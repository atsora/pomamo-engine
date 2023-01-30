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
  /// Implementation of <see cref="Lemoine.ModelDAO.IReasonDAO">IReasonDAO</see>
  /// </summary>
  public class ReasonDAO
    : VersionableNHibernateDAO<Reason, IReason, int>
    , IReasonDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      { // UndefinedValue
        IReason reason = new Reason ((int)ReasonId.Undefined, "UndefinedValue",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Default));
        InsertDefaultValue (reason);
      }
      { // Motion
        IReason reason = new Reason ((int)ReasonId.Motion, "ReasonMotion",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Motion));
        InsertDefaultValue (reason);
      }
      { // Short
        IReason reason = new Reason ((int)ReasonId.Short, "ReasonShort",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Short));
        InsertDefaultValue (reason);
      }
      { // Unanswered
        IReason reason = new Reason ((int)ReasonId.Unanswered, "ReasonUnanswered",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Idle));
        InsertDefaultValue (reason);
      }
      { // Unattended
        IReason reason = new Reason ((int)ReasonId.Unattended, "ReasonUnattended",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Idle));
        InsertDefaultValue (reason);
      }
      { // Off
        IReason reason = new Reason ((int)ReasonId.Off, "ReasonOff",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Auto));
        InsertDefaultValue (reason);
      }
      { // Unknown
        IReason reason = new Reason ((int)ReasonId.Unknown, "ReasonUnknown",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Unknown));
        InsertDefaultValue (reason);
      }
      { // Processing
        IReason reason = new Reason ((int)ReasonId.Processing, "ReasonProcessing",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Unknown));
        InsertDefaultValue (reason);
      }
      { // Break
        IReason reason = new Reason ((int)ReasonId.Break, "ReasonBreak",
                                     new ReasonGroupDAO ().FindById ((int)ReasonGroupId.Auto));
        InsertDefaultValue (reason);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reason">not null</param>
    private void InsertDefaultValue (IReason reason)
    {
      Debug.Assert (null != reason);
      
      try {
        IReason existingReason = FindById (reason.Id);
        if (null == existingReason) { // the config does not exist => create it
          log.InfoFormat ("InsertDefaultValue: " +
                          "add id={0} translationKey={1}",
                          reason.Id, reason.TranslationKey);
          // Use a raw SQL Command, else the Id is reset
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
          {
            command.CommandText = string.Format (@"INSERT INTO Reason (reasonid, reasontranslationkey, reasongroupid)
VALUES ({0}, '{1}', {2})",
                                                 reason.Id, reason.TranslationKey,
                                                 reason.ReasonGroup.Id);
            command.ExecuteNonQuery();
          }
          ModelDAOHelper.DAOFactory.FlushData ();
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("InsertDefaultValue: " +
                         "inserting new reason {0} " +
                         "failed with {1}",
                         reason,
                         ex);
      }
    }
    #endregion // DefaultValues
    
    /// <summary>
    /// Find a reason with the specified code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public IReason FindByCode (string code)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Reason> ()
        .Add (Restrictions.Eq ("Code", code))
        .UniqueResult<IReason> ();
    }
    
    /// <summary>
    /// FindAll implementation
    /// with an eager fetch of the corresponding ReasonGroup
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IReason> FindAllWithReasonGroup ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Reason> ()
        .AddOrder(Order.Asc("Id"))
        .Fetch (SelectMode.Fetch, "ReasonGroup")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IReason> ();
    }
    
    /// <summary>
    /// FindAll implementation
    /// with an eager fetch of the corresponding ReasonGroup
    /// and restricting the result set to a given reason group
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="reasonGroup"></param>
    /// <returns></returns>
    public IList<IReason> FindAllWithReasonGroup (IReasonGroup reasonGroup)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Reason> ()
        .Add (Restrictions.Eq ("ReasonGroup", reasonGroup))
        .AddOrder(Order.Asc("Id"))
        .Fetch (SelectMode.Fetch, "ReasonGroup")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IReason> ();
    }
  }
}
