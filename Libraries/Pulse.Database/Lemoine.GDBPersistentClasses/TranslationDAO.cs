// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ITranslationDAO">ITranslationDAO</see>
  /// </summary>
  public class TranslationDAO
    : VersionableNHibernateDAO<Translation, ITranslation, int>
    , ITranslationDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TranslationDAO).FullName);
    
    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      {
        ITranslation translation = new Translation ("", "UndefinedValue");
        translation.TranslationValue = "Undefined";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UndefinedValue");
        translation.TranslationValue = "Non défini";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "WorkOrderProjectIsJob");
        translation.TranslationValue = "Work Order + Project = Job";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "WorkOrderProjectIsJob");
        translation.TranslationValue = "OF + Projet = Job";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ProjectComponentIsPart");
        translation.TranslationValue = "Project + Component = Part";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ProjectComponentIsPart");
        translation.TranslationValue = "Projet + Composant = Pièce";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "IntermediateWorkPieceOperationIsSimpleOperation");
        translation.TranslationValue = "Intermediate Work Piece + Operation = Simple Operation";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "IntermediateWorkPieceOperationIsSimpleOperation");
        translation.TranslationValue = "Pièce intermédiaire + Opération = Opération simple";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UniqueWorkOrderFromProjectOrComponent");
        translation.TranslationValue = "Project/Component/Part => 1 Work Order";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UniqueWorkOrderFromProjectOrComponent");
        translation.TranslationValue = "Projet/Composant/Pièce => 1 OF";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UniqueComponentFromOperation");
        translation.TranslationValue = "Operation => 1 Component/Part";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UniqueComponentFromOperation");
        translation.TranslationValue = "Opération => 1 Composant/Pièce";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ComponentFromOperationOnly");
        translation.TranslationValue = "Project/Component/Part <= Operation only";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ComponentFromOperationOnly");
        translation.TranslationValue = "Projet/Composant/Pièce <= Opération seulement";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "WorkOrderFromComponentOnly");
        translation.TranslationValue = "WorkOrder <= Project/Component/Part only";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "WorkOrderFromComponentOnly");
        translation.TranslationValue = "OF <= Projet/Composant/Pièce seulement";
        InsertDefaultValue (translation);
      }

      // Machine mode categories
      {
        ITranslation translation = new Translation ("", "MachineModeCategoryInactive");
        translation.TranslationValue = "Inactive";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeCategoryInactive");
        translation.TranslationValue = "Inactif";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeCategoryActive");
        translation.TranslationValue = "Active";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeCategoryActive");
        translation.TranslationValue = "Actif";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeCategoryError");
        translation.TranslationValue = "Machine error";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeCategoryError");
        translation.TranslationValue = "Erreur machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeCategoryUnknown");
        translation.TranslationValue = "Unknown";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeCategoryUnknown");
        translation.TranslationValue = "État inconnu";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeCategoryEco");
        translation.TranslationValue = "Eco";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeCategoryEco");
        translation.TranslationValue = "Éco";
        InsertDefaultValue (translation);
      }

      // MachineModes
      {
        ITranslation translation = new Translation ("", "MachineModeInactive");
        translation.TranslationValue = "Inactive";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeInactive");
        translation.TranslationValue = "Inactif";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeActive");
        translation.TranslationValue = "Active";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeActive");
        translation.TranslationValue = "Actif";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeUnknown");
        translation.TranslationValue = "Unknown";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeInactiveOn");
        translation.TranslationValue = "Inactive (On)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoInactive");
        translation.TranslationValue = "Inactive (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoActive");
        translation.TranslationValue = "Active (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeManualActive");
        translation.TranslationValue = "Active (Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeSingleBlockActive");
        translation.TranslationValue = "Active (Single block)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeJogActive");
        translation.TranslationValue = "Active (Jog)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeHandleActive");
        translation.TranslationValue = "Active (Handle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMdiActive");
        translation.TranslationValue = "Active (MDI)";
        InsertDefaultValue (translation);
      }
      { // Cnc service stopped
        ITranslation translation = new Translation ("", "MachineModeNoData");
        translation.TranslationValue = "No data (no acquisition)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeUnavailable");
        translation.TranslationValue = "Machine unavailable";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeError");
        translation.TranslationValue = "Error";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeOff");
        translation.TranslationValue = "Off";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoNoRunningProgram");
        translation.TranslationValue = "Inactive (Auto) - No running program";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeInterrupted");
        translation.TranslationValue = "Interrupted";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeHold");
        translation.TranslationValue = "Feed hold";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeReady");
        translation.TranslationValue = "Program ready";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeStopped");
        translation.TranslationValue = "Program stopped";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeFinished");
        translation.TranslationValue = "Program finished";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeReset");
        translation.TranslationValue = "Program reset";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeNotReady");
        translation.TranslationValue = "Program not ready";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoError");
        translation.TranslationValue = "Error in program execution";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoErrorCleared");
        translation.TranslationValue = "Error in program execution cleared";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoEmergency");
        translation.TranslationValue = "Emergency stop (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoNullOverride");
        translation.TranslationValue = "Program suspended by an override 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoNullFeedrateOverride");
        translation.TranslationValue = "Program suspended by a feedrate override 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoNullRapidTraverseOverride");
        translation.TranslationValue = "Program suspended by a rapid traverse override 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoMachining");
        translation.TranslationValue = "Machining (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoFeed");
        translation.TranslationValue = "Feed (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoRapidTraverse");
        translation.TranslationValue = "Rapid traverse (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMachining");
        translation.TranslationValue = "Machining (Auto or Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeRapidTraverse");
        translation.TranslationValue = "Rapid traverse (Auto or Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoOtherOperation");
        translation.TranslationValue = "Not machining operation (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoToolChange");
        translation.TranslationValue = "Tool change (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoLaserCheck");
        translation.TranslationValue = "Laser check (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoPalletChange");
        translation.TranslationValue = "Pallet change (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoProbingCycle");
        translation.TranslationValue = "Probing cycle (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoHomePositioning");
        translation.TranslationValue = "Home positioning (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMStop");
        translation.TranslationValue = "Programmed machine stop";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeM0");
        translation.TranslationValue = "M0 (Stop)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeM1");
        translation.TranslationValue = "M1 (Optional stop)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeM60");
        translation.TranslationValue = "M60 (pallet shuttle and stop)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMWait");
        translation.TranslationValue = "Programmed operator input wait";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeEmergency");
        translation.TranslationValue = "Emergency stop";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeManualInactive");
        translation.TranslationValue = "Inactive (Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeSingleBlockInactive");
        translation.TranslationValue = "Inactive (Single block)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMdiInactive");
        translation.TranslationValue = "Inactive (MDI)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeJogInactive");
        translation.TranslationValue = "Inactive (Jog)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeHandleInactive");
        translation.TranslationValue = "Inactive (Handle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeProbingCycle");
        translation.TranslationValue = "Probing cycle (Auto or Manual)";
        InsertDefaultValue (translation);
      }
      /* MachineModeHomePositioning: deprecated */
      {
        ITranslation translation = new Translation ("", "MachineModeManualUnknown");
        translation.TranslationValue = "Unknown (Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeSingleBlock");
        translation.TranslationValue = "Unknown (Single block)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMdi");
        translation.TranslationValue = "Unknown (MDI)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeJog");
        translation.TranslationValue = "Unknown (Jog)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeHandle");
        translation.TranslationValue = "Unknown (Handle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAcquisitionError");
        translation.TranslationValue = "Acquisition error";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeReference");
        translation.TranslationValue = "Manual return to reference";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeDryRun");
        translation.TranslationValue = "Dry run";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMachineLock");
        translation.TranslationValue = "Machine lock (test mode)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoNormalActive");
        translation.TranslationValue = "Active (auto, normal execution)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoTestActive");
        translation.TranslationValue = "Active (auto, test mode)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMissingInfo");
        translation.TranslationValue = "Missing information to get the activity";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeNoMotion");
        translation.TranslationValue = "No motion (no feed or rapid traverse)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoNoMotion");
        translation.TranslationValue = "No motion in Auto mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeManualNoMotion");
        translation.TranslationValue = "No motion in Manual mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeSingleBlockNoMotion");
        translation.TranslationValue = "No motion in single block mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMdiNoMotion");
        translation.TranslationValue = "No motion in MDI mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeJogNoMotion");
        translation.TranslationValue = "No motion in jog mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeHandleNoMotion");
        translation.TranslationValue = "No motion in handle mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeProbablyOff");
        translation.TranslationValue = "Probably off";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoUnknown");
        translation.TranslationValue = "Unknown (auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAlarmStop");
        translation.TranslationValue = "Stopped because of an alarm";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoAlarmStop");
        translation.TranslationValue = "Stopped because of an alarm (auto)";
        InsertDefaultValue (translation);
      }
      
      // Reason groups
      {
        ITranslation translation = new Translation ("", "ReasonGroupDefault");
        translation.TranslationValue = "Unclassified reasons";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonGroupMotion");
        translation.TranslationValue = "Motion";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonGroupShort");
        translation.TranslationValue = "Short idle time";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonGroupIdle");
        translation.TranslationValue = "Idle";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonGroupUnknown");
        translation.TranslationValue = "Unknown status";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonGroupAuto");
        translation.TranslationValue = "Auto-reasons";
        InsertDefaultValue (translation);
      }

      // Reasons
      {
        ITranslation translation = new Translation ("", "ReasonMotion");
        translation.TranslationValue = "Motion";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonShort");
        translation.TranslationValue = "Short idle time";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonUnanswered");
        translation.TranslationValue = "Unanswered";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonUnattended");
        translation.TranslationValue = "Unattended";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonOff");
        translation.TranslationValue = "Off";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonUnknown");
        translation.TranslationValue = "Unknown status";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonProcessing");
        translation.TranslationValue = "Processing";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonBreak");
        translation.TranslationValue = "Break";
        InsertDefaultValue (translation);
      }

      // Machine monitoring types
      {
        ITranslation translation = new Translation ("", "MonitoringTypeMonitored");
        translation.TranslationValue = "Monitored";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MonitoringTypeNotMonitored");
        translation.TranslationValue = "Not monitored";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MonitoringTypeOutsource");
        translation.TranslationValue = "Outsource";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MonitoringTypeObsolete");
        translation.TranslationValue = "Obsolete";
        InsertDefaultValue (translation);
      }
      
      // Machine observation states
      {
        ITranslation translation = new Translation ("", "MachineObservationStateAttended");
        translation.TranslationValue = "Machine ON with operator (attended)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateUnattended");
        translation.TranslationValue = "Machine ON without operator (unattended)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateOnSite");
        translation.TranslationValue = "Machine ON with operator (on-site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateOnCall");
        translation.TranslationValue = "Machine ON with on call operator (off-site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateOff");
        translation.TranslationValue = "Machine OFF";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateUnknown");
        translation.TranslationValue = "Unknown";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateSetUp");
        translation.TranslationValue = "Set-up";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateQualityCheck");
        translation.TranslationValue = "Quality check";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateProduction");
        translation.TranslationValue = "Production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateMaintenance");
        translation.TranslationValue = "Maintenance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateBreak");
        translation.TranslationValue = "Break";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateCleanup");
        translation.TranslationValue = "Cleanup";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateWeekEnd");
        translation.TranslationValue = "Week-end";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateNight");
        translation.TranslationValue = "Night";
        InsertDefaultValue (translation);
      }

      // Machine state templates
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateAttended");
        translation.TranslationValue = "Machine ON with operator (attended)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateUnattended");
        translation.TranslationValue = "Machine ON without operator (unattended)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateOnSite");
        translation.TranslationValue = "Machine ON with operator (on-site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateOnCall");
        translation.TranslationValue = "Machine ON with on call operator (off-site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateOff");
        translation.TranslationValue = "Machine OFF";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateSetUp");
        translation.TranslationValue = "Set-up";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateQualityCheck");
        translation.TranslationValue = "Quality check";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateProduction");
        translation.TranslationValue = "Production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateMaintenance");
        translation.TranslationValue = "Maintenance";
        InsertDefaultValue (translation);
      }

      // Fields
      {
        ITranslation translation = new Translation ("", "FieldCADModelName");
        translation.TranslationValue = "CAD model name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldProjectName");
        translation.TranslationValue = "Project name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldComponentName");
        translation.TranslationValue = "Component name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldComponentTypeCode");
        translation.TranslationValue = "Component type code";
        InsertDefaultValue (translation);
      }
      // Deprecated: FieldComponentTypeKey (Component type key), FieldComponentTypeId (Component type ID)
      {
        ITranslation translation = new Translation ("", "FieldOperationName");
        translation.TranslationValue = "Operation name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldOperationTypeCode");
        translation.TranslationValue = "Operation type code";
        InsertDefaultValue (translation);
      }
      // Deprecated: FieldOperationTypeKey (Operation type key), FieldOperationTypeId (Operation type ID), FieldToolCode (Tool code), FieldToolName (Tool name)
      {
        ITranslation translation = new Translation ("", "FieldToolDiameter");
        translation.TranslationValue = "Tool diameter";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldToolRadius");
        translation.TranslationValue = "Tool radius";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldStrategy");
        translation.TranslationValue = "Strategy";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldContext");
        translation.TranslationValue = "Context";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldDepth");
        translation.TranslationValue = "Depth";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldWidth");
        translation.TranslationValue = "Width";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldTolerance");
        translation.TranslationValue = "Tolerance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldStock");
        translation.TranslationValue = "Stock";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldProgrammedFeedrate");
        translation.TranslationValue = "Programmed feed rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldProgrammedSpindleSpeed");
        translation.TranslationValue = "Programmed spindle speed";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFeedrate");
        translation.TranslationValue = "Feed rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSpindleSpeed");
        translation.TranslationValue = "Spindle speed";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSpindleLoad");
        translation.TranslationValue = "Spindle load";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFeedrateOverride");
        translation.TranslationValue = "Feed rate override";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSpindleSpeedOverride");
        translation.TranslationValue = "Spindle speed override";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldRapidTraverse");
        translation.TranslationValue = "Rapid traverse";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldCuttingFeedRate");
        translation.TranslationValue = "Cutting feed rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldRapidTraverseRate");
        translation.TranslationValue = "Rapid traverse rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFeedrateUS");
        translation.TranslationValue = "Feedrate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldRapidTraverseRateUS");
        translation.TranslationValue = "Rapid traverse rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldProgramName");
        translation.TranslationValue = "Program name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSubProgramName");
        translation.TranslationValue = "Sub-program name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldProgramFileName");
        translation.TranslationValue = "Program file name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSubProgramFileName");
        translation.TranslationValue = "Sub-program file name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldCncModes");
        translation.TranslationValue = "CNC modes";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldHold");
        translation.TranslationValue = "Hold";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldOptionalStopSwitch");
        translation.TranslationValue = "Optional stop switch";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldBlockDeleteSwitch");
        translation.TranslationValue = "Block delete switch";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldDryRun");
        translation.TranslationValue = "Dry run";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldToolNumber");
        translation.TranslationValue = "Tool number";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSingleBlock");
        translation.TranslationValue = "Single block";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldMachineLock");
        translation.TranslationValue = "Machine lock";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSpindleLoadPeak");
        translation.TranslationValue = "Spindle load peak";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldAlarmSignal");
        translation.TranslationValue = "Alarm signal";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldPalletNumber");
        translation.TranslationValue = "Pallet number";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldPalletReady");
        translation.TranslationValue = "Pallet ready";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldStackLight");
        translation.TranslationValue = "Stack light";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldProgramComment");
        translation.TranslationValue = "Program comment";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSubProgramComment");
        translation.TranslationValue = "Sub-program comment";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldCncPartCount");
        translation.TranslationValue = "Cnc part count";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldCncSequenceNumber");
        translation.TranslationValue = "Cnc sequence number";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldBlockNumber");
        translation.TranslationValue = "Block number";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFlow");
        translation.TranslationValue = "Flow";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFanucAutoManualMode");
        translation.TranslationValue = "Auto/manual mode (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFanucRunningStatus");
        translation.TranslationValue = "Run status (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFanucMotionStatus");
        translation.TranslationValue = "Motion status (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldMTConnectControllerMode");
        translation.TranslationValue = "Controller mode (MTConnect)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldMTConnectExecution");
        translation.TranslationValue = "Execution (MTConnect)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldMoriSeikiOperationMode");
        translation.TranslationValue = "Operation mode (Mori Seiki)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldHeidenhainExecutionMode");
        translation.TranslationValue = "Execution mode (Heidenhain)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldHeidenhainProgramStatus");
        translation.TranslationValue = "Program status (Heidenhain)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldRoedersMode");
        translation.TranslationValue = "Mode (Roeders)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldRoedersState");
        translation.TranslationValue = "State (Roeders)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldSelcaNCStatusCode");
        translation.TranslationValue = "NC Status Code (Selca)";
        InsertDefaultValue (translation);
      }
      
      // Shift
      {
        ITranslation translation = new Translation ("", "NoShift");
        translation.TranslationValue = "No shift";
        InsertDefaultValue (translation);
      }
      
      // Role
      {
        ITranslation translation = new Translation ("", "RoleOperator");
        translation.TranslationValue = "Operator";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "RoleSetup");
        translation.TranslationValue = "Set-up";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "RoleQuality");
        translation.TranslationValue = "Quality";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "RoleSupervisor");
        translation.TranslationValue = "Supervisor";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "RoleManager");
        translation.TranslationValue = "Manager";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "RoleAdministrator");
        translation.TranslationValue = "Administrator";
        InsertDefaultValue (translation);
      }

      // GoalType
      {
        ITranslation translation = new Translation ("", "GoalUtilizationPercentage");
        translation.TranslationValue = "Utilization goal";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "GoalQuantityVsProductionCycleDuration");
        translation.TranslationValue = "Expected quantity (%) of the theoretical output rate";
        InsertDefaultValue (translation);
      }

      // Unit
      {
        ITranslation translation = new Translation ("", "UnitFeedrate");
        translation.TranslationValue = "mm/min";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitFeedrateUS");
        translation.TranslationValue = "IPM";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitRotationSpeed");
        translation.TranslationValue = "RMP";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitPercent");
        translation.TranslationValue = "%";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitNumberOfParts");
        translation.TranslationValue = "parts";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitNone");
        translation.TranslationValue = "";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitDistanceMillimeter");
        translation.TranslationValue = "mm";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitDistanceInch");
        translation.TranslationValue = "inch";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitDistanceMeter");
        translation.TranslationValue = "m";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitDistanceFeet");
        translation.TranslationValue = "feet";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitDurationSeconds");
        translation.TranslationValue = "s";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitDurationMinutes");
        translation.TranslationValue = "min";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitDurationHours");
        translation.TranslationValue = "h";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitUnknown");
        translation.TranslationValue = "";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitToolNumberOfTimes");
        translation.TranslationValue = "times";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitWear");
        translation.TranslationValue = "(wear)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitFlowRate");
        translation.TranslationValue = "L/s";
        InsertDefaultValue (translation);
      }

      // EventLevel
      {
        ITranslation translation = new Translation ("", "EventLevelAlert");
        translation.TranslationValue = "Alert";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "EventLevelError");
        translation.TranslationValue = "Error";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "EventLevelWarn");
        translation.TranslationValue = "Warn";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "EventLevelNotice");
        translation.TranslationValue = "Notice";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "EventLevelInfo");
        translation.TranslationValue = "Info";
        InsertDefaultValue (translation);
      }

      // Controls
      {
        ITranslation translation = new Translation ("", "CadModelNull");
        translation.TranslationValue = "No CAD model";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "CncAcquisitionNull");
        translation.TranslationValue = "No cnc acquisition";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "CellNull");
        translation.TranslationValue = "No cell";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "CompanyNull");
        translation.TranslationValue = "No company";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ComputerNull");
        translation.TranslationValue = "No computer";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "DateTimeNull");
        translation.TranslationValue = "No date/time";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "DepartmentNull");
        translation.TranslationValue = "No department";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "EventLevelNull");
        translation.TranslationValue = "No event level";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldNull");
        translation.TranslationValue = "No field";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FileRepositoryNull");
        translation.TranslationValue = "No file";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineCategoryNull");
        translation.TranslationValue = "No machine category";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineFilterNull");
        translation.TranslationValue = "No machine filter";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeNull");
        translation.TranslationValue = "No machine mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModuleNull");
        translation.TranslationValue = "No machine module";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineMonitoringTypeNull");
        translation.TranslationValue = "No monitoring type";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineObservationStateNull");
        translation.TranslationValue = "No machine state";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateNull");
        translation.TranslationValue = "No machine state template";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineSubCategoryNull");
        translation.TranslationValue = "No machine sub-category";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MonitoredMachineNull");
        translation.TranslationValue = "No monitored machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "OperationCycleNull");
        translation.TranslationValue = "No operation cycle";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "OperationSlotNull");
        translation.TranslationValue = "No operation slot";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonGroupNull");
        translation.TranslationValue = "No motion status group";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ReasonReasonGroupNull");
        translation.TranslationValue = "No motion status";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "RoleNull");
        translation.TranslationValue = "No role";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ShiftNull");
        translation.TranslationValue = "No shift";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "TimePeriodOfDayNull");
        translation.TranslationValue = "No time period";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "ToolNull");
        translation.TranslationValue = "No tool";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "TranslationKeyNull");
        translation.TranslationValue = "No translation key";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "UnitNull");
        translation.TranslationValue = "No unit";
        InsertDefaultValue (translation);
      }
    }

    private void InsertDefaultValue (ITranslation translation)
    {
      if (null == Find (translation.Locale, translation.TranslationKey)) { // the translation does not exist => create it
        log.InfoFormat ("InsertDefaultValue: " +
                        "add translation {0} for locale={1} key={2}",
                        translation.TranslationValue, translation.Locale, translation.TranslationKey);
        MakePersistent (translation);
      }
    }
    #endregion // DefaultValues
    
    /// <summary>
    /// Find the ITranslation for the specified locale and translationKey
    /// 
    /// null is returned if the specified pair was not found
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="translationKey"></param>
    /// <returns></returns>
    public ITranslation Find (string locale, string translationKey)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Translation> ()
        .Add (Restrictions.Eq ("Locale", locale))
        .Add (Restrictions.Eq ("TranslationKey", translationKey))
        .SetCacheable (true)
        .UniqueResult<ITranslation> ();
    }
    
    /// <summary>
    /// Implements <see cref="Lemoine.ModelDAO.ITranslationDAO.GetDistinctTranslationKeys" />
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<string> GetDistinctTranslationKeys ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Translation> ()
        .SetProjection (Projections.Distinct (Projections.Property ("TranslationKey")))
        .AddOrder (Order.Asc ("TranslationKey"))
        .SetCacheable (true)
        .List<string> ();
    }
    
    /// <summary>
    /// Implements <see cref="Lemoine.ModelDAO.ITranslationDAO.GetTranslationFromKeyAndLocales" />
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<ITranslation> GetTranslationFromKeyAndLocales(string key, List<string> locales)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Translation> ()
        .Add (Restrictions.Eq ("TranslationKey", key))
        .Add (Restrictions.In ("Locale", locales))
        .SetCacheable(true)
        .List<ITranslation> ();
    }
  }
}
