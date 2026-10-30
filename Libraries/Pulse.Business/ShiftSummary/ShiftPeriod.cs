// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.ShiftSummary
{
  /// <summary>
  /// Day, shift and range a shift summary applies to, for a machine
  ///
  /// The shift of a machine is the one of its observation state slots
  /// (<see cref="Lemoine.Business.MachineState.MachineShiftSlotDAO"/>), the one the
  /// ShiftStart and ShiftEnd frequencies of the operator tasks consider too.
  /// When the machine has no shift, the period is the whole day
  /// </summary>
  [Serializable]
  public sealed class ShiftPeriod
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftPeriod).FullName);

    /// <summary>
    /// Maximum number of empty machine shift slots that are skipped to get a previous shift
    /// </summary>
    const int MAX_PREVIOUS_ATTEMPTS = 20;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="day">DateTimeKind.Unspecified</param>
    /// <param name="shiftId">null: whole day</param>
    /// <param name="shiftDisplay">nullable</param>
    /// <param name="range">not null</param>
    public ShiftPeriod (DateTime day, int? shiftId, string shiftDisplay, UtcDateTimeRange range)
    {
      Debug.Assert (range != null);

      this.Day = DateTime.SpecifyKind (day.Date, DateTimeKind.Unspecified);
      this.ShiftId = shiftId;
      this.ShiftDisplay = shiftDisplay;
      this.Range = range;
    }

    /// <summary>
    /// Day
    /// </summary>
    public DateTime Day { get; }

    /// <summary>
    /// Shift id, null if the period is the whole day
    /// </summary>
    public int? ShiftId { get; }

    /// <summary>
    /// Display of the shift, nullable
    /// </summary>
    public string ShiftDisplay { get; }

    /// <summary>
    /// UTC range of the period
    /// </summary>
    public UtcDateTimeRange Range { get; }

    /// <summary>
    /// Is the period over at the specified date/time?
    /// </summary>
    /// <param name="dateTime">UTC</param>
    /// <returns></returns>
    public bool IsCompleted (DateTime dateTime) =>
      this.Range.Upper.HasValue && (this.Range.Upper.Value <= dateTime);

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[ShiftPeriod {this.Day:yyyy-MM-dd} {this.ShiftId} {this.Range}]";

    /// <summary>
    /// Get the period of a day and a shift
    ///
    /// To be run inside a session
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift">nullable: whole day</param>
    /// <returns>null if the shift is not defined that day on the machine</returns>
    public static async Task<ShiftPeriod> FindAsync (IMachine machine, DateTime day, IShift shift)
    {
      Debug.Assert (machine != null);

      day = DateTime.SpecifyKind (day.Date, DateTimeKind.Unspecified);
      if (shift is null) {
        var daySlot = await ModelDAOHelper.DAOFactory.DaySlotDAO.FindByDayAsync (day);
        if (daySlot is null) {
          log.Error ($"FindAsync: no day slot for {day}");
          return null;
        }
        return new ShiftPeriod (day, null, null, daySlot.DateTimeRange);
      }
      else {
        var slot = await new Lemoine.Business.MachineState.MachineShiftSlotDAO ()
          .FindWithAsync (machine, day, shift);
        if (slot is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"FindAsync: shift {shift.Id} is not defined on {day} for machine {machine.Id}");
          }
          return null;
        }
        return new ShiftPeriod (day, shift.Id, shift.Display, slot.DateTimeRange);
      }
    }

    /// <summary>
    /// Get the period at a date/time: the shift of the machine, else the day
    ///
    /// To be run inside a session
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">UTC</param>
    /// <returns>nullable</returns>
    public static async Task<ShiftPeriod> FindAtAsync (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (machine != null);

      var slot = await new Lemoine.Business.MachineState.MachineShiftSlotDAO ()
        .FindAtAsync (machine, dateTime);
      if (slot?.Shift != null && slot.DayRange.Lower.HasValue) {
        return new ShiftPeriod (slot.DayRange.Lower.Value, slot.Shift.Id, slot.Shift.Display, slot.DateTimeRange);
      }
      var daySlot = await ModelDAOHelper.DAOFactory.DaySlotDAO.FindAtAsync (new LowerBound<DateTime> (dateTime));
      if (daySlot?.Day is null) {
        log.Error ($"FindAtAsync: no day at {dateTime}");
        return null;
      }
      return new ShiftPeriod (daySlot.Day.Value, null, null, daySlot.DateTimeRange);
    }

    /// <summary>
    /// Get the period that precedes another one, skipping the periods with no shift
    /// when the machine has shifts
    ///
    /// To be run inside a session
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="period">not null</param>
    /// <returns>nullable</returns>
    public static async Task<ShiftPeriod> FindPreviousAsync (IMachine machine, ShiftPeriod period)
    {
      Debug.Assert (machine != null);
      Debug.Assert (period != null);

      if (!period.Range.Lower.HasValue) {
        log.Error ($"FindPreviousAsync: period {period} with no start");
        return null;
      }

      var at = period.Range.Lower.Value.AddSeconds (-1);
      if (!period.ShiftId.HasValue) {
        return await FindAtAsync (machine, at);
      }
      var machineShiftSlotDAO = new Lemoine.Business.MachineState.MachineShiftSlotDAO ();
      for (int i = 0; i < MAX_PREVIOUS_ATTEMPTS; ++i) {
        var slot = await machineShiftSlotDAO.FindAtAsync (machine, at);
        if (slot is null || !slot.DateTimeRange.Lower.HasValue) {
          if (log.IsDebugEnabled) {
            log.Debug ($"FindPreviousAsync: no machine shift slot at {at}");
          }
          return null;
        }
        if (slot.Shift != null && slot.DayRange.Lower.HasValue) {
          return new ShiftPeriod (slot.DayRange.Lower.Value, slot.Shift.Id, slot.Shift.Display, slot.DateTimeRange);
        }
        at = slot.DateTimeRange.Lower.Value.AddSeconds (-1);
      }
      log.Warn ($"FindPreviousAsync: no previous shift found before {period}");
      return null;
    }
  }
}
