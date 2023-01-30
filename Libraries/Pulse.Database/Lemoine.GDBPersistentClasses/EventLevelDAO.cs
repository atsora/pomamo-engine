// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IEventLevelDAO">IEventLevelDAO</see>
  /// </summary>
  public class EventLevelDAO
    : VersionableNHibernateDAO<EventLevel, IEventLevel, int>
    , IEventLevelDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (EventLevelDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      {
        IEventLevel eventLevel = new EventLevel (1, 100, "EventLevelAlert");
        InsertDefaultValue (eventLevel);
      }
      {
        IEventLevel eventLevel = new EventLevel (2, 300, "EventLevelError");
        InsertDefaultValue (eventLevel);
      }
      {
        IEventLevel eventLevel = new EventLevel (3, 400, "EventLevelWarn");
        InsertDefaultValue (eventLevel);
      }
      {
        IEventLevel eventLevel = new EventLevel (4, 500, "EventLevelNotice");
        InsertDefaultValue (eventLevel);
      }
      {
        IEventLevel eventLevel = new EventLevel (5, 600, "EventLevelInfo");
        InsertDefaultValue (eventLevel);
      }
      // Note: this is not needed to reset the event level sequence here
      //       because these event levels should have been already set by the migration
    }
    
    private void InsertDefaultValue (IEventLevel eventLevel)
    {
      Debug.Assert (null != eventLevel);
      
      try {
        if (null == FindById (eventLevel.Id)) { // the config does not exist => create it
          log.InfoFormat ("InsertDefaultValue: " +
                          "add id={0} translationKey={1}",
                          eventLevel.Id, eventLevel.TranslationKey);
          // Use a raw SQL Command, else the Id is resetted
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
          {
            command.CommandText = string.Format (@"INSERT INTO eventlevel (eventlevelid, eventlevelpriority, eventleveltranslationkey)
VALUES ({0}, {1}, '{2}')",
                                                 eventLevel.Id,
                                                 eventLevel.Priority,
                                                 eventLevel.TranslationKey);
            command.ExecuteNonQuery();
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("InsertDefaultValue: " +
                         "inserting new event level {0} " +
                         "failed with {1}",
                         eventLevel,
                         ex);
      }
    }
    #endregion // DefaultValues
    
    /// <summary>
    /// Implementation of <see cref="Lemoine.ModelDAO.IEventLevelDAO.FindAllForConfig">IFieldDAO</see>
    /// </summary>
    public IList<IEventLevel> FindAllForConfig()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<IEventLevel>()
        .AddOrder(Order.Asc("Priority"))
        .List<IEventLevel>();
    }
    
    /// <summary>
    /// Return all IEventLevel that match a specified priority
    /// </summary>
    /// <param name="priority"></param>
    /// <returns></returns>
    public IList<IEventLevel> FindByPriority (int priority)
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<IEventLevel>()
        .Add (Restrictions.Eq ("Priority", priority))
        .List<IEventLevel>();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IEventLevelDAO.IsEventLevelUsed" />
    /// </summary>
    /// <param name="eventLevel"></param>
    /// <returns>bool</returns>
    public bool IsEventLevelUsed(IEventLevel eventLevel){
      int isUsedinEventCncValue = TestIsEventLevelUsedIn<EventCncValue>(eventLevel);
      int isUsedinEventCncValueConfig = TestIsEventLevelUsedIn<EventCncValueConfig>(eventLevel);
      int isUsedinEventLongPeriod = TestIsEventLevelUsedIn<EventLongPeriod>(eventLevel);
      int isUsedinEventLongPeriodConfig = TestIsEventLevelUsedIn<EventLongPeriodConfig>(eventLevel);
      
      return (isUsedinEventCncValue+isUsedinEventCncValueConfig+isUsedinEventLongPeriod+isUsedinEventLongPeriodConfig)>= 1 ? true : false;
    }
    
    /// <summary>
    /// Generic way used to test if a EvenLevel is linked to any data
    /// usage : TestIsEventLevelUsedIn&lt;ClassWithLevelMapping&gt;(eventLevel)
    /// </summary>
    /// <param name="eventLevel"></param>
    /// <returns>int count of Id with Eq(Level)</returns>
    private int TestIsEventLevelUsedIn<T>(IEventLevel eventLevel) where T: class {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<T>()
        .Add(Restrictions.Eq("Level", eventLevel))
        .SetProjection(Projections.CountDistinct("Id"))
        .UniqueResult<int>();
    }
  }
}
