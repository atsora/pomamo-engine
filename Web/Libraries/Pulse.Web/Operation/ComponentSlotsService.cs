// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Collections;
using Lemoine.Web;
using Pulse.Extensions.Web.Responses;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Description of ComponentSlotsService
  /// </summary>
  public class ComponentSlotsService
    : GenericCachedService<ComponentSlotsRequestDTO>
  {
    static readonly string MINIMUM_DATETIME_KEY = "MinimumDateTime";
    static readonly DateTime MINIMUM_DATETIME_DEFAULT = new DateTime (2015, 1, 1, 00, 00, 00, DateTimeKind.Utc);

    static readonly string MINIMUM_GAP_KEY = "Web.Operation.ComponentSlots.MinimumGap";
    static readonly TimeSpan MINIMUM_GAP_DEFAULT = TimeSpan.FromMinutes (15);

    static readonly ILog log = LogManager.GetLogger (typeof (ComponentSlotsService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ComponentSlotsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, ComponentSlotsRequestDTO requestDTO)
    {
      UtcDateTimeRange range = ParseRange (requestDTO.Range);

      TimeSpan cacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          cacheTimeSpan = CacheTimeOut.OldShort.GetTimeSpan ();
        }
        else { // Past
          cacheTimeSpan = CacheTimeOut.PastShort.GetTimeSpan ();
        }
      }
      else { // Current or future
        cacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      log.DebugFormat ("GetCacheTimeOut: " +
                       "cacheTimeSpan is {0} for url={1}",
                       cacheTimeSpan, url);
      return cacheTimeSpan;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (ComponentSlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        UtcDateTimeRange range = ParseRange (request.Range);

        if ((request.WorkOrderId <= 0) && (request.ProjectId <= 0)) {
          log.ErrorFormat ("GetWithoutCache: WorkOrderId or ProjectId parameter is mandatory");
          return new ErrorDTO ("One of the two parameters WorkOrderId and ProjectId is mandatory", ErrorStatus.WrongRequestParameter);
        }

        IJob job = null;
        using (var transaction = session.BeginReadOnlyTransaction ("Web.Operation.ComponentSlots.Job")) {
          if (0 < request.WorkOrderId) {
            job = ModelDAOHelper.DAOFactory.JobDAO.FindByWorkOrderId (request.WorkOrderId);
            if (null == job) {
              log.ErrorFormat ("GetWithoutCache: no job with work order id {0}", request.WorkOrderId);
              return new ErrorDTO ("No job with the specified work order id", ErrorStatus.WrongRequestParameter);
            }
          }
          if (0 < request.ProjectId) {
            if (null != job) {
              if (job.ProjectId != request.ProjectId) {
                log.ErrorFormat ("GetWithoutCache: ProjectId {0} and WorkOrderId {1} are incompatible", request.ProjectId, request.WorkOrderId);
                return new ErrorDTO ("ProjectId and WorkOrderId ad incompatible", ErrorStatus.WrongRequestParameter);
              }
            }
            else {
              job = ModelDAOHelper.DAOFactory.JobDAO.FindByProjectId (request.ProjectId);
              if (null == job) {
                log.ErrorFormat ("GetWithoutCache: no job with project id {0}", request.ProjectId);
                return new ErrorDTO ("No job with the specified project id", ErrorStatus.WrongRequestParameter);
              }
            }
          }
          Debug.Assert (null != job);
        }

        return GetAutoSplit (job, range, request.MachineIds); // Less than a week
      }
    }

    IEnumerable<IOperationSlot> GetOperationSlots (IJob job, IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != job);
      Debug.Assert (null != job.WorkOrder);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Web.Operation.ComponentSlots.GetOperationSlots")) {
          var slots = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRangeWithWorkOrder (machine, job.WorkOrder, range);

          // Remove some items at the end if they are not really effective
          int numberToRemove = 0;
          {
            DateTime now = DateTime.UtcNow;
            IOperationSlot firstNotInFuture = null;
            int pendingCounter = 0;
            foreach (var slot in slots.Reverse ()) {
              if (Bound.Compare<DateTime> (now, slot.BeginDateTime) <= 0) { // In the future => remove
                ++numberToRemove;
              }
              else { // Not completely in the future
                if (null != slot.Operation) { // An operation ! Keep them now
                  if (0 < pendingCounter) {
                    // Is there an operation between the slot and now ?
                    if (!ModelDAOHelper.DAOFactory.OperationSlotDAO
                        .ExistsDifferentOperationBetween (machine, slot.EndDateTime.Value, now, null)) {
                      numberToRemove += pendingCounter;
                    }
                  }
                  break;
                }
                if (null == firstNotInFuture) {
                  firstNotInFuture = slot;
                }
                Debug.Assert (null != firstNotInFuture);
                Debug.Assert (null == slot.Operation);
                if (firstNotInFuture.EndDateTime.HasValue
                    && !firstNotInFuture.DateTimeRange.ContainsElement (now)) {
                  ++pendingCounter;
                }
                else {
                  ++numberToRemove;
                }
              }
            }
          }

          return slots.Take (slots.Count - numberToRemove);
        }
      }
    }

    ComponentSlotsResponseDTO GetAutoSplit (IJob job, UtcDateTimeRange requestedRange, IList<int> machineIds)
    {
      var result = new ComponentSlotsResponseDTO ();
      result.RequestedRange = requestedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.ProjectId = job.ProjectId;
      result.WorkOrderId = job.WorkOrderId;
      result.ProjectDisplay = job.Project.Display;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IEnumerable<IMachine> machines;
        using (var transaction = session.BeginReadOnlyTransaction ("Web.Operation.ComponentSlots.Machines")) {
          machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll ();
        }
        if ( (null != machineIds) && machineIds.Any ()) {
          machines = machines.Where (m => machineIds.Contains (m.Id));
        }

        IEnumerable<IOperationSlot> slots = new List<IOperationSlot> ();
        foreach (var machine in machines) {
          slots = slots.Union (GetOperationSlots (job, machine, requestedRange));
        }

        slots = slots
          .Where (s => null != s.WorkOrder)
          .Where (s => null != s.Component);

        if (!slots.Any ()) {
          var r = new UtcDateTimeRange ();
          result.Range = r.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          return result;
        }
        var range = GroupRanges (slots.Select (s => s.DateTimeRange));
        result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
        var minimumDateTime = Lemoine.Info.ConfigSet.LoadAndGet<DateTime> (MINIMUM_DATETIME_KEY, MINIMUM_DATETIME_DEFAULT);
        var limitedRange = new UtcDateTimeRange (range.Intersects (new UtcDateTimeRange (minimumDateTime, DateTime.UtcNow)));
        result.LimitedRange = limitedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
        Debug.Assert (limitedRange.Duration.HasValue);
        var minimumGap = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (MINIMUM_GAP_KEY, MINIMUM_GAP_DEFAULT);

        var groupedByComponent = slots.GroupBy (s => s.Component);
        foreach (var group in groupedByComponent) {
          var component = group.Key;
          Debug.Assert (null != component);
          var componentSlots = new ComponentSlotsDTO (component);
          componentSlots.BgColor = ColorGenerator.GetColor ("Component", ((IDataWithId)component).Id);
          componentSlots.FgColor = ColorGenerator.GetContrastColor (componentSlots.BgColor);
          var impactedMachines = group.Select (s => s.Machine).Distinct ();
          componentSlots.Machines = impactedMachines.Select (m => new MachineDTO (m)).ToList ();
          componentSlots.MachineIds = string.Join (",", impactedMachines.Select (m => m.Id.ToString ()).ToArray ());
          result.Components.Add (componentSlots);

          IList<IOperationSlot> pending = new List<IOperationSlot> ();
          UtcDateTimeRange pendingRange = new UtcDateTimeRange ();
          foreach (var slot in group.OrderBy (s => s.DateTimeRange)) {
            UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange
                                                               .Intersects (limitedRange));
            Debug.Assert (!slotRange.IsEmpty ());
            Debug.Assert (slotRange.Duration.HasValue);

            if (!pendingRange.IsEmpty ()
                && !slotRange.Overlaps (pendingRange)
                && pendingRange.Upper.HasValue
                && slotRange.Lower.HasValue
                && (minimumGap < slotRange.Lower.Value.Subtract (pendingRange.Upper.Value))) {
              // Minimum gap reached
              PushBlock (componentSlots, pending);
              pending.Clear ();
            }

            pending.Add (slot);
            pendingRange = GroupRanges (pendingRange, slotRange);
          }
          PushBlock (componentSlots, pending);
        }

        return result;
      }
    }

    void PushBlock (ComponentSlotsDTO response, IList<IOperationSlot> slots)
    {
      if (!slots.Any ()) {
        return;
      }

      var workOrder = slots.First ().WorkOrder;
      Debug.Assert (null != workOrder);

      var range = GroupRanges (slots.Select (s => s.DateTimeRange));
      var block = new ComponentSlotBlockDTO (range
        .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound)));
      var machines = slots.Select (s => s.Machine).Distinct ();
      block.Machines = machines.Select (m => new MachineDTO (m)).ToList ();
      block.MachineIds = string.Join (",", machines.Select (m => m.Id.ToString ()).ToArray ());

      response.Blocks.Add (block);
    }

    UtcDateTimeRange GroupRanges (params UtcDateTimeRange[] ranges)
    {
      IEnumerable<UtcDateTimeRange> r = ranges;
      return GroupRanges (r);
    }

    UtcDateTimeRange GroupRanges (IEnumerable<UtcDateTimeRange> ranges)
    {
      var notEmptyRanges = ranges.Where (r => !r.IsEmpty ());
      if (!notEmptyRanges.Any ()) {
        return new UtcDateTimeRange ();
      }
      else {
        var first = notEmptyRanges.OrderBy (r => r.Lower.HasValue ? r.Lower.Value.ToOADate () : double.MinValue).First ();
        var last = notEmptyRanges.OrderByDescending (r => r.Upper.HasValue ? r.Upper.Value.ToOADate () : double.MaxValue).First ();
        return new UtcDateTimeRange (first.Lower,
                                     last.Upper,
                                     first.LowerInclusive,
                                     last.UpperInclusive);
      }
    }

    UtcDateTimeRange ParseRange (string range)
    {
      if (string.IsNullOrEmpty (range)) {
        return GetDefaultRange ();
      }
      else if (range.Equals ("CurrentShift")) {
        return GetCurrentShift ();
      }
      else {
        return new UtcDateTimeRange (range);
      }
    }

    /// <summary>
    /// Return the default range: the current day
    /// </summary>
    /// <returns></returns>
    UtcDateTimeRange GetDefaultRange ()
    {
      var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
      var daySlot = Lemoine.Business.ServiceProvider
        .Get (dayAtRequest);
      if (daySlot is null) {
        log.Fatal ($"GetDefaultRange: no day slot now");
        return new UtcDateTimeRange ();
      }
      return daySlot.DateTimeRange;
    }

    /// <summary>
    /// Get the current shift range
    /// </summary>
    /// <returns></returns>
    UtcDateTimeRange GetCurrentShift ()
    {
      var currentShiftRequest = new Lemoine.Business.Shift.CurrentShift ();
      var currentShift = Lemoine.Business.ServiceProvider
        .Get (currentShiftRequest);
      if (currentShift is null) {
        var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
        var daySlot = Lemoine.Business.ServiceProvider
          .Get (dayAtRequest);
        if (daySlot is null) {
          log.Fatal ($"GetCurrentShift: no day slot now");
          return new UtcDateTimeRange ();
        }
        return daySlot.DateTimeRange;
      }
      else {
        return currentShift.DateTimeRange;
      }
    }
    #endregion // Methods
  }
}
