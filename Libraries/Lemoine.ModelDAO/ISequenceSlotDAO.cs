// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ISequenceSlot.
  /// </summary>
  public interface ISequenceSlotDAO: IGenericByMachineModuleUpdateDAO<ISequenceSlot, int>
  {
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ISequenceSlot> FindOverlapsRange (IMachineModule machineModule, UtcDateTimeRange range);

    /// <summary>
    /// Find all the sequence slots at and after a given date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IList<ISequenceSlot> FindAllAtAndAfter (IMachineModule machineModule,
                                            DateTime dateTime);

    /// <summary>
    /// Find the first n sequence slots at and after a given date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    IList<ISequenceSlot> FindAllAtAndAfter (IMachineModule machineModule,
                                            DateTime dateTime,
                                            int limit);
   
    /// <summary>
    /// Find all the sequence slots whose end is after (not strictly) a specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IList<ISequenceSlot> FindAllEndFrom (IMachineModule machineModule,
                                         DateTime dateTime);

    /// <summary>
    /// Find the first sequence slot whose end is before a specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    ISequenceSlot FindFirstBefore (IMachineModule machineModule,
                                   DateTime dateTime);

    /// <summary>
    /// Find the first sequence slot whose begin is after (not strictly) a specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    ISequenceSlot FindFirstAfter (IMachineModule machineModule,
                                  DateTime dateTime);

    /// <summary>
    /// Find the last sequence slot for the specified machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    ISequenceSlot FindLast (IMachineModule machineModule);

    /// <summary>
    /// Find the last sequence slot for the specified machine module
    /// whose begin is strictly before the specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    ISequenceSlot FindLastBefore (IMachineModule machineModule,
                                  DateTime dateTime);
    
    /// <summary>
    /// Find the last sequence slot for the specified machine module
    /// which intersects the specified date/time range
    /// and whose sequence is not null
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    ISequenceSlot FindLastWithSequence (IMachineModule machineModule,
                                        UtcDateTimeRange range);

    /// <summary>
    /// Find the sequence at the specific date/time.
    /// 
    /// Keep only the sequence slot with a not null sequence.
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="at"></param>
    /// <returns></returns>
    ISequenceSlot FindAtWithSequence (IMachineModule machineModule, DateTime at);

    /// <summary>
    /// Check if it exists a sequence slot with no associated sequence
    /// for the specified machine in the specified period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    bool ExistsNoSequenceBetween (IMonitoredMachine machine,
                                  DateTime begin,
                                  DateTime end);

    /// <summary>
    /// Find all the sequence slots which NextBegin or Begin is after (not strictly) the specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IList<ISequenceSlot> FindAllNextBeginFrom (IMachineModule machineModule,
                                               DateTime dateTime);
  }
}
