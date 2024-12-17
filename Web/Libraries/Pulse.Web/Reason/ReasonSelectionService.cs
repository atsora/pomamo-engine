// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Database;
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Web;
using Pulse.Extensions.Database;

namespace Pulse.Web.Reason
{
  internal class ReasonSelectionKey : IEquatable<ReasonSelectionKey>
  {
    public IMachineMode MachineMode { get; set; }
    public IMachineObservationState MachineObservationState { get; set; }
    public ReasonSelectionKey (IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      this.MachineMode = machineMode;
      this.MachineObservationState = machineObservationState;
    }

    #region IEquatable implementation
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (ReasonSelectionKey other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ReasonSelectionKey other = obj as ReasonSelectionKey;
      if (null == other) {
        return false;
      }
      return (object.Equals (this.MachineMode, other.MachineMode)
              && object.Equals (this.MachineObservationState, other.MachineObservationState));
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * this.MachineMode.GetHashCode ();
        hashCode += 1000000009 * this.MachineObservationState.GetHashCode ();
      }
      return hashCode;
    }
    #endregion // IEquatable implementation
  }

  /// <summary>
  /// Description of ReasonSelectionService
  /// </summary>
  public class ReasonSelectionService
    : GenericCachedService<ReasonSelectionRequestDTO>
    , IBodySupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSelectionService).FullName);

    Stream m_body;

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ReasonSelectionService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.PastLong)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (ReasonSelectionRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.Error ($"GetWithoutCache: unknown machine with ID {machineId}");
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        UtcDateTimeRange range;
        if (string.IsNullOrEmpty (request.Range)) {
          range = new UtcDateTimeRange (DateTime.UtcNow, DateTime.UtcNow, "[]");
        }
        else {
          range = new UtcDateTimeRange (request.Range);
        }

        var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindOverlapsRange (machine, range);
        if (!reasonSlots.Any ()) {
          log.Error ($"GetWithoutCache: no reason slot at {range}");
          return new ErrorDTO ("No reason slot at the specified range",
            ErrorStatus.ProcessingDelay);
        }
        var reasonSelectionKeys = reasonSlots
          .Select (reasonSlot => new ReasonSelectionKey (reasonSlot.MachineMode, reasonSlot.MachineObservationState))
          .Distinct ();

        return BuildResponseDTO (machine, reasonSelectionKeys, reasonSlots);
      }
    }

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Post (ReasonSelectionPostRequestDTO request)
    {
      int machineId = request.MachineId;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.ReasonSelection")) {
        var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (machineId);
        if (machine is null) {
          log.Error ($"Post: unknown machine with ID {machineId}");
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        // 2- Get data (= one period or more )
        // Ranges
        RangesPostDTO deserializedResult = PostDTO.Deserialize<RangesPostDTO> (m_body);
        IEnumerable<IReasonSlot> reasonSlots = new List<IReasonSlot> ();
        IEnumerable<ReasonSelectionKey> reasonSelectionKeys = new List<ReasonSelectionKey> ();
        foreach (var range in deserializedResult.ExtractRanges ()) {
          // TODO: use an asynchronous method for FindOverlapsRange
          var newReasonSlots = await Task.Run ( () => ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, range));
          reasonSlots = reasonSlots.Union (newReasonSlots);
          var newReasonSelectionKeys = newReasonSlots
            .Select (reasonSlot => new ReasonSelectionKey (reasonSlot.MachineMode, reasonSlot.MachineObservationState))
            .Distinct ();
          reasonSelectionKeys = reasonSelectionKeys.Union (newReasonSelectionKeys);
        }

        return BuildResponseDTO (machine, reasonSelectionKeys, reasonSlots);
      }
    }

    object BuildResponseDTO (IMonitoredMachine machine, IEnumerable<ReasonSelectionKey> reasonSelectionKeys, IEnumerable<IReasonSlot> reasonSlots)
    {
      var reasonSelectionEqualityComparer = new ReasonSelectionReasonEqualityComparer ();

      var isExtraAutoReasons = reasonSlots.Any (s => IsExtraAutoReasons (s));

      IEnumerable<IReasonSelection> reasonSelections = null;
      foreach (var reasonSlot in reasonSlots) {
        var nextExtraReasonSelections = GetReasonSelections (machine, reasonSlot, isExtraAutoReasons)
          .Where (s => reasonSlot.ReasonScore <= s.ReasonScore);
        if (null == reasonSelections) {
          reasonSelections = nextExtraReasonSelections;
        }
        else { // Filter !
          reasonSelections = reasonSelections
            .Intersect (nextExtraReasonSelections, reasonSelectionEqualityComparer)
            .GroupSameReason ();
        }
      }

      // End
      if (null != reasonSelections) {
        reasonSelections = reasonSelections
          .OrderBy (x => x.Reason);
        return new ReasonSelectionResponseDTOAssembler ().Assemble (reasonSelections);
      }
      else {
        return new List<ReasonSelectionResponseDTO> ();
      }
    }
    #endregion // Methods

    bool IsExtraAutoReasons (IReasonSlot reasonSlot)
    {
      if (reasonSlot.ReasonSource.HasFlag (ReasonSource.UnsafeAutoReasonNumber)) {
        return true;
      }
      else {
        if (reasonSlot.ReasonSource.IsAuto ()) {
          return (1 < reasonSlot.AutoReasonNumber);
        }
        else {
          return (0 < reasonSlot.AutoReasonNumber);
        }
      }
    }

    IEnumerable<IReasonSelection> GetReasonSelections (IMonitoredMachine monitoredMachine, IReasonSlot reasonSlot, bool includeExtraAutoReasons)
    {
      IEnumerable<IReasonSelection> result = new List<IReasonSelection> ();
      var reasonSelectionExtensions = GetReasonSelectionExtensions (monitoredMachine);
      foreach (var reasonSelectionExtension in reasonSelectionExtensions) {
        var extraReasonSelections = reasonSelectionExtension
          .GetReasonSelections (reasonSlot.DateTimeRange, reasonSlot.MachineMode, reasonSlot.MachineObservationState, includeExtraAutoReasons);
        result = result
        .Union (extraReasonSelections, new ReasonSelectionReasonEqualityComparer ());
      }
      return result;
    }

    IEnumerable<IReasonSelectionExtension> GetReasonSelectionExtensions (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      return Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.MonitoredMachineExtensions<IReasonSelectionExtension> (machine, (x, m) => x.Initialize (m)));
    }

    #region IBodySupport
    /// <summary>
    /// <see cref="IBodySupport"/>
    /// </summary>
    /// <param name="body"></param>
    public void SetBody (Stream body)
    {
      m_body = body;
    }
    #endregion // IBodySupport
  }
  }
