// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pulse.Extensions.Web.Reason
{
  /// <summary>
  /// Extension that drives the reason configuration of the web service
  ///
  /// A reason configuration screen must not list the reason table: what the operator ends
  /// up selecting is produced by every implementation of IReasonSelectionExtension, and some
  /// of them invent reasons no row of the reason table carries - a machine state template
  /// presented as a reason, for instance. Each of them exposes here what it configures
  ///
  /// An extension does two independent things:
  /// <item>it <b>declares</b> reasons - identity and presentation;</item>
  /// <item>it <b>contributes</b> selection and default entries to reasons, whether it
  /// declared them or not.</item>
  ///
  /// A caller concatenates what the extensions return. On a write, it offers the request to
  /// the extensions in the order of the priorities: each reads the options of the reason to
  /// tell whether it is one of its own
  ///
  /// <b>An id is declared by one extension only.</b> Two extensions that would declare the
  /// same one are a misconfiguration - typically two alternative ways of listing the reasons
  /// of a table, installed together: the caller logs an error and keeps the declaration of
  /// the most prioritary extension, dropping the others
  ///
  /// Everything the extension returns carries ids and values only, never an entity, so that
  /// a business request may cache it
  ///
  /// What a client is allowed to see and set is not decided here but by
  /// <see cref="IReasonConfigPropertiesExtension"/>: this interface says what an
  /// implementation is able to do
  /// </summary>
  public interface IReasonConfigExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension. Return true if the extension is active, else false
    /// </summary>
    /// <returns>the extension is active</returns>
    bool Initialize ();

    /// <summary>
    /// Priority of the extension, highest first
    ///
    /// It orders the extensions a write is offered to, the options of the reason telling
    /// which of them takes it
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// The options this extension declares, so that a configuration screen may propose them
    /// without knowing the extension
    /// </summary>
    IEnumerable<IReasonConfigAvailableOption> AvailableOptions { get; }

    /// <summary>
    /// The reasons this extension declares
    ///
    /// It may be empty: an extension may only contribute selection or default entries
    /// </summary>
    /// <returns>not null</returns>
    Task<IEnumerable<IReasonConfigReason>> GetReasonsAsync ();

    /// <summary>
    /// The selection entries this extension contributes
    /// </summary>
    /// <returns>not null</returns>
    Task<IEnumerable<IReasonConfigSelection>> GetSelectionsAsync ();

    /// <summary>
    /// The default entries this extension contributes
    /// </summary>
    /// <returns>not null</returns>
    Task<IEnumerable<IReasonConfigDefault>> GetDefaultsAsync ();

    /// <summary>
    /// Create a reason
    ///
    /// The extension reads the options of the request to tell whether the reason is one of
    /// its own
    /// </summary>
    /// <param name="reason">not null, with no <see cref="IReasonConfigReason.ClassificationId"/> yet</param>
    /// <returns>the created reason, null when the extension does not take it</returns>
    Task<IReasonConfigReason> CreateReasonAsync (IReasonConfigReason reason);

    /// <summary>
    /// Apply the part of the reason this extension owns
    ///
    /// The reason is complete: the caller has read the current one and applied the
    /// properties the client provided. Every extension is offered it, and each writes what
    /// it owns - the one that declared the reason writes its presentation, the ones that
    /// contribute its selection or default entries write theirs
    /// </summary>
    /// <param name="reason">not null</param>
    /// <returns>the extension applied something</returns>
    Task<bool> UpdateReasonAsync (IReasonConfigReason reason);

    /// <summary>
    /// Archive a reason: remove or disable every selection entry and every default value
    /// that makes it reachable, so that it is not proposed any more
    ///
    /// Every extension is offered the id and archives what it owns
    /// </summary>
    /// <param name="classificationId">not null and not empty, see <see cref="IReasonConfigReason.ClassificationId"/></param>
    /// <returns>the extension archived something</returns>
    Task<bool> ArchiveReasonAsync (string classificationId);
  }
}
