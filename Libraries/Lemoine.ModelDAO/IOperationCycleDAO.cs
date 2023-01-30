// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IOperationCycle.
  /// </summary>
  public interface IOperationCycleDAO : IGenericByMachineUpdateDAO<IOperationCycle, int>
  {
    /// <summary>
    /// Check if there is no operation cycle which begin or end
    /// is strictly after the specificed date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns>true if there is no such operation cycle</returns>
    bool CheckNoOperationCycleStrictlyAfter (IMachine machine,
                                             DateTime dateTime);

    /// <summary>
    /// Get the last operation cycle for the specified machine
    /// with an eager fetch of the operation slot
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IOperationCycle GetLast (IMachine machine);

    /// <summary>
    /// Get the last full operation cycle for the specified machine
    /// 
    /// The associated operation slot is fetched in the same time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IOperationCycle GetLastFullCycle (IMachine machine);

    /// <summary>
    /// Get the n last full operation cycles for the specified machine before a specified date/time
    /// 
    /// This is sorted by descending date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="before"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    IList<IOperationCycle> GetLastFullCyclesBefore (IMachine machine, DateTime before, int quantity);

    /// <summary>
    /// Get all operation cycles that are associated to a specified operation slot
    /// </summary>
    /// <param name="operationSlot">not null</param>
    /// <returns></returns>
    IList<IOperationCycle> FindAllWithOperationSlot (IOperationSlot operationSlot);

    /// <summary>
    /// Get all the operation cycles that are associated to a specified operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    IList<IOperationCycle> FindAllWithOperation (IOperation operation);

    /// <summary>
    /// Find all the operation cycles:
    /// <item>which end is in the specified date/time range</item>
    /// <item>with no end but a begin in the specified date/time range</item>
    /// and that is either not associated to any operation slot
    /// or that is not associated to the specified operation slot
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="rangeBegin"></param>
    /// <param name="rangeEnd"></param>
    /// <param name="operationSlot"></param>
    /// <returns></returns>
    IList<IOperationCycle> FindAllInRangeExceptInSlot (IMachine machine,
                                                       DateTime rangeBegin,
                                                       DateTime? rangeEnd,
                                                       IOperationSlot operationSlot);

    /// <summary>
    /// Find all operation cycles:
    /// <item>which end is in the specified date/time range</item>
    /// <item>with no end but a begin in the specified date/time range</item>
    /// and with an eager fetch of the operation slot
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IOperationCycle> FindAllInRange (IMachine machine,
                                           UtcDateTimeRange range);

    /// <summary>
    /// Find all operation cycles:
    /// <item>which end is in the specified date/time range</item>
    /// <item>with no end but a begin in the specified date/time range</item>
    /// and with an eager fetch of the operation slot
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null</param>
    /// <returns></returns>
    IList<IOperationCycle> FindOverlapsRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find asynchronously all operation cycles:
    /// <item>which end is in the specified date/time range</item>
    /// <item>with no end but a begin in the specified date/time range</item>
    /// and with an eager fetch of the operation slot
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IList<IOperationCycle>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Get the minimum cycle end for the specified operation slot
    /// </summary>
    /// <param name="operationSlot">not null</param>
    /// <returns></returns>
    DateTime? GetMinCycleEnd (IOperationSlot operationSlot);

    /// <summary>
    /// For a given operation slot, find its last full operation cycle
    /// with no associated deliverable piece
    /// and with end date \le date
    /// </summary>
    IOperationCycle FindLastFullNotAssociated (IOperationSlot operationSlot,
                                              DateTime date);

    /// <summary>
    /// For a given operation slot, finds its operation cycles
    /// with no associated deliverable piece (sorted by descending begin date times)
    /// and with begin date \le date
    /// </summary>
    IList<IOperationCycle> FindAllNotAssociated (IOperationSlot operationSlot,
                                                DateTime date);

    /// <summary>
    /// Get the period duration for the specified operation slot
    /// to compute the average cycle time
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <returns></returns>
    TimeSpan? GetSlotPeriodDuration (IOperationSlot operationSlot);

    /// <summary>
    /// Get the cycle at a given date on a given machine:
    /// operation cycle has a begin which is lower or equal to dateTime
    /// and its end (if any) is greater than dateTime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationCycle FindAt (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Get the cycle at a given date on a given machine asynchronously:
    /// operation cycle has a begin which is lower or equal to dateTime
    /// and its end (if any) is greater than dateTime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IOperationCycle> FindAtAsync (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Get the operation cycle matching a dateTime on a given machine:
    /// operation cycle has an end which is greater or equal to dateTime
    /// and its begin (if any) is lower than dateTime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationCycle FindAtEnd (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Get the cycle with a begin date time equal to input dateTime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationCycle FindWithBeginEqualTo (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Get the cycle with an end date time equal to input dateTime
    /// with an early fetch of the operation slot
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationCycle FindWithEndEqualTo (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Get the first cycle of an operation slot
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IOperationCycle GetFirstCycle (IOperationSlot slot);

    /// <summary>
    /// Get the last cycle of an operation slot
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IOperationCycle GetLastCycle (IOperationSlot slot);

    /// <summary>
    /// Get the first cycle whose begin is before the specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationCycle GetFirstBeginBefore (IMachine machine, Bound<DateTime> dateTime);

    /// <summary>
    /// Get the first operation cycle strictly before the specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationCycle GetFirstStrictlyBefore (IMachine machine, Bound<DateTime> dateTime);

    /// <summary>
    /// Get the first operation cycle strictly after the specified date/time
    /// with an eager fetch of the operation slot
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationCycle GetFirstStrictlyAfter (IMachine machine, Bound<DateTime> dateTime);

    /// <summary>
    /// Get the first cycle whose beginning is strictly after the specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IOperationCycle GetFirstBeginAfter (IMachine machine, Bound<DateTime> dateTime);

    /// <summary>
    /// Find the first n full operation cycle in the range in the specified order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order (not implemented), True: descending order</param>
    /// <returns></returns>
    IEnumerable<IOperationCycle> FindFirstFullOverlapsRange (IMachine machine, UtcDateTimeRange range, int n, bool descending);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a descending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<IOperationCycle> FindFullOverlapsRangeDescending (IMachine machine, UtcDateTimeRange range, int step);

    /// <summary>
    /// Find the first n operation cycle in the range in the specified order
    /// 
    /// Potentially, the associated operationSlots are lazy.
    /// Make sure they are loaded with LoadOperationSlots
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order (not implemented)</param>
    /// <returns></returns>
    IEnumerable<IOperationCycle> FindFirstOverlapsRange (IMachine machine, UtcDateTimeRange range, int n, bool descending);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a ascending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// 
    /// Potentially, the associated operationSlots are lazy.
    /// Make sure they are loaded with LoadOperationSlots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<IOperationCycle> FindOverlapsRangeAscending (IMachine machine, UtcDateTimeRange range, int step);

    /// <summary>
    /// Make sure the operation slot of the specified operation cycle is loaded
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <returns>Same operation cycle</returns>
    IOperationCycle LoadOperationSlot (IOperationCycle operationCycle);

    /// <summary>
    /// Make sure the operation slots of the specified operation cycles are loaded
    /// </summary>
    /// <param name="operationCycles"></param>
    /// <returns>Same operation cycles</returns>
    IEnumerable<IOperationCycle> LoadOperationSlots (IEnumerable<IOperationCycle> operationCycles);
  }

  /// <summary>
  /// Extensions
  /// </summary>
  public static class OperationCycleDAOExtensions
  {
    /// <summary>
    /// Load the operation slot
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <returns></returns>
    public static IOperationCycle LoadOperationSlot (this IOperationCycle operationCycle)
    {
      if (null == operationCycle) {
        return operationCycle;
      }
      else {
        return ModelDAOHelper.DAOFactory.OperationCycleDAO.LoadOperationSlot (operationCycle);
      }
    }

    /// <summary>
    /// Load the operation slots
    /// </summary>
    /// <param name="operationCycles"></param>
    /// <returns></returns>
    public static IEnumerable<IOperationCycle> LoadOperationSlots (this IEnumerable<IOperationCycle> operationCycles)
    {
      return ModelDAOHelper.DAOFactory.OperationCycleDAO.LoadOperationSlots (operationCycles);
    }
  }
}
