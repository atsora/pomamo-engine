// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Cnc value
  ///
  /// It contains only different kind of data that is received directly from the Cnc. The description of these data is in the Field table.
  ///
  /// It contains now only the data from the machine and its past data can't be modified.
  /// Only the current data can be modified and only from some data coming from the CNC.
  ///
  /// To read the cnc values from both cncvalue and cncdataperiod, use <see cref="ICncValueData"/> and ICncValueDataDAO
  /// </summary>
  public interface ICncValue
    : ICncValueData
    , IVersionable<long>
    , IPartitionedByMachineModule
    , Lemoine.Model.ISerializableModel
    , IWithDateTimeRange
  {
    /// <summary>
    /// Id
    /// </summary>
    new long Id { get; }

    /// <summary>
    /// Associated machine module
    /// </summary>
    new IMachineModule MachineModule { get; }

    /// <summary>
    /// End UTC date/time
    /// </summary>
    new DateTime End { get; set; }

    /// <summary>
    /// String value in case the corresponding Field refers to a String
    /// </summary>
    new string String { get; set; }

    /// <summary>
    /// Int value in case the corresponding Field refers to an Int32
    /// </summary>
    new Nullable<int> Int { get; set; }

    /// <summary>
    /// Double or average value in case the corresponding Field refers to a Double
    /// </summary>
    new Nullable<double> Double { get; set; }

    /// <summary>
    /// String, Int or Double value according to the Type property of the field
    /// </summary>
    new object Value { get; set; }

    /// <summary>
    /// Standard Deviation in case the aggregation type is Average
    /// </summary>
    new Nullable<double> Deviation { get; set; }

    /// <summary>
    /// Was the Cnc value interrupted ?
    /// </summary>
    new bool Stopped { get; set; }
  }
}
