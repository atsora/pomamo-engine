// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Business.ShiftSummary;

namespace Lemoine.Business.ShiftSummary
{
  /// <summary>
  /// Activity data in a form that can be serialized in JSON: what may be given to a LLM
  /// </summary>
  [Serializable]
  public sealed class ShiftActivityDataDTO
  {
    /// <summary>
    /// Machine id
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Display of the machine
    /// </summary>
    public string Machine { get; set; }

    /// <summary>
    /// Day, yyyy-MM-dd
    /// </summary>
    public string Day { get; set; }

    /// <summary>
    /// Shift id
    /// </summary>
    public int? ShiftId { get; set; }

    /// <summary>
    /// Display of the shift
    /// </summary>
    public string Shift { get; set; }

    /// <summary>
    /// UTC start
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// UTC end
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// The shift is over
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// Items, the most important first
    /// </summary>
    public List<ShiftSummaryItemDTO> Items { get; set; } = new List<ShiftSummaryItemDTO> ();

    /// <summary>
    /// Copy of activity data
    /// </summary>
    /// <param name="data">not null</param>
    /// <returns></returns>
    public static ShiftActivityDataDTO From (IShiftActivityData data) => new ShiftActivityDataDTO {
      MachineId = data.MachineId,
      Machine = data.MachineDisplay,
      Day = data.Day.ToString ("yyyy-MM-dd"),
      ShiftId = data.ShiftId,
      Shift = data.ShiftDisplay,
      Start = data.Range.Lower.HasValue ? data.Range.Lower.Value : (DateTime?)null,
      End = data.Range.Upper.HasValue ? data.Range.Upper.Value : (DateTime?)null,
      Completed = data.Completed,
      Items = data.Items.Select (ShiftSummaryItemDTO.From).ToList (),
    };

    /// <summary>
    /// Serialize it in JSON
    /// </summary>
    /// <returns></returns>
    public string ToJson () => JsonSerializer.Serialize (this, ShiftSummaryValue.JsonOptions);
  }

  /// <summary>
  /// Shift summaries: read them (web services), and compute them (analysis service only)
  ///
  /// <item>a summary is created <see cref="Lead"/> before the end of its shift, with one
  /// section per contributor section (<see cref="IShiftActivityExtension"/>)</item>
  /// <item>the analysis of the machine computes one section per run, the sections that are
  /// flagged: at the creation, when relevant data changes (flag set by the contributors),
  /// and every <see cref="RefreshFrequency"/> in the window from <see cref="Lead"/> before the
  /// end of the shift to <see cref="After"/> after it for the sections that have no change
  /// notification</item>
  /// <item>the content of the fast generators (Raw) is rendered by the analysis of the
  /// machine after a section is computed, the content of the slow ones (LLM) by a background
  /// analysis, from the stored sections</item>
  /// The web services only read them
  /// </summary>
  public static class ShiftSummaryService
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSummaryService).FullName);

    static readonly string LEAD_KEY = "Business.ShiftSummary.Lead";
    static readonly TimeSpan LEAD_DEFAULT = TimeSpan.FromMinutes (30);
    static readonly string AFTER_KEY = "Business.ShiftSummary.After";
    static readonly TimeSpan AFTER_DEFAULT = TimeSpan.FromMinutes (10);
    static readonly string REFRESH_FREQUENCY_KEY = "Business.ShiftSummary.RefreshFrequency";
    static readonly TimeSpan REFRESH_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (5);
    static readonly string MIN_SECTION_INTERVAL_KEY = "Business.ShiftSummary.MinSectionInterval";
    static readonly TimeSpan MIN_SECTION_INTERVAL_DEFAULT = TimeSpan.FromMinutes (2);

    /// <summary>
    /// The summary of a shift is created this period before its end
    /// </summary>
    public static TimeSpan Lead => Lemoine.Info.ConfigSet.LoadAndGet (LEAD_KEY, LEAD_DEFAULT);

    /// <summary>
    /// End of the window of the periodic refresh, after the end of the shift
    /// </summary>
    public static TimeSpan After => Lemoine.Info.ConfigSet.LoadAndGet (AFTER_KEY, AFTER_DEFAULT);

    /// <summary>
    /// Frequency of the refresh of the sections that have no change notification, in the
    /// window around the end of the shift
    /// </summary>
    public static TimeSpan RefreshFrequency => Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_FREQUENCY_KEY, REFRESH_FREQUENCY_DEFAULT);

    /// <summary>
    /// Minimum period between two computations of a section: a section that is flagged again
    /// sooner keeps its flag and is computed later, so that a series of changes does not give
    /// a series of computations, whatever the contributor
    /// </summary>
    public static TimeSpan MinSectionInterval => Lemoine.Info.ConfigSet.LoadAndGet (MIN_SECTION_INTERVAL_KEY, MIN_SECTION_INTERVAL_DEFAULT);

    /// <summary>
    /// Is a date/time in the window of a period, from <see cref="Lead"/> before its end to
    /// <see cref="After"/> after it?
    /// </summary>
    /// <param name="end">UTC end of the period</param>
    /// <param name="dateTime">UTC</param>
    /// <returns></returns>
    public static bool IsInWindow (DateTime end, DateTime dateTime) =>
      (end.Subtract (Lead) <= dateTime) && (dateTime <= end.Add (After));

    /// <summary>
    /// Keys of the available generators, the default one first
    /// </summary>
    /// <returns>not null</returns>
    public static async Task<IList<string>> GetGeneratorsAsync () =>
      (await GetGeneratorExtensionsAsync ())
      .Select (x => x.Generator)
      .Distinct ()
      .ToList ();

    /// <summary>
    /// Available generators, the default one first
    /// </summary>
    /// <returns>not null</returns>
    public static async Task<IList<IShiftSummaryExtension>> GetGeneratorExtensionsAsync () =>
      (await ServiceProvider.GetAsync (new Lemoine.Business.Extension.GlobalExtensions<IShiftSummaryExtension> (ext => ext.Initialize ())))
      .OrderByDescending (x => x.Priority)
      .ToList ();

    /// <summary>
    /// Get the period of a machine for a day and a shift, or the current one
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="day">null: current period</param>
    /// <param name="shiftId">null: whole day, or current period if day is null</param>
    /// <returns>null if the machine does not exist or the period is not defined</returns>
    public static async Task<ShiftPeriod> GetPeriodAsync (int machineId, DateTime? day, int? shiftId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.ShiftSummary.Period")) {
          var machine = await ModelDAOHelper.DAOFactory.MachineDAO.FindByIdAsync (machineId);
          if (machine is null) {
            log.Error ($"GetPeriodAsync: machine {machineId} does not exist");
            return null;
          }
          if (!day.HasValue) {
            return await ShiftPeriod.FindAtAsync (machine, DateTime.UtcNow);
          }
          var shift = shiftId.HasValue
            ? await ModelDAOHelper.DAOFactory.ShiftDAO.FindByIdAsync (shiftId.Value)
            : null;
          if (shiftId.HasValue && shift is null) {
            log.Error ($"GetPeriodAsync: shift {shiftId} does not exist");
            return null;
          }
          return await ShiftPeriod.FindAsync (machine, day.Value, shift);
        }
      }
    }

    /// <summary>
    /// Get the period that precedes the current one on a machine
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns>nullable</returns>
    public static async Task<ShiftPeriod> GetPreviousPeriodAsync (int machineId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.ShiftSummary.PreviousPeriod")) {
          var machine = await ModelDAOHelper.DAOFactory.MachineDAO.FindByIdAsync (machineId);
          if (machine is null) {
            log.Error ($"GetPreviousPeriodAsync: machine {machineId} does not exist");
            return null;
          }
          var current = await ShiftPeriod.FindAtAsync (machine, DateTime.UtcNow);
          if (current is null) {
            return null;
          }
          return await ShiftPeriod.FindPreviousAsync (machine, current);
        }
      }
    }

    /// <summary>
    /// Get the summary of a period, as a generator rendered it
    ///
    /// If there is no content for the requested generator yet (a LLM that did not answer
    /// yet, for example), the content of the default generator is returned, else any content
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="period">not null</param>
    /// <param name="generator">null or empty: default generator</param>
    /// <returns>nullable: not created yet</returns>
    public static async Task<ShiftSummaryValue> FindAsync (int machineId, ShiftPeriod period, string generator)
    {
      Debug.Assert (period != null);

      var keys = new List<string> ();
      if (!string.IsNullOrEmpty (generator)) {
        keys.Add (generator);
      }
      keys.AddRange (await GetGeneratorsAsync ());

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.ShiftSummary.Find")) {
          var machine = await ModelDAOHelper.DAOFactory.MachineDAO.FindByIdAsync (machineId);
          if (machine is null) {
            return null;
          }
          var shift = period.ShiftId.HasValue
            ? await ModelDAOHelper.DAOFactory.ShiftDAO.FindByIdAsync (period.ShiftId.Value)
            : null;
          var shiftSummary = await ModelDAOHelper.DAOFactory.ShiftSummaryDAO
            .FindAsync (machine, period.Day, shift);
          if (shiftSummary is null) {
            return null;
          }
          var contents = await ModelDAOHelper.DAOFactory.ShiftSummaryContentDAO.FindBySummaryAsync (shiftSummary);
          var content = keys
            .Select (k => contents.FirstOrDefault (c => string.Equals (c.Generator, k, StringComparison.InvariantCultureIgnoreCase)))
            .FirstOrDefault (c => c != null)
            ?? contents.FirstOrDefault ();
          if (content is null) {
            return null;
          }
          var sections = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindBySummaryAsync (shiftSummary);
          return ShiftSummaryValue.From (content, sections);
        }
      }
    }

    /// <summary>
    /// Get a content by its id
    ///
    /// To be run inside a session
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns>nullable</returns>
    public static async Task<ShiftSummaryValue> FindContentAsync (int contentId)
    {
      var content = await ModelDAOHelper.DAOFactory.ShiftSummaryContentDAO.FindByIdAsync (contentId);
      if (content is null) {
        return null;
      }
      var sections = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindBySummaryAsync (content.ShiftSummary);
      return ShiftSummaryValue.From (content, sections);
    }

    /// <summary>
    /// Create the summary of a period with its sections if it does not exist yet, and add the
    /// sections that are missing (a new contributor)
    ///
    /// To be run by the analysis service only, out of any session
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="period">not null</param>
    /// <param name="sectionKeys">not null</param>
    /// <returns>the summary was created, or a section added</returns>
    public static async Task<bool> EnsureAsync (IMachine machine, ShiftPeriod period, IEnumerable<string> sectionKeys)
    {
      Debug.Assert (machine != null);
      Debug.Assert (period != null);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Business.ShiftSummary.Ensure")) {
          var shift = period.ShiftId.HasValue
            ? await ModelDAOHelper.DAOFactory.ShiftDAO.FindByIdAsync (period.ShiftId.Value)
            : null;
          var shiftSummary = await ModelDAOHelper.DAOFactory.ShiftSummaryDAO
            .FindAsync (machine, period.Day, shift);
          var created = false;
          IList<IShiftSummarySection> sections;
          if (shiftSummary is null) {
            shiftSummary = ModelDAOHelper.ModelFactory.CreateShiftSummary (machine, period.Day, shift, period.Range);
            await ModelDAOHelper.DAOFactory.ShiftSummaryDAO.MakePersistentAsync (shiftSummary);
            sections = new List<IShiftSummarySection> ();
            created = true;
          }
          else {
            sections = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindBySummaryAsync (shiftSummary);
          }
          foreach (var sectionKey in sectionKeys.Where (k => !sections.Any (s => s.SectionKey == k))) {
            var section = ModelDAOHelper.ModelFactory.CreateShiftSummarySection (shiftSummary, sectionKey);
            await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.MakePersistentAsync (section);
            created = true;
          }
          transaction.Commit ();
          return created;
        }
      }
    }

    /// <summary>
    /// Compute a section with its contributor and store it
    ///
    /// To be run by the analysis service only, out of any session
    /// </summary>
    /// <param name="sectionId"></param>
    /// <param name="extension">contributor of the section, not null</param>
    /// <param name="dataUntil">UTC date/time until which the data is processed</param>
    /// <param name="cancellationToken"></param>
    /// <returns>id of the summary of the section, null if the section does not exist any more</returns>
    public static async Task<int?> ComputeSectionAsync (int sectionId, IShiftActivityExtension extension, DateTime? dataUntil, CancellationToken cancellationToken)
    {
      Debug.Assert (extension != null);

      int shiftSummaryId;
      List<ShiftSummaryItemDTO> items;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.ShiftSummary.ComputeSection")) {
          var section = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindByIdAsync (sectionId);
          if (section is null) {
            return null;
          }
          var shiftSummary = section.ShiftSummary;
          shiftSummaryId = shiftSummary.Id;
          items = (await extension.GetItemsAsync (section.SectionKey, shiftSummary.Machine, shiftSummary.Day, shiftSummary.Shift, shiftSummary.DateTimeRange, cancellationToken))
            .Select (ShiftSummaryItemDTO.From)
            .ToList ();
        }
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Business.ShiftSummary.StoreSection")) {
          var section = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindByIdAsync (sectionId);
          if (section is null) {
            return null;
          }
          section.Items = JsonSerializer.Serialize (items, ShiftSummaryValue.JsonOptions);
          section.DataUntil = dataUntil;
          section.DateTime = DateTime.UtcNow;
          section.UpdateRequired = false;
          await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.MakePersistentAsync (section);
          transaction.Commit ();
          return shiftSummaryId;
        }
      }
    }

    /// <summary>
    /// Is the content of a generator out of date?
    ///
    /// To be run inside a session
    /// </summary>
    /// <param name="shiftSummary">not null</param>
    /// <param name="generator">not null and not empty</param>
    /// <returns></returns>
    public static async Task<bool> IsContentOutOfDateAsync (IShiftSummary shiftSummary, string generator)
    {
      var sections = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindBySummaryAsync (shiftSummary);
      if (!sections.Any (x => x.DateTime.HasValue)) {
        return false; // Nothing to render yet
      }
      var content = await ModelDAOHelper.DAOFactory.ShiftSummaryContentDAO.FindAsync (shiftSummary, generator);
      return (content is null)
        || !string.Equals (content.SourceVersion, ShiftSummaryValue.GetSourceVersion (sections), StringComparison.Ordinal);
    }

    /// <summary>
    /// Render the content of a summary with a generator, from its stored sections, and store it
    ///
    /// The data of the machine is not read again. To be run by the analysis service only, out
    /// of any session: the fast generators by the analysis of the machine, the slow ones by a
    /// background analysis
    /// </summary>
    /// <param name="shiftSummaryId"></param>
    /// <param name="generator">not null</param>
    /// <param name="cancellationToken"></param>
    /// <returns>the content was rendered</returns>
    public static async Task<bool> RenderAsync (int shiftSummaryId, IShiftSummaryExtension generator, CancellationToken cancellationToken)
    {
      Debug.Assert (generator != null);

      IShiftActivityData data;
      string sourceVersion;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.ShiftSummary.Render.Data")) {
          var shiftSummary = await ModelDAOHelper.DAOFactory.ShiftSummaryDAO.FindByIdAsync (shiftSummaryId);
          if (shiftSummary is null) {
            return false;
          }
          var sections = await ModelDAOHelper.DAOFactory.ShiftSummarySectionDAO.FindBySummaryAsync (shiftSummary);
          sourceVersion = ShiftSummaryValue.GetSourceVersion (sections);
          data = ShiftActivityData.FromSections (shiftSummary, sections, DateTime.UtcNow);
        }
      }

      // Out of any session: a slow generator does not keep a connection open
      var result = await generator.CreateSummaryAsync (data, cancellationToken);
      if (result is null) {
        log.Error ($"RenderAsync: generator {generator.Generator} returned no summary for summary {shiftSummaryId}");
        return false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Business.ShiftSummary.Render.Store")) {
          var shiftSummary = await ModelDAOHelper.DAOFactory.ShiftSummaryDAO.FindByIdAsync (shiftSummaryId);
          if (shiftSummary is null) {
            return false;
          }
          var content = await ModelDAOHelper.DAOFactory.ShiftSummaryContentDAO.FindAsync (shiftSummary, generator.Generator)
            ?? ModelDAOHelper.ModelFactory.CreateShiftSummaryContent (shiftSummary, generator.Generator);
          content.Content = JsonSerializer.Serialize (ShiftSummaryContentDTO.From (result), ShiftSummaryValue.JsonOptions);
          content.Text = result.Text;
          content.SourceVersion = sourceVersion;
          content.DateTime = DateTime.UtcNow;
          await ModelDAOHelper.DAOFactory.ShiftSummaryContentDAO.MakePersistentAsync (content);
          transaction.Commit ();
          return true;
        }
      }
    }
  }
}
