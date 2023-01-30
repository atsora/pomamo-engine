// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncValueDAO">ICncValueDAO</see>
  /// </summary>
  public class CncValueDAO
    : VersionableByMachineModuleNHibernateDAO<CncValue, ICncValue, long>
    , ICncValueDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncValueDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncValueDAO ()
      : base ("MachineModule")
    { }

    /// <summary>
    /// Find the unique ICncValue that matches the specified parameters
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="begin"></param>
    /// <returns></returns>
    public ICncValue FindByMachineModuleFieldBegin (IMachineModule machineModule,
                                                    IField field,
                                                    DateTime begin)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Eq ("Field", field))
        .Add (Restrictions.Eq ("Begin", begin))
        .UniqueResult<ICncValue> ();
    }

    /// <summary>
    /// Find the ICncValues for specified machineModule and field
    /// which cross the interval [begin,end]
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="utcbegin"></param>
    /// <param name="utcend"></param>
    /// <returns></returns>
    public IList<ICncValue> FindByMachineFieldDateRange (IMachineModule machineModule,
                                                        IField field,
                                                        DateTime utcbegin,
                                                        DateTime utcend)
    {
      return FindByMachineFieldDateRange (machineModule,
                                          field,
                                          new UtcDateTimeRange (utcbegin, utcend));
    }

    /// <summary>
    /// Find the ICncValues for specified machineModule and field
    /// in the specified range
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncValue> FindByMachineFieldDateRange (IMachineModule machineModule,
                                                        IField field,
                                                        UtcDateTimeRange range)
    {
      return FindOverlapsRange (machineModule, field, range);
    }

    /// <summary>
    /// Find the next ICncValues for specified machineModule and field
    /// 
    /// The start date/time must be after (including) dateTime
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="dateTime"></param>
    /// <param name="maxNbFetched"></param>
    /// <returns></returns>
    public IList<ICncValue> FindNext (IMachineModule machineModule,
                                      IField field,
                                      DateTime dateTime,
                                      int maxNbFetched)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id))
        .Add (Restrictions.Ge ("Begin", dateTime))
        .AddOrder (Order.Asc ("Begin"))
        .SetMaxResults (maxNbFetched)
        .List<ICncValue> ();
    }

    /// <summary>
    /// Find the ICncValues at a specified date/time for all the fields
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public IList<ICncValue> FindAt (IMachineModule machineModule,
                                    DateTime at)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Le ("Begin", at))
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.Gt ("End", at))
              .Add (Restrictions.Conjunction ()
                    .Add (Restrictions.Eq ("End", at))
                    .Add (Restrictions.EqProperty ("Begin", "End"))
                    )
        )
        // Not cacheable because it may cause some problems with a partitioned table:
        // there is a risk to make NHibernate use a FindById without the secondary key
        .List<ICncValue> ();
    }

    /// <summary>
    /// Find the ICncValues at a specified date/time for all the fields with an eager fetch of the field
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public IList<ICncValue> FindAtWithField (IMachineModule machineModule,
                                             DateTime at)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Le ("Begin", at))
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.Gt ("End", at))
              .Add (Restrictions.Conjunction ()
                    .Add (Restrictions.Eq ("End", at))
                    .Add (Restrictions.EqProperty ("Begin", "End"))
                    )
        )
        .Fetch (SelectMode.Fetch, "Field")
        // Not cacheable because it may cause some problems with a partitioned table:
        // there is a risk to make NHibernate use a FindById without the secondary key
        .List<ICncValue> ();
    }

    /// <summary>
    /// Find the ICncValues at a specified date/time for all the fields with an eager fetch of the field and of the unit
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public IList<ICncValue> FindAtWithFieldUnit (IMachineModule machineModule,
                                                 DateTime at)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Le ("Begin", at))
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.Gt ("End", at))
              .Add (Restrictions.Conjunction ()
                    .Add (Restrictions.Eq ("End", at))
                    .Add (Restrictions.EqProperty ("Begin", "End"))
                    )
        )
        .Fetch (SelectMode.Fetch, "Field")
        .Fetch (SelectMode.Fetch, "Field.Unit")
        // Not cacheable because it may cause some problems with a partitioned table:
        // there is a risk to make NHibernate use a FindById without the secondary key
        .List<ICncValue> ();
    }

    /// <summary>
    /// Find the ICncValue at a specified date/time
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public ICncValue FindAt (IMachineModule machineModule,
                             IField field,
                             DateTime at)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id)) // To make it cacheable
        .Add (Restrictions.Le ("Begin", at))
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.Gt ("End", at))
              .Add (Restrictions.Conjunction ()
                    .Add (Restrictions.Eq ("End", at))
                    .Add (Restrictions.EqProperty ("Begin", "End"))
                    )
        )
        // Not cacheable because it may cause some problems with a partitioned table:
        // there is a risk to make NHibernate use a FindById without the secondary key
        .UniqueResult<ICncValue> ();
    }

    /// <summary>
    /// Find the ICncValue with a specified end date/time
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public ICncValue FindWithEnd (IMachineModule machineModule,
                                  IField field,
                                  DateTime end)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Eq ("Field", field))
        .Add (Restrictions.Lt ("Begin", end))
        .Add (Restrictions.Eq ("End", end))
        // Not cacheable because it may cause some problems with a partitioned table:
        // there is a risk to make NHibernate use a FindById without the secondary key
        .UniqueResult<ICncValue> ();
    }

    AbstractCriterion InUtcRange (UtcDateTimeRange range)
    {
      LowerBound<DateTime> lower;
      if (!range.LowerInclusive && range.Lower.HasValue) {
        lower = range.Lower.Value.AddSeconds (1);
      }
      else {
        lower = range.Lower;
      }

      Junction result = Restrictions.Conjunction ();
      if (lower.HasValue) {
        result.Add (Restrictions.Disjunction ()
          .Add (Restrictions.Gt ("End", lower.Value))
          .Add (Restrictions.Conjunction ()
                .Add (Restrictions.Eq ("End", lower.Value))
                .Add (Restrictions.EqProperty ("Begin", "End"))
                )
        );
      }
      if (range.Upper.HasValue) {
        if (range.UpperInclusive) {
          result.Add (Restrictions.Le ("Begin", range.Upper.Value));
        }
        else {
          result.Add (Restrictions.Lt ("Begin", range.Upper.Value));
        }
      }
      return result;
    }

    /// <summary>
    /// Find the first n elements in the range in the specified order
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    public virtual IEnumerable<ICncValue> FindFirstOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range, int n, bool descending)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id)) // Id to use the cache
        .Add (InUtcRange (range))
        .AddOrder (descending
                   ? Order.Desc ("Begin")
                   : Order.Asc ("Begin"))
        .SetMaxResults (n)
        // Not cacheable because it may cause some problems with a partitioned table:
        // there is a risk to make NHibernate use a FindById without the secondary key
        .List<ICncValue> ();
    }

    /// <summary>
    /// Find the ICncValues for specified machineModule and field
    /// in the specified range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<ICncValue> FindOverlapsRange (IMachineModule machineModule,
      IField field,
      UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncValue> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id)) // Id to use the cache
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("Begin"))
        // Not cacheable because it may cause some problems with a partitioned table:
        // there is a risk to make NHibernate use a FindById without the secondary key
        .List<ICncValue> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in an ascending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public virtual IEnumerable<ICncValue> FindOverlapsRangeAscending (IMachineModule machineModule, IField field,
                                                                  UtcDateTimeRange range,
                                                                  TimeSpan step)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      var logger = LogManager.GetLogger (typeof (FactDAO).FullName + "." + machineModule.Id + "." + field.Id);
      var strategy = new CncValueOverlapsRangeByStepStrategy (field, step, false);
      return new FindOverlapsRangeEnumerable<ICncValue, IMachineModule> (strategy, machineModule, range, logger);
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a descending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public virtual IEnumerable<ICncValue> FindOverlapsRangeDescending (IMachineModule machineModule, IField field,
                                                                   UtcDateTimeRange range,
                                                                   TimeSpan step)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      var logger = LogManager.GetLogger (typeof (FactDAO).FullName + "." + machineModule.Id + "." + field.Id);
      var strategy = new CncValueOverlapsRangeByStepStrategy (field, step, true);
      return new FindOverlapsRangeEnumerable<ICncValue, IMachineModule> (strategy, machineModule, range, logger);
    }

    /// <summary>
    /// OverlapsRangeByStep strategy implementation for cnc value
    /// </summary>
    public class CncValueOverlapsRangeByStepStrategy
      : IFindOverlapsRangeByStepStrategy<ICncValue, IMachineModule>
    {
      readonly string INITIAL_STEP_NUMBER_KEY = "Database.CncValueDAO.FindOverlapsRangeStep.InitialStepNumber";
      readonly int INITIAL_STEP_NUMBER_DEFAULT = 4;

      readonly string LOWER_LIMIT_KEY = "Database.CncValueDAO.FindOverlapsRangeStep.LowerLimit";
      readonly LowerBound<DateTime> LOWER_LIMIT_DEFAULT = new DateTime (2016, 01, 01, 00, 00, 00, DateTimeKind.Utc);

      readonly string MAX_GAP_KEY = "Database.CncValueDAO.FindOverlapsRangeStep.MaxGap";
      readonly TimeSpan? MAX_GAP_DEFAULT = TimeSpan.FromDays (90); // 90 days

      readonly IField m_field;
      readonly TimeSpan m_step;
      readonly bool m_descending;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="field">not null</param>
      /// <param name="step"></param>
      /// <param name="descending"></param>
      public CncValueOverlapsRangeByStepStrategy (IField field, TimeSpan step, bool descending)
      {
        Debug.Assert (null != field);

        m_field = field;
        m_step = step;
        m_descending = descending;
      }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public bool Descending { get { return m_descending; } }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public UtcDateTimeRange LimitRange
      {
        get
        {
          var lower = Lemoine.Info.ConfigSet
            .LoadAndGet (LOWER_LIMIT_KEY, LOWER_LIMIT_DEFAULT);
          return new UtcDateTimeRange (lower, DateTime.UtcNow);
        }
      }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public TimeSpan? MaxGap
      {
        get
        {
          return Lemoine.Info.ConfigSet
            .LoadAndGet (MAX_GAP_KEY, MAX_GAP_DEFAULT);
        }
      }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public TimeSpan Step { get { return m_step; } }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      /// <param name="machineModule"></param>
      /// <param name="range"></param>
      /// <returns></returns>
      public IEnumerable<ICncValue> InitialRequest (IMachineModule machineModule, UtcDateTimeRange range)
      {
        var initialStepNumber = Lemoine.Info.ConfigSet
          .LoadAndGet (INITIAL_STEP_NUMBER_KEY, INITIAL_STEP_NUMBER_DEFAULT);
        return ModelDAOHelper.DAOFactory.CncValueDAO
          .FindFirstOverlapsRange (machineModule, m_field, range, initialStepNumber, m_descending);
      }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public bool MaxResultsInitial { get { return true; } }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public bool LimitRangeInitial { get { return false; } }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      /// <param name="machineModule"></param>
      /// <param name="range"></param>
      /// <returns></returns>
      public IEnumerable<ICncValue> NextRequest (IMachineModule machineModule, UtcDateTimeRange range)
      {
        var requestResult = ModelDAOHelper.DAOFactory.CncValueDAO
          .FindOverlapsRange (machineModule, m_field, range);
        if (m_descending) {
          return requestResult.Reverse ();
        }
        else {
          return requestResult;
        }
      }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public bool MaxResultsNext { get { return false; } }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public bool LimitRangeNext { get { return true; } }

      /// <summary>
      /// <see cref="IFindOverlapsRangeByStepStrategy{I, IPartitionKey}"/> 
      /// </summary>
      public bool ReadWrite { get { return false; } }
    }

  }
}
