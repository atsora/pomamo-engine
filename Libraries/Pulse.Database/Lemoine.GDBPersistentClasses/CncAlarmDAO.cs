// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncAlarmDAO">ICncAlarmDAO</see>
  /// </summary>
  public class CncAlarmDAO :
    VersionableByMachineModuleNHibernateDAO<CncAlarm, ICncAlarm, int>, ICncAlarmDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncAlarmDAO () : base ("MachineModule") { }

    /// <summary>
    /// Find all ICncAlarm for a specified machineModule
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public IList<ICncAlarm> FindByMachineModule (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .AddOrder (Order.Asc (GetUpperProjection ()))
        .List<ICncAlarm> ();
    }

    /// <summary>
    /// Find the active cnc alarms at the specified time for the specified machine module
    /// 
    /// This request is not cacheable because it is a partitioned table
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindAt (IMachineModule machineModule, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
        // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), dateTime, "@>"))
        .AddOrder (Order.Asc ("DateTimeRange")) // Ok because they are not so many
                                                // Not cacheable because it may cause some problems with a partitioned table:
                                                // there is a risk to make NHibernate use a FindById without the secondary key
        .List<ICncAlarm> ();
    }

    /// <summary>
    /// Find the active cnc alarms at the specified time for the specified machine module with an eager fetch of the severity
    /// 
    /// This request is not cacheable because of the early fetch of the severity and because it is a partitioned table
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindAtWithSeverity (IMachineModule machineModule, DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
        // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), dateTime, "@>"))
        .AddOrder (Order.Asc ("DateTimeRange")) // Ok because they are not so many
                                                // Not cacheable because it may cause some problems with a partitioned table:
                                                // there is a risk to make NHibernate use a FindById without the secondary key
        .Fetch (SelectMode.Fetch, "Severity")
        .List<ICncAlarm> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindOverlapsRange (IMachineModule machineModule, UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<ICncAlarm> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range with an eager fetch of the severity
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindOverlapsRangeWithSeverity (IMachineModule machineModule, UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .Fetch (SelectMode.Fetch, "Severity")
        .List<ICncAlarm> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IEnumerable<ICncAlarm> FindOverlapsRange (IMonitoredMachine machine, UtcDateTimeRange range)
    {
      return machine.MachineModules
        .SelectMany (m => FindOverlapsRange (m, range))
        .OrderBy (cncAlarm => cncAlarm.DateTimeRange);
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// 
    /// Be careful using this method. It looks like it is inefficient.
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IEnumerable<ICncAlarm> FindOverlapsRangeWithSeverity (IMonitoredMachine machine, UtcDateTimeRange range)
    {
      return machine.MachineModules
        .SelectMany (m => FindOverlapsRangeWithSeverity (m, range))
        .OrderBy (cncAlarm => cncAlarm.DateTimeRange);
    }

    /// <summary>
    /// Get all cnc types that have been found
    /// </summary>
    /// <returns></returns>
    public IList<string> FindAllCncTypes ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateQuery ("SELECT DISTINCT CncInfo FROM CncAlarm")
        .List<string> ();
    }

    /// <summary>
    /// Get all alarms of a particular cnc, after a specific date
    /// The severity is NOT loaded
    /// </summary>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IEnumerable<ICncAlarm> FindByCncRange (string cncType, UtcDateTimeRange range)
    {
      var machineModules = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindAllWithMonitoredMachine ();
      return machineModules
        .SelectMany (m => FindByCncRange (m, cncType, range))
        .OrderBy (cncAlarm => cncAlarm.DateTimeRange);
    }

    /// <summary>
    /// Get all alarms of a particular cnc, after a specific date
    /// The severity is NOT loaded
    /// </summary>
    /// <param name="machineModule">not null or empty</param>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindByCncRange (IMachineModule machineModule, string cncType, UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);

      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (OverlapRange (range)); // Overlap

      // Restriction of the cnc type?
      if (!string.IsNullOrEmpty (cncType)) {
        criteria = criteria.Add (Restrictions.Eq ("CncInfo", cncType));
      }

      var alarms = criteria
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<ICncAlarm> ();

      return alarms;
    }

    /// <summary>
    /// Get all alarms of a particular cnc, after a specific date
    /// The severity is loaded
    /// </summary>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IEnumerable<ICncAlarm> FindByCncRangeWithSeverity (string cncType, UtcDateTimeRange range)
    {
      var alarms = FindByCncRange (cncType, range);

      // Extra step: initialize the severities
      foreach (var alarm in alarms) {
        ModelDAOHelper.DAOFactory.Initialize (alarm.Severity);
      }

      return alarms;
    }

    /// <summary>
    /// Get all alarms of a particular cnc, after a specific date
    /// The severity is loaded
    /// </summary>
    /// <param name="machineModule">not null or empty</param>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindByCncRangeWithSeverity (IMachineModule machineModule, string cncType, UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      var alarms = FindByCncRange (machineModule, cncType, range);

      // Extra step: initialize the severities
      foreach (var alarm in alarms) {
        ModelDAOHelper.DAOFactory.Initialize (alarm.Severity);
      }

      return alarms;
    }

    /// <summary>
    /// Find the next ICncAlarms for a specified machineModule
    /// whose ID is strictly more than the specified ID
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="id"></param>
    /// <param name="maxNbFetched"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindAfterID (IMachineModule machineModule, int id, int maxNbFetched)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Gt ("Id", id))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (maxNbFetched)
        .List<ICncAlarm> ();
    }

    /// <summary>
    /// Find all the slots whose beginning is within the specified range
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindWithBeginningInRange (IMachineModule machineModule, UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (new SimpleExpression ("DateTimeRange", range, "&&")) // Overlap
        .Add (new SimpleExpression ("DateTimeRange", range, "&>")) // + Doesn't extend to the left of
                                                                   // = beginning is included in the range
        .AddOrder (Order.Asc ("DateTimeRange")) // No need for an index, since there won't be many
        .List<ICncAlarm> ();
    }

    /// <summary>
    /// Find all the slots whose beginning is within the specified range
    /// Order them by ascending range
    /// The severity is loaded
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncAlarm> FindWithBeginningInRangeWithSeverity (IMachineModule machineModule, UtcDateTimeRange range)
    {
      var alarms = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (new SimpleExpression ("DateTimeRange", range, "&&")) // Overlap
        .Add (new SimpleExpression ("DateTimeRange", range, "&>")) // + Doesn't extend to the left of
                                                                   // = beginning is included in the range
        .Fetch (SelectMode.Fetch, "Severity")
        .AddOrder (Order.Asc ("DateTimeRange")) // No need for an index, since there won't be many
        .List<ICncAlarm> ();

      // Extra step: initialize the severities
      foreach (var alarm in alarms) {
        ModelDAOHelper.DAOFactory.Initialize (alarm.Severity);
      }

      return alarms;
    }

    /// <summary>
    /// Find the first alarm occurring after a specific datetime
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="datetime"></param>
    /// <returns>Can be null</returns>
    public ICncAlarm FindFirstAfter (IMachineModule machineModule, DateTime datetime)
    {
      var lowerBoundProjection = GetLowerProjection ();

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Ge (lowerBoundProjection, datetime))
        .AddOrder (Order.Asc (lowerBoundProjection))
        .SetMaxResults (1)
        .UniqueResult<ICncAlarm> ();
    }

    AbstractCriterion OverlapRange (UtcDateTimeRange range)
    {
      return new SimpleExpression ("DateTimeRange", range, "&&");
    }

    /// <summary>
    /// Range criterion with UTC date/times using the lower/upper bounds
    /// to be able to use some other indexes
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    [Obsolete ("This is pretty inefficient", true)]
    protected virtual AbstractCriterion OverlapRangeUsingBounds (UtcDateTimeRange range)
    {
      if ((null == range) || (range.IsEmpty ())) {
        return Expression.Sql ("FALSE");
      }

      var lowerProjection = GetLowerProjection ();
      var upperProjection = GetUpperProjection ();

      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        if (range.LowerInclusive) {
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (IsUpperInfinite ())
                  .Add (Restrictions.Gt (upperProjection, range.Lower.Value))
                  .Add (Restrictions.Conjunction ()
                        .Add (IsUpperInclusive ())
                        .Add (Restrictions.Ge (upperProjection, range.Lower.Value)))
                 );
        }
        else { // Not inclusive
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (IsUpperInfinite ())
                  .Add (Restrictions.Gt (upperProjection, range.Lower.Value)));
        }
      }

      // To constraint
      if (range.Upper.HasValue) {
        if (range.UpperInclusive) {
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (IsLowerInfinite ())
                  .Add (Restrictions.Lt (lowerProjection, range.Upper.Value))
                  .Add (Restrictions.Conjunction ()
                        .Add (IsLowerInclusive ())
                        .Add (Restrictions.Le (lowerProjection, range.Upper.Value)))
                 );
        }
        else {
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (IsLowerInfinite ())
                  .Add (Restrictions.Lt (lowerProjection, range.Upper.Value)));
        }
      }

      return result;
    }

    /// <summary>
    /// Is the lower bound infinite ?
    /// </summary>
    /// <returns></returns>
    AbstractCriterion IsLowerInfinite ()
    {
      return new FunctionExpression ("DateTimeRange", "lower_inf");
      // Or: return Restrictions.Eq (GetLowerInfProjection (), true);
    }

    /// <summary>
    /// Is the upper bound infinite ?
    /// </summary>
    /// <returns></returns>
    protected virtual AbstractCriterion IsUpperInfinite ()
    {
      return new FunctionExpression ("DateTimeRange", "upper_inf");
      // Or: return Restrictions.Eq (GetUpperInfProjection (), true);
    }

    /// <summary>
    /// Is the lower bound inclusive ?
    /// </summary>
    /// <returns></returns>
    protected virtual AbstractCriterion IsLowerInclusive ()
    {
      return new FunctionExpression ("DateTimeRange", "lower_inc");
      // Or: return Restrictions.Eq (GetLowerIncProjection (), true);
    }

    /// <summary>
    /// Is the upper bound inclusive ?
    /// </summary>
    /// <returns></returns>
    protected virtual AbstractCriterion IsUpperInclusive ()
    {
      return new FunctionExpression ("DateTimeRange", "upper_inc");
      // Or: return Restrictions.Eq (GetUpperIncProjection (), true);
    }

    IProjection GetLowerProjection ()
    {
      return Projections
        .SqlFunction ("lower",
          new NHibernateTypes.UTCDateTimeFullType (),
          Projections.Property ("DateTimeRange"));
    }

    IProjection GetUpperProjection ()
    {
      return Projections
        .SqlFunction ("upper",
          new NHibernateTypes.UTCDateTimeFullType (),
          Projections.Property ("DateTimeRange"));
    }

    IProjection GetLowerInfProjection ()
    {
      return Projections
        .SqlFunction ("lower_inf",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    IProjection GetUpperInfProjection ()
    {
      return Projections
        .SqlFunction ("upper_inf",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    IProjection GetLowerIncProjection ()
    {
      return Projections
        .SqlFunction ("lower_inc",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    IProjection GetUpperIncProjection ()
    {
      return Projections
        .SqlFunction ("upper_inc",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    IProjection GetIsEmptyProjection ()
    {
      return Projections
        .SqlFunction ("isempty",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }
  }
}
