// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// Description of IExtensionCncValue.
  /// Entry point in ImportCncDataFromQueue, Lemoine.Cnc.DataImport
  /// </summary>
  public interface IImportCncValuesExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machineModule"></param>
    bool Initialize (IMachineModule machineModule);
    
    /// <summary>
    /// Run before a new cnc value is processed
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    void BeforeCncValueProcessing (IMachineModule machineModule, IField field, object value, DateTime dateTime);
    
    /// <summary>
    /// Run before storing a current average value
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="range"></param>
    void BeforeAddingNewAverageValue (IMachineModule machineModule, IField field, object value, UtcDateTimeRange range);
    
    /// <summary>
    /// Process a new Cnc value
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="range"></param>
    void AfterCncValueProcessing (IMachineModule machineModule, IField field, UtcDateTimeRange range);
    
    /// <summary>
    /// Run before a commit
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    void BeforeCncValueCommit (IMachineModule machineModule, IField field);
    
    /// <summary>
    /// Run after a rollback
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <param name="fieldId"></param>
    void AfterCncValueRollback (int machineModuleId, int fieldId);
  }
}
