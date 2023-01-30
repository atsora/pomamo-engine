// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IObservationStateSlot.
  /// </summary>
  public interface IObservationStateSlotDAO
    : IGenericByMachineUpdateDAO<IObservationStateSlot, int>
    , IMachineSlotDAO<IObservationStateSlot>
  {
    /// <summary>
    /// Find the observation state slots that match a day/shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day">DateTimeKind.Unspecified</param>
    /// <param name="shift">not null</param>
    /// <returns></returns>
    IList<IObservationStateSlot> FindWith (IMachine machine, DateTime day, IShift shift);

    /// <summary>
    /// Find the observation state slots that match a day/shift asynchronously
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day">DateTimeKind.Unspecified</param>
    /// <param name="shift">not null</param>
    /// <returns></returns>
    Task<IList<IObservationStateSlot>> FindWithAsync (IMachine machine, DateTime day, IShift shift);

    /// <summary>
    /// Get the list of slots in a given time range.
    /// 
    /// The list is ordered by ascending Begin
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IList<IObservationStateSlot> GetListInRange (IMachine machine,
                                                 DateTime begin,
                                                 DateTime end);

    /// <summary>
    /// Find all the observation slots in a specified UTC date/time range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    [Obsolete ("Use FindOverlapsRange instead")]
    IList<IObservationStateSlot> FindAllInUtcRange (IMachine machine,
                                                    UtcDateTimeRange range);

    /// <summary>
    /// Find all the machine observation state slots in a specified UTC date/time range
    /// with the specified properties.
    /// 
    /// If one property is null, it must be null in the database as well
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="shift"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    IList<IObservationStateSlot> FindAllInUtcRangeWith (IMachine machine,
                                                        UtcDateTimeRange range,
                                                        IMachineStateTemplate machineStateTemplate,
                                                        IMachineObservationState machineObservationState,
                                                        IShift shift,
                                                        IUser user);

    /// <summary>
    /// Find all the machine observation state slots that match a machine state template association.
    /// It means:
    /// <item>same machine state template</item>
    /// <item>if the shift or user is set, it must match</item>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="shift"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    IList<IObservationStateSlot> FindMatchingMachineStateTemplateAssociation (IMachine machine,
                                                                              UtcDateTimeRange range,
                                                                              IMachineStateTemplate machineStateTemplate,
                                                                              IShift shift,
                                                                              IUser user);

    /// <summary>
    /// Find all the observation slots in a specified day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IObservationStateSlot> FindAllInDayRange (IMachine machine,
                                                    DayRange range);

    /// <summary>
    /// Find all the observation state slots that matches a given user in a specified UTC date/time range
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IObservationStateSlot> FindByUserInRange (IUser user,
                                                    UtcDateTimeRange range);

    /// <summary>
    /// Get the impacted observation state slots by a user site attendance change
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newSiteAttendance"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IObservationStateSlot> GetAttendanceChangeImpacts (IUser user, bool newSiteAttendance, UtcDateTimeRange range);

    /// <summary>
    /// Find all the observation state slots with no machine observation state
    /// that intersect the specified date/range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IObservationStateSlot> FindWithNoMachineObservationState (IMachine machine,
                                                                    UtcDateTimeRange range);

    /// <summary>
    /// Find all the observation state slots with no machine observation state
    /// that intersect the specified date/range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="utcBegin">in UTC</param>
    /// <param name="utcEnd">in UTC</param>
    /// <param name="limit">maximum number of items to take</param>
    /// <returns></returns>
    IEnumerable<IObservationStateSlot> FindWithNoMachineObservationStateInRange (IMachine machine,
                                                                           DateTime utcBegin,
                                                                           DateTime utcEnd,
                                                                           int limit);

    /// <summary>
    /// Find all the observation state slots with no machine observation state
    /// that begin before the specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="until">in UTC</param>
    /// <param name="limit">maximum number of items to take</param>
    /// <returns></returns>
    IEnumerable<IObservationStateSlot> FindWithNoMachineObservationState (IMachine machine,
                                                                    DateTime until,
                                                                    int limit);

    /// <summary>
    /// Get last MachineObservationStateSlot for a given machine in a date range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IObservationStateSlot GetLastIntersectWithRange (IMachine machine, DateTime begin, DateTime end);

    /// <summary>
    /// Get the production duration in a specific date/time range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    TimeSpan GetProductionDuration (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the observation state slots (on different machines)
    /// at a specific date/time with the specified machine state template
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    IList<IObservationStateSlot> FindAt (IMachineStateTemplate machineStateTemplate, DateTime at);

    /// <summary>
    /// Find all the observation state slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    IEnumerable<IObservationStateSlot> FindOverlapsRangeMatchingMachineStateTemplate (IMachine machine,
                                                                                      UtcDateTimeRange range,
                                                                                      IMachineStateTemplate machineStateTemplate);

    /// <summary>
    /// Find the slot with the specified end date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IObservationStateSlot FindWithEnd (IMachine machine,
      DateTime end);

    /// <summary>
    /// Find the slot with the specified end date/time asynchronously
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IObservationStateSlot> FindWithEndAsync (IMachine machine,
      DateTime end);
  }
}
