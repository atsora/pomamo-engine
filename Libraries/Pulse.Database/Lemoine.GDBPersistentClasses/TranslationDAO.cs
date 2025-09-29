// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
        ITranslation translation = new Translation ("de", "UndefinedValue");
        translation.TranslationValue = "Undefiniert";
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
        ITranslation translation = new Translation ("de", "WorkOrderProjectIsJob");
        translation.TranslationValue = "Arbeitsauftrag + Projekt = Auftrag";
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
        ITranslation translation = new Translation ("de", "ProjectComponentIsPart");
        translation.TranslationValue = "Projekt + Komponente = Teil";
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
        ITranslation translation = new Translation ("de", "IntermediateWorkPieceOperationIsSimpleOperation");
        translation.TranslationValue = "Zwischenwerkstück + Operation = Einfache Operation";
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
        ITranslation translation = new Translation ("de", "UniqueWorkOrderFromProjectOrComponent");
        translation.TranslationValue = "Projekt/Komponente/Teil => 1 Arbeitsauftrag";
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
        ITranslation translation = new Translation ("de", "UniqueComponentFromOperation");
        translation.TranslationValue = "Operation => 1 Komponente/Teil";
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
        ITranslation translation = new Translation ("de", "ComponentFromOperationOnly");
        translation.TranslationValue = "Projekt/Komponente/Teil <= Nur Operation";
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
      {
        ITranslation translation = new Translation ("de", "WorkOrderFromComponentOnly");
        translation.TranslationValue = "Arbeitsauftrag <= Nur Projekt/Komponente/Teil";
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
        ITranslation translation = new Translation ("de", "MachineModeCategoryInactive");
        translation.TranslationValue = "Inaktiv";
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
        ITranslation translation = new Translation ("de", "MachineModeCategoryActive");
        translation.TranslationValue = "Aktiv";
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
        ITranslation translation = new Translation ("de", "MachineModeCategoryError");
        translation.TranslationValue = "Maschinenfehler";
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
        ITranslation translation = new Translation ("de", "MachineModeCategoryUnknown");
        translation.TranslationValue = "Unbekannt";
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
      {
        ITranslation translation = new Translation ("de", "MachineModeCategoryEco");
        translation.TranslationValue = "Öko";
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
        ITranslation translation = new Translation ("de", "MachineModeInactive");
        translation.TranslationValue = "Inaktiv";
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
        ITranslation translation = new Translation ("de", "MachineModeActive");
        translation.TranslationValue = "Aktiv";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeUnknown");
        translation.TranslationValue = "Unknown";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeUnknown");
        translation.TranslationValue = "État inconnu";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeUnknown");
        translation.TranslationValue = "Unbekannt";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeInactiveOn");
        translation.TranslationValue = "Inactive (On)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeInactiveOn");
        translation.TranslationValue = "Inactif (Allumé)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeInactiveOn");
        translation.TranslationValue = "Inaktiv (Eingeschaltet)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoInactive");
        translation.TranslationValue = "Inactive (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoInactive");
        translation.TranslationValue = "Inactif (Automatique)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoInactive");
        translation.TranslationValue = "Inaktiv (Automatisch)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoActive");
        translation.TranslationValue = "Active (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoActive");
        translation.TranslationValue = "Actif (Automatique)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoActive");
        translation.TranslationValue = "Aktiv (Automatisch)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeManualActive");
        translation.TranslationValue = "Active (Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeManualActive");
        translation.TranslationValue = "Actif (Manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeManualActive");
        translation.TranslationValue = "Aktiv (Manuell)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeSingleBlockActive");
        translation.TranslationValue = "Active (Single block)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeSingleBlockActive");
        translation.TranslationValue = "Actif (Bloc unique)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeSingleBlockActive");
        translation.TranslationValue = "Aktiv (Einzelsatzbetrieb)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeJogActive");
        translation.TranslationValue = "Active (Jog)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeJogActive");
        translation.TranslationValue = "Actif (Déplacement manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeJogActive");
        translation.TranslationValue = "Aktiv (Handbetrieb)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeHandleActive");
        translation.TranslationValue = "Active (Handle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeHandleActive");
        translation.TranslationValue = "Actif (Manivelle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeHandleActive");
        translation.TranslationValue = "Aktiv (Handrad)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeMdiActive");
        translation.TranslationValue = "Active (MDI)";
        InsertDefaultValue (translation);
      }
      { // Cnc service stopped
        ITranslation translation = new Translation ("fr", "MachineModeMdiActive");
        translation.TranslationValue = "Actif (MDI)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMdiActive");
        translation.TranslationValue = "Aktiv (MDI)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeNoData");
        translation.TranslationValue = "No data (no acquisition)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeNoData");
        translation.TranslationValue = "Pas de données (pas d'acquisition)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeNoData");
        translation.TranslationValue = "Keine Daten (keine Erfassung)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeUnavailable");
        translation.TranslationValue = "Machine unavailable";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeUnavailable");
        translation.TranslationValue = "Machine indisponible";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeUnavailable");
        translation.TranslationValue = "Maschine nicht verfügbar";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeError");
        translation.TranslationValue = "Error";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeError");
        translation.TranslationValue = "Erreur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeError");
        translation.TranslationValue = "Fehler";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeOff");
        translation.TranslationValue = "Off";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeOff");
        translation.TranslationValue = "Éteint";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeOff");
        translation.TranslationValue = "Aus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoNoRunningProgram");
        translation.TranslationValue = "Inactive (Auto) - No running program";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoNoRunningProgram");
        translation.TranslationValue = "Inactif (Auto) - Aucun programme en cours";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoNoRunningProgram");
        translation.TranslationValue = "Inaktiv (Auto) - Kein laufendes Programm";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeInterrupted");
        translation.TranslationValue = "Interrupted";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeInterrupted");
        translation.TranslationValue = "Interrompu";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeInterrupted");
        translation.TranslationValue = "Unterbrochen";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeHold");
        translation.TranslationValue = "Feed hold";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeHold");
        translation.TranslationValue = "Maintien d'avance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeHold");
        translation.TranslationValue = "Vorschubhalt";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeReady");
        translation.TranslationValue = "Program ready";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeReady");
        translation.TranslationValue = "Programme prêt";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeReady");
        translation.TranslationValue = "Programm bereit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeStopped");
        translation.TranslationValue = "Program stopped";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeStopped");
        translation.TranslationValue = "Programme arrêté";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeStopped");
        translation.TranslationValue = "Programm gestoppt";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeFinished");
        translation.TranslationValue = "Program finished";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeFinished");
        translation.TranslationValue = "Programme terminé";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeFinished");
        translation.TranslationValue = "Programm beendet";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeReset");
        translation.TranslationValue = "Program reset";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeReset");
        translation.TranslationValue = "Programme réinitialisé";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeReset");
        translation.TranslationValue = "Programm zurückgesetzt";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeNotReady");
        translation.TranslationValue = "Program not ready";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeNotReady");
        translation.TranslationValue = "Programme non prêt";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeNotReady");
        translation.TranslationValue = "Programm nicht bereit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoError");
        translation.TranslationValue = "Error in program execution";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoError");
        translation.TranslationValue = "Erreur dans l'exécution du programme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoError");
        translation.TranslationValue = "Fehler bei der Programmausführung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoErrorCleared");
        translation.TranslationValue = "Error in program execution cleared";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoErrorCleared");
        translation.TranslationValue = "Erreur dans l'exécution du programme réinitialisée";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoErrorCleared");
        translation.TranslationValue = "Fehler bei der Programmausführung behoben";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoEmergency");
        translation.TranslationValue = "Emergency stop (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoEmergency");
        translation.TranslationValue = "Arrêt d'urgence (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoEmergency");
        translation.TranslationValue = "Not-Aus (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeAutoNullOverride");
        translation.TranslationValue = "Program suspended by an override 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoNullOverride");
        translation.TranslationValue = "Programme suspendu par une correction d'avance 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoNullOverride");
        translation.TranslationValue = "Programm durch eine Vorschubüberschreibung 0 angehalten";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoNullFeedrateOverride");
        translation.TranslationValue = "Program suspended by a feedrate override 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoNullFeedrateOverride");
        translation.TranslationValue = "Programme suspendu par une correction d'avance de vitesse 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoNullFeedrateOverride");
        translation.TranslationValue = "Programm durch eine Vorschubgeschwindigkeitsüberschreibung 0 angehalten";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoNullRapidTraverseOverride");
        translation.TranslationValue = "Program suspended by a rapid traverse override 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoNullRapidTraverseOverride");
        translation.TranslationValue = "Programme suspendu par une correction de déplacement rapide 0";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoNullRapidTraverseOverride");
        translation.TranslationValue = "Programm durch eine Schnellvorschubüberschreibung 0 angehalten";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoMachining");
        translation.TranslationValue = "Machining (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoMachining");
        translation.TranslationValue = "Usinage (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoMachining");
        translation.TranslationValue = "Bearbeitung (Auto)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoFeed");
        translation.TranslationValue = "Feed (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoFeed");
        translation.TranslationValue = "Avance (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoFeed");
        translation.TranslationValue = "Vorschub (Auto)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoRapidTraverse");
        translation.TranslationValue = "Rapid traverse (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoRapidTraverse");
        translation.TranslationValue = "Déplacement rapide (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoRapidTraverse");
        translation.TranslationValue = "Schnellvorschub (Auto)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeMachining");
        translation.TranslationValue = "Machining (Auto or Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeMachining");
        translation.TranslationValue = "Usinage (Auto ou Manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMachining");
        translation.TranslationValue = "Bearbeitung (Auto oder Manuell)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeRapidTraverse");
        translation.TranslationValue = "Rapid traverse (Auto or Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeRapidTraverse");
        translation.TranslationValue = "Déplacement rapide (Auto ou Manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeRapidTraverse");
        translation.TranslationValue = "Schnellvorschub (Auto oder Manuell)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoOtherOperation");
        translation.TranslationValue = "Not machining operation (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoOtherOperation");
        translation.TranslationValue = "Opération non usinage (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoOtherOperation");
        translation.TranslationValue = "Keine Bearbeitungsoperation (Auto)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoToolChange");
        translation.TranslationValue = "Tool change (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoToolChange");
        translation.TranslationValue = "Changement d'outil (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoToolChange");
        translation.TranslationValue = "Werkzeugwechsel (Auto)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoLaserCheck");
        translation.TranslationValue = "Laser check (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoLaserCheck");
        translation.TranslationValue = "Vérification laser (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoLaserCheck");
        translation.TranslationValue = "Laserprüfung (Auto)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoPalletChange");
        translation.TranslationValue = "Pallet change (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoPalletChange");
        translation.TranslationValue = "Changement de palette (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoPalletChange");
        translation.TranslationValue = "Palettenwechsel (Auto)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoProbingCycle");
        translation.TranslationValue = "Probing cycle (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoProbingCycle");
        translation.TranslationValue = "Cycle de palpage (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoProbingCycle");
        translation.TranslationValue = "Messzyklus (Auto)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoHomePositioning");
        translation.TranslationValue = "Home positioning (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoHomePositioning");
        translation.TranslationValue = "Positionnement d'origine (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoHomePositioning");
        translation.TranslationValue = "Referenzpositionierung (Auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeMStop");
        translation.TranslationValue = "Programmed machine stop";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeMStop");
        translation.TranslationValue = "Arrêt machine programmé";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMStop");
        translation.TranslationValue = "Programmierter Maschinenstopp";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeM0");
        translation.TranslationValue = "M0 (Stop)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeM0");
        translation.TranslationValue = "M0 (Arrêt)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeM0");
        translation.TranslationValue = "M0 (Stopp)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeM1");
        translation.TranslationValue = "M1 (Optional stop)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeM1");
        translation.TranslationValue = "M1 (Arrêt optionnel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeM1");
        translation.TranslationValue = "M1 (Optionaler Stopp)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeM60");
        translation.TranslationValue = "M60 (Pallet shuttle and stop)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeM60");
        translation.TranslationValue = "M60 (Changement de palette et arrêt)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeM60");
        translation.TranslationValue = "M60 (Palettenwechsel und Stopp)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeMWait");
        translation.TranslationValue = "Programmed operator input wait";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeMWait");
        translation.TranslationValue = "Attente d'entrée opérateur programmée";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMWait");
        translation.TranslationValue = "Programmiertes Warten auf Bedienereingabe";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeEmergency");
        translation.TranslationValue = "Emergency stop";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeEmergency");
        translation.TranslationValue = "Arrêt d'urgence";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeEmergency");
        translation.TranslationValue = "Not-Aus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeManualInactive");
        translation.TranslationValue = "Inactive (Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeManualInactive");
        translation.TranslationValue = "Inactif (Manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeManualInactive");
        translation.TranslationValue = "Inaktiv (Manuell)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeSingleBlockInactive");
        translation.TranslationValue = "Inactive (Single block)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeSingleBlockInactive");
        translation.TranslationValue = "Inactif (Bloc unique)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeSingleBlockInactive");
        translation.TranslationValue = "Inaktiv (Einzelsatzbetrieb)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeMdiInactive");
        translation.TranslationValue = "Inactive (MDI)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeMdiInactive");
        translation.TranslationValue = "Inactif (MDI)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMdiInactive");
        translation.TranslationValue = "Inaktiv (MDI)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeJogInactive");
        translation.TranslationValue = "Inactive (Jog)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeJogInactive");
        translation.TranslationValue = "Inactif (Déplacement manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeJogInactive");
        translation.TranslationValue = "Inaktiv (Handbetrieb)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeHandleInactive");
        translation.TranslationValue = "Inactive (Handle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeHandleInactive");
        translation.TranslationValue = "Inactif (Manivelle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeHandleInactive");
        translation.TranslationValue = "Inaktiv (Handrad)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "MachineModeProbingCycle");
        translation.TranslationValue = "Probing cycle (Auto or Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeProbingCycle");
        translation.TranslationValue = "Cycle de palpage (Auto ou Manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeProbingCycle");
        translation.TranslationValue = "Messzyklus (Auto oder Manuell)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeManualUnknown");
        translation.TranslationValue = "Unknown (Manual)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeManualUnknown");
        translation.TranslationValue = "Inconnu (Manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeManualUnknown");
        translation.TranslationValue = "Unbekannt (Manuell)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeSingleBlock");
        translation.TranslationValue = "Unknown (Single block)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeSingleBlock");
        translation.TranslationValue = "Inconnu (Bloc unique)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeSingleBlock");
        translation.TranslationValue = "Unbekannt (Einzelsatzbetrieb)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeMdi");
        translation.TranslationValue = "Unknown (MDI)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeMdi");
        translation.TranslationValue = "Inconnu (MDI)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMdi");
        translation.TranslationValue = "Unbekannt (MDI)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeJog");
        translation.TranslationValue = "Unknown (Jog)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeJog");
        translation.TranslationValue = "Inconnu (Déplacement manuel)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeJog");
        translation.TranslationValue = "Unbekannt (Handbetrieb)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeHandle");
        translation.TranslationValue = "Unknown (Handle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeHandle");
        translation.TranslationValue = "Inconnu (Manivelle)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeHandle");
        translation.TranslationValue = "Unbekannt (Handrad)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAcquisitionError");
        translation.TranslationValue = "Acquisition error";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAcquisitionError");
        translation.TranslationValue = "Erreur d'acquisition";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAcquisitionError");
        translation.TranslationValue = "Erfassungsfehler";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeReference");
        translation.TranslationValue = "Manual return to reference";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeReference");
        translation.TranslationValue = "Retour manuel à la référence";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeReference");
        translation.TranslationValue = "Manuelle Rückkehr zur Referenz";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeDryRun");
        translation.TranslationValue = "Dry run";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeDryRun");
        translation.TranslationValue = "Simulation à vide";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeDryRun");
        translation.TranslationValue = "Trockenlauf";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeMachineLock");
        translation.TranslationValue = "Machine lock (test mode)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeMachineLock");
        translation.TranslationValue = "Verrouillage machine (mode test)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMachineLock");
        translation.TranslationValue = "Maschinensperre (Testmodus)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoNormalActive");
        translation.TranslationValue = "Active (auto, normal execution)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoNormalActive");
        translation.TranslationValue = "Actif (auto, exécution normale)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoNormalActive");
        translation.TranslationValue = "Aktiv (auto, normale Ausführung)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoTestActive");
        translation.TranslationValue = "Active (auto, test mode)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoTestActive");
        translation.TranslationValue = "Actif (auto, mode test)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoTestActive");
        translation.TranslationValue = "Aktiv (auto, Testmodus)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeMissingInfo");
        translation.TranslationValue = "Missing information to get the activity";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeMissingInfo");
        translation.TranslationValue = "Informations manquantes pour obtenir l'activité";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMissingInfo");
        translation.TranslationValue = "Fehlende Informationen zur Aktivitätsermittlung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeNoMotion");
        translation.TranslationValue = "No motion (no feed or rapid traverse)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeNoMotion");
        translation.TranslationValue = "Aucun mouvement (ni avance ni déplacement rapide)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeNoMotion");
        translation.TranslationValue = "Keine Bewegung (weder Vorschub noch Schnellvorschub)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoNoMotion");
        translation.TranslationValue = "No motion in Auto mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoNoMotion");
        translation.TranslationValue = "Aucun mouvement en mode Auto";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoNoMotion");
        translation.TranslationValue = "Keine Bewegung im Automatikmodus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeManualNoMotion");
        translation.TranslationValue = "No motion in Manual mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeManualNoMotion");
        translation.TranslationValue = "Aucun mouvement en mode Manuel";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeManualNoMotion");
        translation.TranslationValue = "Keine Bewegung im manuellen Modus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeSingleBlockNoMotion");
        translation.TranslationValue = "No motion in single block mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeSingleBlockNoMotion");
        translation.TranslationValue = "Aucun mouvement en mode Bloc unique";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeSingleBlockNoMotion");
        translation.TranslationValue = "Keine Bewegung im Einzelsatzmodus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeMdiNoMotion");
        translation.TranslationValue = "No motion in MDI mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeMdiNoMotion");
        translation.TranslationValue = "Aucun mouvement en mode MDI";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeMdiNoMotion");
        translation.TranslationValue = "Keine Bewegung im MDI-Modus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeJogNoMotion");
        translation.TranslationValue = "No motion in jog mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeJogNoMotion");
        translation.TranslationValue = "Aucun mouvement en mode Déplacement manuel";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeJogNoMotion");
        translation.TranslationValue = "Keine Bewegung im Handbetrieb";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeHandleNoMotion");
        translation.TranslationValue = "No motion in handle mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeHandleNoMotion");
        translation.TranslationValue = "Aucun mouvement en mode Manivelle";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeHandleNoMotion");
        translation.TranslationValue = "Keine Bewegung im Handradmodus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeProbablyOff");
        translation.TranslationValue = "Probably off";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeProbablyOff");
        translation.TranslationValue = "Probablement éteint";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeProbablyOff");
        translation.TranslationValue = "Wahrscheinlich aus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoUnknown");
        translation.TranslationValue = "Unknown (auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoUnknown");
        translation.TranslationValue = "Inconnu (auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoUnknown");
        translation.TranslationValue = "Unbekannt (Automatik)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAlarmStop");
        translation.TranslationValue = "Stopped because of an alarm";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAlarmStop");
        translation.TranslationValue = "Arrêté en raison d'une alarme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAlarmStop");
        translation.TranslationValue = "Angehalten wegen eines Alarms";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeAutoAlarmStop");
        translation.TranslationValue = "Stopped because of an alarm (auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeAutoAlarmStop");
        translation.TranslationValue = "Arrêté en raison d'une alarme (auto)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeAutoAlarmStop");
        translation.TranslationValue = "Angehalten wegen eines Alarms (Automatik)";
        InsertDefaultValue (translation);
      }

      // Reason groups
      {
        ITranslation translation = new Translation ("", "ReasonGroupDefault");
        translation.TranslationValue = "Unclassified reasons";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonGroupDefault");
        translation.TranslationValue = "Raisons non classifiées";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonGroupDefault");
        translation.TranslationValue = "Nicht klassifizierte Gründe";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonGroupMotion");
        translation.TranslationValue = "Motion";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonGroupMotion");
        translation.TranslationValue = "Mouvement";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonGroupMotion");
        translation.TranslationValue = "Bewegung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonGroupShort");
        translation.TranslationValue = "Short idle time";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonGroupShort");
        translation.TranslationValue = "Temps d'inactivité court";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonGroupShort");
        translation.TranslationValue = "Kurze Leerlaufzeit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonGroupIdle");
        translation.TranslationValue = "Idle";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonGroupIdle");
        translation.TranslationValue = "Inactivité";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonGroupIdle");
        translation.TranslationValue = "Leerlauf";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonGroupUnknown");
        translation.TranslationValue = "Unknown status";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonGroupUnknown");
        translation.TranslationValue = "Statut inconnu";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonGroupUnknown");
        translation.TranslationValue = "Unbekannter Status";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonGroupAuto");
        translation.TranslationValue = "Auto-reasons";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonGroupAuto");
        translation.TranslationValue = "Raisons automatiques";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonGroupAuto");
        translation.TranslationValue = "Automatische Gründe";
        InsertDefaultValue (translation);
      }

      // Reasons
      {
        ITranslation translation = new Translation ("", "ReasonMotion");
        translation.TranslationValue = "Motion";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonMotion");
        translation.TranslationValue = "Mouvement";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonMotion");
        translation.TranslationValue = "Bewegung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonShort");
        translation.TranslationValue = "Short idle time";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonShort");
        translation.TranslationValue = "Temps d'inactivité court";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonShort");
        translation.TranslationValue = "Kurze Leerlaufzeit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonUnanswered");
        translation.TranslationValue = "Unanswered";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonUnanswered");
        translation.TranslationValue = "Non renseigné";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonUnanswered");
        translation.TranslationValue = "Unbeantwortet";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonUnattended");
        translation.TranslationValue = "Unattended";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonUnattended");
        translation.TranslationValue = "Non surveillé";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonUnattended");
        translation.TranslationValue = "Unbeaufsichtigt";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonOff");
        translation.TranslationValue = "Off";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonOff");
        translation.TranslationValue = "Éteint";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonOff");
        translation.TranslationValue = "Aus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonUnknown");
        translation.TranslationValue = "Unknown status";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonUnknown");
        translation.TranslationValue = "Statut inconnu";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonUnknown");
        translation.TranslationValue = "Unbekannter Status";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonProcessing");
        translation.TranslationValue = "Processing";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonProcessing");
        translation.TranslationValue = "En traitement";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonProcessing");
        translation.TranslationValue = "Verarbeitung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonBreak");
        translation.TranslationValue = "Break";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonBreak");
        translation.TranslationValue = "Pause";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonBreak");
        translation.TranslationValue = "Pause";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonInactive");
        translation.TranslationValue = "Inactive";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonInactive");
        translation.TranslationValue = "Inactif";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonInactive");
        translation.TranslationValue = "Inaktiv";
        InsertDefaultValue (translation);
      }

      // Machine monitoring types
      {
        ITranslation translation = new Translation ("", "MonitoringTypeMonitored");
        translation.TranslationValue = "Monitored";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MonitoringTypeMonitored");
        translation.TranslationValue = "Surveillé";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MonitoringTypeMonitored");
        translation.TranslationValue = "Überwacht";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MonitoringTypeNotMonitored");
        translation.TranslationValue = "Not monitored";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MonitoringTypeNotMonitored");
        translation.TranslationValue = "Non surveillé";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MonitoringTypeNotMonitored");
        translation.TranslationValue = "Nicht überwacht";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MonitoringTypeOutsource");
        translation.TranslationValue = "Outsource";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MonitoringTypeOutsource");
        translation.TranslationValue = "Externalisé";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MonitoringTypeOutsource");
        translation.TranslationValue = "Ausgelagert";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MonitoringTypeObsolete");
        translation.TranslationValue = "Obsolete";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MonitoringTypeObsolete");
        translation.TranslationValue = "Obsolète";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MonitoringTypeObsolete");
        translation.TranslationValue = "Veraltet";
        InsertDefaultValue (translation);
      }

      // Machine observation states
      {
        ITranslation translation = new Translation ("", "MachineObservationStateAttended");
        translation.TranslationValue = "Machine ON with operator (attended)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateAttended");
        translation.TranslationValue = "Machine ALLUMÉE avec opérateur (surveillée)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateAttended");
        translation.TranslationValue = "Maschine EIN mit Bediener (überwacht)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateUnattended");
        translation.TranslationValue = "Machine ON without operator (unattended)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateUnattended");
        translation.TranslationValue = "Machine ALLUMÉE sans opérateur (non surveillée)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateUnattended");
        translation.TranslationValue = "Maschine EIN ohne Bediener (unbeaufsichtigt)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateOnSite");
        translation.TranslationValue = "Machine ON with operator (on-site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateOnSite");
        translation.TranslationValue = "Machine ALLUMÉE avec opérateur (sur site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateOnSite");
        translation.TranslationValue = "Maschine EIN mit Bediener (vor Ort)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateOnCall");
        translation.TranslationValue = "Machine ON with on call operator (off-site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateOnCall");
        translation.TranslationValue = "Machine ALLUMÉE avec opérateur d'astreinte (hors site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateOnCall");
        translation.TranslationValue = "Maschine EIN mit Bereitschaftsbediener (außerhalb)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateOff");
        translation.TranslationValue = "Machine OFF";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateOff");
        translation.TranslationValue = "Machine ÉTEINTE";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateOff");
        translation.TranslationValue = "Maschine AUS";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateUnknown");
        translation.TranslationValue = "Unknown";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateUnknown");
        translation.TranslationValue = "Inconnu";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateUnknown");
        translation.TranslationValue = "Unbekannt";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateSetUp");
        translation.TranslationValue = "Set-up";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateSetUp");
        translation.TranslationValue = "Réglage";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateSetUp");
        translation.TranslationValue = "Einrichtung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateQualityCheck");
        translation.TranslationValue = "Quality check";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateQualityCheck");
        translation.TranslationValue = "Contrôle qualité";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateQualityCheck");
        translation.TranslationValue = "Qualitätsprüfung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateProduction");
        translation.TranslationValue = "Production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateProduction");
        translation.TranslationValue = "Production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateProduction");
        translation.TranslationValue = "Produktion";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateMaintenance");
        translation.TranslationValue = "Maintenance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateMaintenance");
        translation.TranslationValue = "Maintenance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateMaintenance");
        translation.TranslationValue = "Wartung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateBreak");
        translation.TranslationValue = "Break";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateBreak");
        translation.TranslationValue = "Pause";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateBreak");
        translation.TranslationValue = "Pause";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateCleanup");
        translation.TranslationValue = "Cleanup";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateCleanup");
        translation.TranslationValue = "Nettoyage";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateCleanup");
        translation.TranslationValue = "Reinigung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateWeekEnd");
        translation.TranslationValue = "Week-end";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateWeekEnd");
        translation.TranslationValue = "Week-end";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateWeekEnd");
        translation.TranslationValue = "Wochenende";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateNight");
        translation.TranslationValue = "Night";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateNight");
        translation.TranslationValue = "Nuit";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateNight");
        translation.TranslationValue = "Nacht";
        InsertDefaultValue (translation);
      }

      // Machine state templates
      {
        ITranslation translation = new Translation ("", "MachineStateTemplateAttended");
        translation.TranslationValue = "Machine ON with operator (attended)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateAttended");
        translation.TranslationValue = "Machine ALLUMÉE avec opérateur (surveillée)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateAttended");
        translation.TranslationValue = "Maschine EIN mit Bediener (überwacht)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateUnattended");
        translation.TranslationValue = "Machine ON without operator (unattended)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateUnattended");
        translation.TranslationValue = "Machine ALLUMÉE sans opérateur (non surveillée)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateUnattended");
        translation.TranslationValue = "Maschine EIN ohne Bediener (unbeaufsichtigt)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateOnSite");
        translation.TranslationValue = "Machine ON with operator (on-site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateOnSite");
        translation.TranslationValue = "Machine ALLUMÉE avec opérateur (sur site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateOnSite");
        translation.TranslationValue = "Maschine EIN mit Bediener (vor Ort)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateOnCall");
        translation.TranslationValue = "Machine ON with on call operator (off-site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateOnCall");
        translation.TranslationValue = "Machine ALLUMÉE avec opérateur d'astreinte (hors site)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateOnCall");
        translation.TranslationValue = "Maschine EIN mit Bereitschaftsbediener (außerhalb)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateOff");
        translation.TranslationValue = "Machine OFF";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateOff");
        translation.TranslationValue = "Machine ÉTEINTE";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateOff");
        translation.TranslationValue = "Maschine AUS";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateSetUp");
        translation.TranslationValue = "Set-up";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateSetUp");
        translation.TranslationValue = "Réglage";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateSetUp");
        translation.TranslationValue = "Einrichtung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateQualityCheck");
        translation.TranslationValue = "Quality check";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateQualityCheck");
        translation.TranslationValue = "Contrôle qualité";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateQualityCheck");
        translation.TranslationValue = "Qualitätsprüfung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateProduction");
        translation.TranslationValue = "Production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateProduction");
        translation.TranslationValue = "Production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateProduction");
        translation.TranslationValue = "Produktion";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateMaintenance");
        translation.TranslationValue = "Maintenance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateMaintenance");
        translation.TranslationValue = "Maintenance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateMaintenance");
        translation.TranslationValue = "Wartung";
        InsertDefaultValue (translation);
      }

      // Fields
      {
        ITranslation translation = new Translation ("", "FieldCADModelName");
        translation.TranslationValue = "CAD model name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldCADModelName");
        translation.TranslationValue = "Nom du modèle CAO";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldCADModelName");
        translation.TranslationValue = "CAD-Modellname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldProjectName");
        translation.TranslationValue = "Project name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldProjectName");
        translation.TranslationValue = "Nom du projet";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldProjectName");
        translation.TranslationValue = "Projektname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldComponentName");
        translation.TranslationValue = "Component name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldComponentName");
        translation.TranslationValue = "Nom du composant";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldComponentName");
        translation.TranslationValue = "Komponentenname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldComponentTypeCode");
        translation.TranslationValue = "Component type code";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldComponentTypeCode");
        translation.TranslationValue = "Code du type de composant";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldComponentTypeCode");
        translation.TranslationValue = "Komponententypcode";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldOperationName");
        translation.TranslationValue = "Operation name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldOperationName");
        translation.TranslationValue = "Nom de l'opération";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldOperationName");
        translation.TranslationValue = "Operationsname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldOperationTypeCode");
        translation.TranslationValue = "Operation type code";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldOperationTypeCode");
        translation.TranslationValue = "Code du type d'opération";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldOperationTypeCode");
        translation.TranslationValue = "Operationstypcode";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldToolDiameter");
        translation.TranslationValue = "Tool diameter";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldToolDiameter");
        translation.TranslationValue = "Diamètre de l'outil";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldToolDiameter");
        translation.TranslationValue = "Werkzeugdurchmesser";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldToolRadius");
        translation.TranslationValue = "Tool radius";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldToolRadius");
        translation.TranslationValue = "Rayon de l'outil";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldToolRadius");
        translation.TranslationValue = "Werkzeugradius";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldStrategy");
        translation.TranslationValue = "Strategy";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldStrategy");
        translation.TranslationValue = "Stratégie";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldStrategy");
        translation.TranslationValue = "Strategie";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldContext");
        translation.TranslationValue = "Context";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldContext");
        translation.TranslationValue = "Contexte";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldContext");
        translation.TranslationValue = "Kontext";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldDepth");
        translation.TranslationValue = "Depth";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldDepth");
        translation.TranslationValue = "Profondeur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldDepth");
        translation.TranslationValue = "Tiefe";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldWidth");
        translation.TranslationValue = "Width";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldWidth");
        translation.TranslationValue = "Largeur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldWidth");
        translation.TranslationValue = "Breite";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldTolerance");
        translation.TranslationValue = "Tolerance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldTolerance");
        translation.TranslationValue = "Tolérance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldTolerance");
        translation.TranslationValue = "Toleranz";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldStock");
        translation.TranslationValue = "Stock";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldStock");
        translation.TranslationValue = "Surépaisseur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldStock");
        translation.TranslationValue = "Aufmaß";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldProgrammedFeedrate");
        translation.TranslationValue = "Programmed feed rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldProgrammedFeedrate");
        translation.TranslationValue = "Vitesse d'avance programmée";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldProgrammedFeedrate");
        translation.TranslationValue = "Programmierter Vorschub";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldProgrammedSpindleSpeed");
        translation.TranslationValue = "Programmed spindle speed";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldProgrammedSpindleSpeed");
        translation.TranslationValue = "Vitesse de broche programmée";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldProgrammedSpindleSpeed");
        translation.TranslationValue = "Programmierter Spindeldrehzahl";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldFeedrate");
        translation.TranslationValue = "Feed rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldFeedrate");
        translation.TranslationValue = "Vitesse d'avance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldFeedrate");
        translation.TranslationValue = "Vorschubgeschwindigkeit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSpindleSpeed");
        translation.TranslationValue = "Spindle speed";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSpindleSpeed");
        translation.TranslationValue = "Vitesse de broche";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSpindleSpeed");
        translation.TranslationValue = "Spindeldrehzahl";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSpindleLoad");
        translation.TranslationValue = "Spindle load";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSpindleLoad");
        translation.TranslationValue = "Charge de la broche";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSpindleLoad");
        translation.TranslationValue = "Spindellast";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldFeedrateOverride");
        translation.TranslationValue = "Feed rate override";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldFeedrateOverride");
        translation.TranslationValue = "Correction de vitesse d'avance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldFeedrateOverride");
        translation.TranslationValue = "Vorschubgeschwindigkeitsüberschreibung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSpindleSpeedOverride");
        translation.TranslationValue = "Spindle speed override";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSpindleSpeedOverride");
        translation.TranslationValue = "Correction de vitesse de broche";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSpindleSpeedOverride");
        translation.TranslationValue = "Spindeldrehzahlüberschreibung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldRapidTraverse");
        translation.TranslationValue = "Rapid traverse";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldRapidTraverse");
        translation.TranslationValue = "Déplacement rapide";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldRapidTraverse");
        translation.TranslationValue = "Schnellvorschub";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldCuttingFeedRate");
        translation.TranslationValue = "Cutting feed rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldCuttingFeedRate");
        translation.TranslationValue = "Vitesse d'avance de coupe";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldCuttingFeedRate");
        translation.TranslationValue = "Schnittvorschubgeschwindigkeit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldRapidTraverseRate");
        translation.TranslationValue = "Rapid traverse rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldRapidTraverseRate");
        translation.TranslationValue = "Vitesse de déplacement rapide";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldRapidTraverseRate");
        translation.TranslationValue = "Schnellvorschubrate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFeedrateUS");
        translation.TranslationValue = "Feedrate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldFeedrateUS");
        translation.TranslationValue = "Vitesse d'avance (US)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldFeedrateUS");
        translation.TranslationValue = "Vorschubgeschwindigkeit (US)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldRapidTraverseRateUS");
        translation.TranslationValue = "Rapid traverse rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldRapidTraverseRateUS");
        translation.TranslationValue = "Vitesse de déplacement rapide (US)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldRapidTraverseRateUS");
        translation.TranslationValue = "Schnellvorschubrate (US)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldProgramName");
        translation.TranslationValue = "Program name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldProgramName");
        translation.TranslationValue = "Nom du programme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldProgramName");
        translation.TranslationValue = "Programmname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSubProgramName");
        translation.TranslationValue = "Sub-program name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSubProgramName");
        translation.TranslationValue = "Nom du sous-programme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSubProgramName");
        translation.TranslationValue = "Unterprogrammname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldProgramFileName");
        translation.TranslationValue = "Program file name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldProgramFileName");
        translation.TranslationValue = "Nom du fichier programme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldProgramFileName");
        translation.TranslationValue = "Programmdateiname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSubProgramFileName");
        translation.TranslationValue = "Sub-program file name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSubProgramFileName");
        translation.TranslationValue = "Nom du fichier sous-programme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSubProgramFileName");
        translation.TranslationValue = "Unterprogrammdateiname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldCncModes");
        translation.TranslationValue = "CNC modes";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldCncModes");
        translation.TranslationValue = "Modes CNC";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldCncModes");
        translation.TranslationValue = "CNC-Modi";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldHold");
        translation.TranslationValue = "Hold";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldHold");
        translation.TranslationValue = "Maintien de l'avance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldHold");
        translation.TranslationValue = "Vorschubhalt";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldOptionalStopSwitch");
        translation.TranslationValue = "Optional stop switch";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldOptionalStopSwitch");
        translation.TranslationValue = "Interrupteur d'arrêt optionnel";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldOptionalStopSwitch");
        translation.TranslationValue = "Optionaler Stopp-Schalter";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldBlockDeleteSwitch");
        translation.TranslationValue = "Block delete switch";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldBlockDeleteSwitch");
        translation.TranslationValue = "Interrupteur de suppression de bloc";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldBlockDeleteSwitch");
        translation.TranslationValue = "Blocklöschschalter";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldDryRun");
        translation.TranslationValue = "Dry run";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldDryRun");
        translation.TranslationValue = "Simulation à vide";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldDryRun");
        translation.TranslationValue = "Trockenlauf";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldToolName");
        translation.TranslationValue = "Tool name";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldToolName");
        translation.TranslationValue = "Nom d'outil";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldToolName");
        translation.TranslationValue = "Werkzeugname";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldToolNumber");
        translation.TranslationValue = "Tool number";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldToolNumber");
        translation.TranslationValue = "Numéro d'outil";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldToolNumber");
        translation.TranslationValue = "Werkzeugnummer";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSingleBlock");
        translation.TranslationValue = "Single block";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSingleBlock");
        translation.TranslationValue = "Bloc unique";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSingleBlock");
        translation.TranslationValue = "Einzelsatzbetrieb";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldMachineLock");
        translation.TranslationValue = "Machine lock";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldMachineLock");
        translation.TranslationValue = "Verrouillage machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldMachineLock");
        translation.TranslationValue = "Maschinensperre";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSpindleLoadPeak");
        translation.TranslationValue = "Spindle load peak";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSpindleLoadPeak");
        translation.TranslationValue = "Pic de charge de broche";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSpindleLoadPeak");
        translation.TranslationValue = "Spindellastspitze";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldAlarmSignal");
        translation.TranslationValue = "Alarm signal";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldAlarmSignal");
        translation.TranslationValue = "Signal d'alarme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldAlarmSignal");
        translation.TranslationValue = "Alarmsignal";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldPalletNumber");
        translation.TranslationValue = "Pallet number";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldPalletNumber");
        translation.TranslationValue = "Numéro de palette";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldPalletNumber");
        translation.TranslationValue = "Palettennummer";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldPalletReady");
        translation.TranslationValue = "Pallet ready";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldPalletReady");
        translation.TranslationValue = "Palette prête";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldPalletReady");
        translation.TranslationValue = "Palette bereit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldStackLight");
        translation.TranslationValue = "Stack light";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldStackLight");
        translation.TranslationValue = "Verrine lumineuse";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldStackLight");
        translation.TranslationValue = "Signalleuchte";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldProgramComment");
        translation.TranslationValue = "Program comment";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldProgramComment");
        translation.TranslationValue = "Commentaire du programme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldProgramComment");
        translation.TranslationValue = "Programmkommentar";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSubProgramComment");
        translation.TranslationValue = "Sub-program comment";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSubProgramComment");
        translation.TranslationValue = "Commentaire du sous-programme";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSubProgramComment");
        translation.TranslationValue = "Unterprogrammkommentar";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldCncPartCount");
        translation.TranslationValue = "CNC part count";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldCncPartCount");
        translation.TranslationValue = "Nombre de pièces CNC";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldCncPartCount");
        translation.TranslationValue = "CNC-Teileanzahl";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldCncSequenceNumber");
        translation.TranslationValue = "CNC sequence number";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldCncSequenceNumber");
        translation.TranslationValue = "Numéro de séquence CNC";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldCncSequenceNumber");
        translation.TranslationValue = "CNC-Sequenznummer";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldBlockNumber");
        translation.TranslationValue = "Block number";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldBlockNumber");
        translation.TranslationValue = "Numéro de bloc";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldBlockNumber");
        translation.TranslationValue = "Blocknummer";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldFlow");
        translation.TranslationValue = "Flow";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldFlow");
        translation.TranslationValue = "Débit";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldFlow");
        translation.TranslationValue = "Durchfluss";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("", "FieldFanucAutoManualMode");
        translation.TranslationValue = "Auto/manual mode (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldFanucAutoManualMode");
        translation.TranslationValue = "Mode automatique/manuel (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldFanucAutoManualMode");
        translation.TranslationValue = "Automatik-/Handbetrieb (Fanuc)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldFanucRunningStatus");
        translation.TranslationValue = "Run status (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldFanucRunningStatus");
        translation.TranslationValue = "Statut d'exécution (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldFanucRunningStatus");
        translation.TranslationValue = "Betriebsstatus (Fanuc)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldFanucMotionStatus");
        translation.TranslationValue = "Motion status (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldFanucMotionStatus");
        translation.TranslationValue = "Statut de mouvement (Fanuc)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldFanucMotionStatus");
        translation.TranslationValue = "Bewegungsstatus (Fanuc)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldMTConnectControllerMode");
        translation.TranslationValue = "Controller mode (MTConnect)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldMTConnectControllerMode");
        translation.TranslationValue = "Mode du contrôleur (MTConnect)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldMTConnectControllerMode");
        translation.TranslationValue = "Steuermodus (MTConnect)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldMTConnectExecution");
        translation.TranslationValue = "Execution (MTConnect)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldMTConnectExecution");
        translation.TranslationValue = "Exécution (MTConnect)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldMTConnectExecution");
        translation.TranslationValue = "Ausführung (MTConnect)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldMoriSeikiOperationMode");
        translation.TranslationValue = "Operation mode (Mori Seiki)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldMoriSeikiOperationMode");
        translation.TranslationValue = "Mode d'opération (Mori Seiki)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldMoriSeikiOperationMode");
        translation.TranslationValue = "Betriebsmodus (Mori Seiki)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldHeidenhainExecutionMode");
        translation.TranslationValue = "Execution mode (Heidenhain)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldHeidenhainExecutionMode");
        translation.TranslationValue = "Mode d'exécution (Heidenhain)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldHeidenhainExecutionMode");
        translation.TranslationValue = "Ausführungsmodus (Heidenhain)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldHeidenhainProgramStatus");
        translation.TranslationValue = "Program status (Heidenhain)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldHeidenhainProgramStatus");
        translation.TranslationValue = "Statut du programme (Heidenhain)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldHeidenhainProgramStatus");
        translation.TranslationValue = "Programmstatus (Heidenhain)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldRoedersMode");
        translation.TranslationValue = "Mode (Roeders)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldRoedersMode");
        translation.TranslationValue = "Mode (Roeders)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldRoedersMode");
        translation.TranslationValue = "Modus (Roeders)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldRoedersState");
        translation.TranslationValue = "State (Roeders)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldRoedersState");
        translation.TranslationValue = "État (Roeders)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldRoedersState");
        translation.TranslationValue = "Zustand (Roeders)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldSelcaNCStatusCode");
        translation.TranslationValue = "NC Status Code (Selca)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldSelcaNCStatusCode");
        translation.TranslationValue = "Code d'état NC (Selca)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldSelcaNCStatusCode");
        translation.TranslationValue = "NC-Statuscode (Selca)";
        InsertDefaultValue (translation);
      }

      // Production state
      {
        ITranslation translation = new Translation ("", "ProductionStateProduction");
        translation.TranslationValue = "Production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ProductionStateProduction");
        translation.TranslationValue = "Production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ProductionStateProduction");
        translation.TranslationValue = "Produktion";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ProductionStateNoProduction");
        translation.TranslationValue = "No production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ProductionStateNoProduction");
        translation.TranslationValue = "Pas de production";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ProductionStateNoProduction");
        translation.TranslationValue = "Keine Produktion";
        InsertDefaultValue (translation);
      }

      // Shift
      {
        ITranslation translation = new Translation ("", "NoShift");
        translation.TranslationValue = "No shift";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "NoShift");
        translation.TranslationValue = "Pas d'équipe";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "NoShift");
        translation.TranslationValue = "Keine Schicht";
        InsertDefaultValue (translation);
      }

      // Role
      {
        ITranslation translation = new Translation ("", "RoleOperator");
        translation.TranslationValue = "Operator";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "RoleOperator");
        translation.TranslationValue = "Opérateur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "RoleOperator");
        translation.TranslationValue = "Bediener";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "RoleSetup");
        translation.TranslationValue = "Set-up";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "RoleSetup");
        translation.TranslationValue = "Régleur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "RoleSetup");
        translation.TranslationValue = "Einrichtung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "RoleQuality");
        translation.TranslationValue = "Quality";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "RoleQuality");
        translation.TranslationValue = "Qualité";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "RoleQuality");
        translation.TranslationValue = "Qualität";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "RoleSupervisor");
        translation.TranslationValue = "Supervisor";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "RoleSupervisor");
        translation.TranslationValue = "Superviseur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "RoleSupervisor");
        translation.TranslationValue = "Aufseher";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "RoleManager");
        translation.TranslationValue = "Manager";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "RoleManager");
        translation.TranslationValue = "Responsable";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "RoleManager");
        translation.TranslationValue = "Manager";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "RoleAdministrator");
        translation.TranslationValue = "Administrator";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "RoleAdministrator");
        translation.TranslationValue = "Administrateur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "RoleAdministrator");
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
        ITranslation translation = new Translation ("fr", "GoalUtilizationPercentage");
        translation.TranslationValue = "Objectif d'utilisation machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "GoalUtilizationPercentage");
        translation.TranslationValue = "Auslastungsziel";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "GoalQuantityVsProductionCycleDuration");
        translation.TranslationValue = "Expected quantity (%) of the theoretical output rate";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "GoalQuantityVsProductionCycleDuration");
        translation.TranslationValue = "Quantité attendue (%) du taux de production théorique";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "GoalQuantityVsProductionCycleDuration");
        translation.TranslationValue = "Erwartete Menge (%) der theoretischen Ausbringungsrate";
        InsertDefaultValue (translation);
      }

      // Unit
      {
        ITranslation translation = new Translation ("", "UnitFeedrate");
        translation.TranslationValue = "mm/min";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitFeedrate");
        translation.TranslationValue = "mm/min";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitFeedrate");
        translation.TranslationValue = "mm/min";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitFeedrateUS");
        translation.TranslationValue = "IPM";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitFeedrateUS");
        translation.TranslationValue = "IPM";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitFeedrateUS");
        translation.TranslationValue = "IPM";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitRotationSpeed");
        translation.TranslationValue = "RPM";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitRotationSpeed");
        translation.TranslationValue = "tr/min";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitRotationSpeed");
        translation.TranslationValue = "U/min";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitPercent");
        translation.TranslationValue = "%";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitPercent");
        translation.TranslationValue = "%";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitPercent");
        translation.TranslationValue = "%";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitNumberOfParts");
        translation.TranslationValue = "parts";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitNumberOfParts");
        translation.TranslationValue = "pièces";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitNumberOfParts");
        translation.TranslationValue = "Teile";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitNone");
        translation.TranslationValue = "";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitNone");
        translation.TranslationValue = "";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitNone");
        translation.TranslationValue = "";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitDistanceMillimeter");
        translation.TranslationValue = "mm";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitDistanceMillimeter");
        translation.TranslationValue = "mm";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitDistanceMillimeter");
        translation.TranslationValue = "mm";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitDistanceInch");
        translation.TranslationValue = "inch";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitDistanceInch");
        translation.TranslationValue = "pouce";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitDistanceInch");
        translation.TranslationValue = "Zoll";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitDistanceMeter");
        translation.TranslationValue = "m";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitDistanceMeter");
        translation.TranslationValue = "m";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitDistanceMeter");
        translation.TranslationValue = "m";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitDistanceFeet");
        translation.TranslationValue = "feet";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitDistanceFeet");
        translation.TranslationValue = "pieds";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitDistanceFeet");
        translation.TranslationValue = "Füße";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitDurationSeconds");
        translation.TranslationValue = "s";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitDurationSeconds");
        translation.TranslationValue = "s";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitDurationSeconds");
        translation.TranslationValue = "s";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitDurationMinutes");
        translation.TranslationValue = "min";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitDurationMinutes");
        translation.TranslationValue = "min";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitDurationMinutes");
        translation.TranslationValue = "min";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitDurationHours");
        translation.TranslationValue = "h";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitDurationHours");
        translation.TranslationValue = "h";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitDurationHours");
        translation.TranslationValue = "h";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitUnknown");
        translation.TranslationValue = "";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitUnknown");
        translation.TranslationValue = "";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitUnknown");
        translation.TranslationValue = "";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitToolNumberOfTimes");
        translation.TranslationValue = "times";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitToolNumberOfTimes");
        translation.TranslationValue = "fois";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitToolNumberOfTimes");
        translation.TranslationValue = "mal";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitWear");
        translation.TranslationValue = "(wear)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitWear");
        translation.TranslationValue = "(usure)";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitWear");
        translation.TranslationValue = "(Abnutzung)";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitFlowRate");
        translation.TranslationValue = "L/s";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitFlowRate");
        translation.TranslationValue = "L/s";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitFlowRate");
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
        ITranslation translation = new Translation ("fr", "EventLevelAlert");
        translation.TranslationValue = "Alerte";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "EventLevelAlert");
        translation.TranslationValue = "Alarm";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "EventLevelError");
        translation.TranslationValue = "Error";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "EventLevelError");
        translation.TranslationValue = "Erreur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "EventLevelError");
        translation.TranslationValue = "Fehler";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "EventLevelWarn");
        translation.TranslationValue = "Warn";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "EventLevelWarn");
        translation.TranslationValue = "Avertissement";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "EventLevelWarn");
        translation.TranslationValue = "Warnung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "EventLevelNotice");
        translation.TranslationValue = "Notice";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "EventLevelNotice");
        translation.TranslationValue = "Notification";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "EventLevelNotice");
        translation.TranslationValue = "Hinweis";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "EventLevelInfo");
        translation.TranslationValue = "Info";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "EventLevelInfo");
        translation.TranslationValue = "Info";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "EventLevelInfo");
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
        ITranslation translation = new Translation ("fr", "CadModelNull");
        translation.TranslationValue = "Aucun modèle CAO";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "CadModelNull");
        translation.TranslationValue = "Kein CAD-Modell";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "CncAcquisitionNull");
        translation.TranslationValue = "No cnc acquisition";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "CncAcquisitionNull");
        translation.TranslationValue = "Aucune acquisition CNC";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "CncAcquisitionNull");
        translation.TranslationValue = "Keine CNC-Erfassung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "CellNull");
        translation.TranslationValue = "No cell";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "CellNull");
        translation.TranslationValue = "Aucune cellule";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "CellNull");
        translation.TranslationValue = "Keine Zelle";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "CompanyNull");
        translation.TranslationValue = "No company";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "CompanyNull");
        translation.TranslationValue = "Aucune société";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "CompanyNull");
        translation.TranslationValue = "Kein Unternehmen";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ComputerNull");
        translation.TranslationValue = "No computer";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ComputerNull");
        translation.TranslationValue = "Aucun ordinateur";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ComputerNull");
        translation.TranslationValue = "Kein Computer";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "DateTimeNull");
        translation.TranslationValue = "No date/time";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "DateTimeNull");
        translation.TranslationValue = "Aucune date/heure";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "DateTimeNull");
        translation.TranslationValue = "Kein Datum/Uhrzeit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "DepartmentNull");
        translation.TranslationValue = "No department";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "DepartmentNull");
        translation.TranslationValue = "Aucun département";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "DepartmentNull");
        translation.TranslationValue = "Keine Abteilung";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "EventLevelNull");
        translation.TranslationValue = "No event level";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "EventLevelNull");
        translation.TranslationValue = "Aucun niveau d'événement";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "EventLevelNull");
        translation.TranslationValue = "Kein Ereignislevel";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FieldNull");
        translation.TranslationValue = "No field";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FieldNull");
        translation.TranslationValue = "Aucun champ";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FieldNull");
        translation.TranslationValue = "Kein Feld";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "FileRepositoryNull");
        translation.TranslationValue = "No file";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "FileRepositoryNull");
        translation.TranslationValue = "Aucun fichier";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "FileRepositoryNull");
        translation.TranslationValue = "Keine Datei";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineCategoryNull");
        translation.TranslationValue = "No machine category";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineCategoryNull");
        translation.TranslationValue = "Aucune catégorie de machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineCategoryNull");
        translation.TranslationValue = "Keine Maschinenkategorie";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineFilterNull");
        translation.TranslationValue = "No machine filter";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineFilterNull");
        translation.TranslationValue = "Aucun filtre de machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineFilterNull");
        translation.TranslationValue = "Kein Maschinenfilter";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModeNull");
        translation.TranslationValue = "No machine mode";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModeNull");
        translation.TranslationValue = "Aucun mode machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModeNull");
        translation.TranslationValue = "Kein Maschinenmodus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineModuleNull");
        translation.TranslationValue = "No machine module";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineModuleNull");
        translation.TranslationValue = "Aucun module de machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineModuleNull");
        translation.TranslationValue = "Kein Maschinenmodul";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineMonitoringTypeNull");
        translation.TranslationValue = "No monitoring type";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineMonitoringTypeNull");
        translation.TranslationValue = "Aucun type de surveillance";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineMonitoringTypeNull");
        translation.TranslationValue = "Kein Überwachungstyp";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineObservationStateNull");
        translation.TranslationValue = "No machine state";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineObservationStateNull");
        translation.TranslationValue = "Aucun état planifié de machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineObservationStateNull");
        translation.TranslationValue = "Kein Maschinenstatus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineStateTemplateNull");
        translation.TranslationValue = "No machine state template";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineStateTemplateNull");
        translation.TranslationValue = "Aucun calendrier d'état de machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineStateTemplateNull");
        translation.TranslationValue = "Kein Maschinenstatusvorlage";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MachineSubCategoryNull");
        translation.TranslationValue = "No machine sub-category";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MachineSubCategoryNull");
        translation.TranslationValue = "Aucune sous-catégorie de machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MachineSubCategoryNull");
        translation.TranslationValue = "Keine Maschinen-Unterkategorie";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "MonitoredMachineNull");
        translation.TranslationValue = "No monitored machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "MonitoredMachineNull");
        translation.TranslationValue = "Aucune machine surveillée";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "MonitoredMachineNull");
        translation.TranslationValue = "Keine überwachte Maschine";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "OperationCycleNull");
        translation.TranslationValue = "No operation cycle";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "OperationCycleNull");
        translation.TranslationValue = "Aucun cycle d'opération";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "OperationCycleNull");
        translation.TranslationValue = "Kein Betriebszyklus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "OperationSlotNull");
        translation.TranslationValue = "No operation slot";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "OperationSlotNull");
        translation.TranslationValue = "Aucun période avec opération";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "OperationSlotNull");
        translation.TranslationValue = "Kein Betriebszeitfenster";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonGroupNull");
        translation.TranslationValue = "No motion status group";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonGroupNull");
        translation.TranslationValue = "Aucun groupe d'arrêt machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonGroupNull");
        translation.TranslationValue = "Keine Bewegungsstatusgruppe";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ReasonReasonGroupNull");
        translation.TranslationValue = "No motion status";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ReasonReasonGroupNull");
        translation.TranslationValue = "Aucun groupe d'arrêt machine";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ReasonReasonGroupNull");
        translation.TranslationValue = "Kein Bewegungsstatus";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "RoleNull");
        translation.TranslationValue = "No role";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "RoleNull");
        translation.TranslationValue = "Aucun rôle";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "RoleNull");
        translation.TranslationValue = "Keine Rolle";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ShiftNull");
        translation.TranslationValue = "No shift";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ShiftNull");
        translation.TranslationValue = "Pas d'équipe";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ShiftNull");
        translation.TranslationValue = "Keine Schicht";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "TimePeriodOfDayNull");
        translation.TranslationValue = "No time period";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "TimePeriodOfDayNull");
        translation.TranslationValue = "Aucune période de la journée";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "TimePeriodOfDayNull");
        translation.TranslationValue = "Keine Tageszeit";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "ToolNull");
        translation.TranslationValue = "No tool";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "ToolNull");
        translation.TranslationValue = "Aucun outil";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "ToolNull");
        translation.TranslationValue = "Kein Werkzeug";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "TranslationKeyNull");
        translation.TranslationValue = "No translation key";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "TranslationKeyNull");
        translation.TranslationValue = "Aucune clé de traduction";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "TranslationKeyNull");
        translation.TranslationValue = "Kein Übersetzungsschlüssel";
        InsertDefaultValue (translation);
      }

      {
        ITranslation translation = new Translation ("", "UnitNull");
        translation.TranslationValue = "No unit";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("fr", "UnitNull");
        translation.TranslationValue = "Aucune unité";
        InsertDefaultValue (translation);
      }
      {
        ITranslation translation = new Translation ("de", "UnitNull");
        translation.TranslationValue = "Keine Einheit";
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
