// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonBetweenComponents
{
  /// <summary>
  /// Detect stops between cycles and apply an autoreason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    static readonly string PREVIOUS_COMPONENT_ID = "PreviousComponentID";
    static readonly string PREVIOUS_COMPONENT_DETAILS = "PreviousComponentDetails";
    static readonly string PREVIOUS_SLOT_END = "PreviousSlotEnd";
    int m_previousComponentId = -1;
    string m_previousComponentDetails = "";
    DateTime? m_previousSlotEnd = null;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.BetweenComponents")
    {
    }

    /// <summary>
    /// Additional configuration of the plugin that comes from the database or from the plugin configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      // Previous state
      var previousComponent = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (this.Machine, GetKey (PREVIOUS_COMPONENT_ID));
      m_previousComponentId = (previousComponent != null) ? (int)previousComponent.Value : -1;

      var previousComponentDetails = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (this.Machine, GetKey (PREVIOUS_COMPONENT_DETAILS));
      m_previousComponentDetails = (previousComponentDetails != null ? (string)previousComponentDetails.Value : "");

      var previousSlotEnd = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (this.Machine, GetKey (PREVIOUS_SLOT_END));
      m_previousSlotEnd = (previousSlotEnd != null) ? (DateTime)previousSlotEnd.Value : (DateTime?)null;

      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    protected override void Check ()
    {
      // Limit for the detection
      var detectionDateTime = ServiceProvider.Get<DateTime?> (new Lemoine.Business.Operation.OperationDetectionStatus (this.Machine));
      if (!detectionDateTime.HasValue) {
        GetLogger ().InfoFormat ("Run: no operation detection status, stop here");
        return;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.BetweenComponents.Check")) {

          // Find the next operation slot
          var nextOs = ModelDAOHelper.DAOFactory.OperationSlotDAO.GetFirstBeginStrictlyBetween (Machine, this.DateTime, detectionDateTime.Value);
          if (nextOs == null) {
            GetLogger ().InfoFormat ("Run: no new operation slot, stop here");
            transaction.Commit ();
            return;
          }

          if (!nextOs.DateTimeRange.Lower.HasValue) {
            // Skip wrong data
            GetLogger ().ErrorFormat ("Run: operation slot with id {0} has no lower bound, weird", nextOs.Id);
            AddUpdatePreviousSlotAction (-1, "", nextOs.DateTimeRange.Upper.HasValue ? nextOs.DateTimeRange.Upper.Value : detectionDateTime.Value);
            return;
          }

          // We wait that the slot is finished (or nextOs.Consolidated?)
          if (!nextOs.EndDateTime.HasValue || nextOs.EndDateTime.Value > detectionDateTime) {
            GetLogger ().InfoFormat ("Run: next operation slot not completed, stop here");
            return;
          }

          // Which component?
          int componentId = (nextOs.Component != null ? ((Collections.IDataWithId<int>)nextOs.Component).Id : -1);
          if (componentId != -1) { // Otherwise we continue
            string newDetails = GetComponentDetails (nextOs.Component);
            if (m_previousComponentId != -1 && m_previousComponentId != componentId && m_previousSlotEnd.HasValue) {
              // Create a reason
              CreateReason (m_previousSlotEnd.Value, m_previousComponentDetails, nextOs.DateTimeRange.Lower.Value, newDetails);
            }

            AddUpdatePreviousSlotAction (componentId, newDetails, nextOs.EndDateTime.Value);
          }

          // We continue
          GoOn (nextOs.EndDateTime.Value);
        }
      }
    }

    string GetComponentDetails(IComponent component)
    {
      // Use the property "Display" for the details, or the ID (fallback)
      string details = component.Display;
      return string.IsNullOrEmpty (details) ? ((Collections.IDataWithId<int>)component).Id.ToString() : details;
    }

    void AddUpdatePreviousSlotAction (int componentId, string componentDetails, DateTime slotEnd)
    {
      var action = new UpdatePreviousSlotAction (this, componentId, componentDetails, slotEnd);
      AddDelayedAction (action);
    }

    internal void GetPreviousSlot (out int componentId, out string componentDetails, out DateTime? slotEnd)
    {
      componentId = m_previousComponentId;
      componentDetails = m_previousComponentDetails;
      slotEnd = m_previousSlotEnd;
    }

    internal void UpdatePreviousSlot (int componentId, string componentDetails, DateTime? slotEnd)
    {
      m_previousComponentId = componentId;
      log.InfoFormat ("AutoReasonBetweenComponents: updated '{0}' to '{1}'", GetKey (PREVIOUS_COMPONENT_ID), componentId);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (PREVIOUS_COMPONENT_ID), componentId);

      m_previousComponentDetails = componentDetails;
      log.InfoFormat ("AutoReasonBetweenComponents: updated '{0}' to '{1}'", GetKey (PREVIOUS_COMPONENT_DETAILS), componentDetails);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (PREVIOUS_COMPONENT_DETAILS), componentDetails);

      m_previousSlotEnd = slotEnd;
      log.InfoFormat ("AutoReasonBetweenComponents: updated '{0}' to '{1}'", GetKey (PREVIOUS_SLOT_END), m_previousSlotEnd);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (PREVIOUS_SLOT_END), m_previousSlotEnd);
    }

    internal void ResetPreviousSlot (int componentId, string componentDetails, DateTime? slotEnd)
    {
      m_previousComponentId = componentId;
      m_previousComponentDetails = componentDetails;
      m_previousSlotEnd = slotEnd;
    }

    void GoOn (DateTime dateTime)
    {
      if (dateTime <= this.DateTime) {
        dateTime = this.DateTime.AddSeconds (1);
      }
      this.AddUpdateDateTimeDelayedAction (dateTime);
    }

    void CreateReason (DateTime start, string oldDetails, DateTime end, string newDetails)
    {
      var details = "between component \"" + oldDetails + "\" and component \"" + newDetails + "\"";
      var action = new ApplyReasonAction (this, new UtcDateTimeRange (start, end), "", details, false);
      AddDelayedAction (action);
    }
  }
}
