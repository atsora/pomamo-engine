// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1803: update the default translations after the terminology change
  /// "motion status" => "reason" and "machine observation state" => "planned state"
  ///
  /// Only the values that were not customized (that are still equal to a former default value) are updated
  /// </summary>
  [Migration (1803)]
  public class UpdateReasonPlannedStateTranslations : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UpdateReasonPlannedStateTranslations).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpdateTranslation ("ReasonGroupNull", "", "No reason group", "No motion status group");
      UpdateTranslation ("ReasonGroupNull", "fr", "Aucun groupe de raisons", "Aucun groupe d'arrêt machine");
      UpdateTranslation ("ReasonGroupNull", "de", "Keine Grundgruppe", "Keine Bewegungsstatusgruppe");
      UpdateTranslation ("ReasonReasonGroupNull", "", "No reason", "No motion status");
      UpdateTranslation ("ReasonReasonGroupNull", "fr", "Aucune raison", "Aucun groupe d'arrêt machine");
      UpdateTranslation ("ReasonReasonGroupNull", "de", "Kein Grund", "Kein Bewegungsstatus");
      UpdateTranslation ("MachineObservationStateNull", "", "No planned state", "No machine state");
      UpdateTranslation ("MachineObservationStateNull", "fr", "Aucun état planifié", "Aucun état planifié de machine");
      UpdateTranslation ("MachineObservationStateNull", "de", "Kein geplanter Zustand", "Kein Maschinenstatus");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      UpdateTranslation ("ReasonGroupNull", "", "No motion status group", "No reason group");
      UpdateTranslation ("ReasonGroupNull", "fr", "Aucun groupe d'arrêt machine", "Aucun groupe de raisons");
      UpdateTranslation ("ReasonGroupNull", "de", "Keine Bewegungsstatusgruppe", "Keine Grundgruppe");
      UpdateTranslation ("ReasonReasonGroupNull", "", "No motion status", "No reason");
      UpdateTranslation ("ReasonReasonGroupNull", "fr", "Aucun groupe d'arrêt machine", "Aucune raison");
      UpdateTranslation ("ReasonReasonGroupNull", "de", "Kein Bewegungsstatus", "Kein Grund");
      UpdateTranslation ("MachineObservationStateNull", "", "No machine state", "No planned state");
      UpdateTranslation ("MachineObservationStateNull", "fr", "Aucun état planifié de machine", "Aucun état planifié");
      UpdateTranslation ("MachineObservationStateNull", "de", "Kein Maschinenstatus", "Kein geplanter Zustand");
    }

    /// <summary>
    /// Set a new translation value if the current value is one of the specified former values
    /// </summary>
    /// <param name="key"></param>
    /// <param name="locale"></param>
    /// <param name="newValue"></param>
    /// <param name="oldValues"></param>
    void UpdateTranslation (string key, string locale, string newValue, params string[] oldValues)
    {
      var oldValueList = string.Join (",", oldValues.Select (v => $"'{Escape (v)}'"));
      Database.ExecuteNonQuery ($@"
UPDATE {TableName.TRANSLATION}
SET translationvalue='{Escape (newValue)}'
WHERE translationkey='{Escape (key)}'
  AND locale='{Escape (locale)}'
  AND translationvalue IN ({oldValueList})
");
    }

    static string Escape (string s) => s.Replace ("'", "''");
  }
}
