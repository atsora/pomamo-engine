// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.Plugin.CncSummaryByStateSlot2
{
  /// <summary>
  /// Description of CncExtension.
  /// Entry point in ImportCncDataFromQueue, Lemoine.Cnc.DataImport
  /// </summary>
  public class CncExtension
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , Lemoine.Extensions.Cnc.IImportCncValuesExtension
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncExtension).FullName);
    
    IMachineModule m_machineModule;
    readonly IDictionary<int, CncByMachineModuleField> m_fieldImplementation
      = new Dictionary<int, CncByMachineModuleField> ();

    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    public CncExtension ()
      : base (new ConfigurationLoader ())
    {
    }
    #endregion // Constructor
    
    #region ICncExtension implementation
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machineModule">not null</param>
    public bool Initialize (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      
      m_machineModule = machineModule;
      
      // Get all field Ids
      var fieldIds = new HashSet<int>();
      var configurations = LoadConfigurations ();
      foreach (var configuration in configurations) {
        foreach (var cncFieldId in configuration.CncFieldIds) {
          fieldIds.Add (cncFieldId);
        }
      }
      
      // Initialize m_fieldImplementation
      foreach (var fieldId in fieldIds) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("CncSummaryByStateSlot2.CncExtension.Initialize")) {
            IField field = ModelDAOHelper.DAOFactory.FieldDAO.FindById (fieldId);
            if (null != field) {
              m_fieldImplementation [fieldId] = new CncByMachineModuleField (machineModule, field);
            }
          }
        }
      }

      return fieldIds.Any ();
    }
    
    /// <summary>
    /// Before processing a new Cnc value
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="value">int, double or string</param>
    /// <param name="dateTime"></param>
    public void BeforeCncValueProcessing (IMachineModule machineModule, IField field, object value, DateTime dateTime)
    { }

    /// <summary>
    /// Process a new Cnc value
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="value">int, double or string</param>
    /// <param name="range"></param>
    public void BeforeAddingNewAverageValue (IMachineModule machineModule, IField field, object value, UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (machineModule.Id == m_machineModule.Id);
      Debug.Assert (null != field);
      
      if (range.IsEmpty ()) { // Nothing to do
        return;
      }
      
      if (value is double) {
        CncByMachineModuleField implementation;
        if (m_fieldImplementation.TryGetValue (field.Id, out implementation)) {
          implementation.Process ((double) value, range);
        }
      }
    }
    
    /// <summary>
    /// Process a new Cnc value
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="range"></param>
    public void AfterCncValueProcessing (IMachineModule machineModule, IField field, UtcDateTimeRange range)
    { }
    
    /// <summary>
    /// Run before a commit
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    public void BeforeCncValueCommit (IMachineModule machineModule, IField field)
    {
      Debug.Assert (machineModule.Id == m_machineModule.Id);

      CncByMachineModuleField implementation;
      if (m_fieldImplementation.TryGetValue (field.Id, out implementation)) {
        implementation.Store ();
      }
    }
    
    /// <summary>
    /// Run after a rollback
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <param name="fieldId"></param>
    public void AfterCncValueRollback (int machineModuleId, int fieldId)
    {
      Debug.Assert (machineModuleId == m_machineModule.Id);

      CncByMachineModuleField implementation;
      if (m_fieldImplementation.TryGetValue (fieldId, out implementation)) {
        implementation.CleanCache ();
      }
    }
    #endregion // ICncExtension implementation
    
    #region Private methods
    #endregion // Private methods
  }
}
