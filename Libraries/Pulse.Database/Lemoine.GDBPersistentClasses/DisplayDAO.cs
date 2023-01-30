// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IDisplayDAO">IDisplayDAO</see>
  /// </summary>
  public class DisplayDAO
    : VersionableNHibernateDAO<Display, IDisplay, int>
    , IDisplayDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DisplayDAO).FullName);

    static readonly string RESET_EMPTY_DISPLAY_PATTERN_KEY = "DefaultValues.Display.ResetEmptyPattern";
    static readonly bool RESET_EMPTY_DISPLAY_PATTERN_DEFAULT = false;

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    /// <param name="migrations"></param>
    /// <returns>Completion</returns>
    internal bool InsertDefaultValues (IList<long> migrations)
    {
      if (!migrations.Contains (335)) {
        return false;
      }

      bool completed = true;
      {
        IDisplay display = new Display ("Unit");
        display.Pattern = "<%NameOrTranslation%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Unit");
        display.Variant = "Brackets";
        display.Description = "In brackets: (). To be used with a field property for example";
        display.Pattern = "(<%Display%>)";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Buffer");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Cell");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Company");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Project");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Component");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Component");
        display.Pattern = "<%Display%>/"; // For mold shops, replace it by "<%Project.Display%>/<%Display%>/"
        display.Variant = "OperationSlot";
        display.Description = "To be used by OperationSlot";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Computer");
        display.Pattern = "<%Name%> (<%Address%>)";
        InsertDefaultValue (display);
      }
      if (migrations.Contains (1200)) {
        IDisplay display = new Display ("Customer");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      else {
        completed = false;
      }
      {
        IDisplay display = new Display ("DeliverablePiece");
        display.Pattern = "<%Code%> <%Component.Display%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Department");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Field");
        display.Pattern = "<%NameOrTranslation%> <%Unit.Display_Brackets%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("IntermediateWorkPiece");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      if (migrations.Contains (356)) {
        IDisplay display = new Display ("IntermediateWorkPiece");
        display.Pattern = "<%Component.Display%> <%Display%>";
        display.Variant = "Long";
        display.Description = "To be used when the intermediate work piece is alone";
        InsertDefaultValue (display);
      }
      else {
        completed = false;
      }
      {
        IDisplay display = new Display ("IntermediateWorkPiece");
        display.Pattern = "<%Display%>";
        display.Variant = "Short";
        display.Description = "To be used in combination with a Component/Part";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("IsoFile");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Job");
        display.Pattern = "<%Project.Display%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Line");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Machine");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("MachineModule");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("MachineCategory");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("MachineSubCategory");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("NonConformanceReason");
        display.Pattern = "<%Code%>: <%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Operation");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Operation");
        display.Pattern = "<%Display%>";
        display.Variant = "OperationSlot";
        display.Description = "To be used by OperationSlot";
        InsertDefaultValue (display);
      }
      if (migrations.Contains (356)) {
        IDisplay display = new Display ("Operation");
        display.Pattern = "<%Component.Display%> <%Display%>";
        display.Variant = "Long";
        display.Description = "To be used when the operation is alone";
        InsertDefaultValue (display);
      }
      else {
        completed = false;
      }
      {
        IDisplay display = new Display ("Operation");
        display.Pattern = "<%Display%>";
        display.Variant = "Short";
        display.Description = "To be used in combination with a Component/Part";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("WorkOrder");
        display.Pattern = "<%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("WorkOrder");
        display.Pattern = "<%Display%>/"; // Or for mold shops, replace it by ""
        display.Variant = "OperationSlot";
        display.Description = "To be used by OperationSlot";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("OperationSlot");
        display.Pattern = "<%WorkOrder.Display_OperationSlot%><%Component.Display_OperationSlot%><%Operation.Display_OperationSlot%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Part");
        display.Pattern = "<%Component.Display%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Path");
        display.Pattern = "<%Number%>";
        InsertDefaultValue (display);
      }
      if (migrations.Contains (1005)) {
        {
          IDisplay display = new Display ("ProductionState");
          display.Pattern = "<%NameOrTranslation%>";
          InsertDefaultValue (display);
        }
        {
          IDisplay display = new Display ("ProductionState");
          display.Pattern = "<%Display%> - <%DescriptionOrTranslation%>";
          display.Variant = "Long";
          display.Description = "Production state with its description";
          InsertDefaultValue (display);
        }
      }
      else {
        completed = false;
      }
      {
        IDisplay display = new Display ("Reason");
        display.Pattern = "<%NameOrTranslation%>";
        InsertDefaultValue (display);
      }
      if (migrations.Contains (802)) {
        {
          IDisplay display = new Display ("Reason");
          display.Pattern = "<%Display%> - <%DescriptionOrTranslation%>";
          display.Variant = "Long";
          display.Description = "Reason with its description";
          InsertDefaultValue (display);
        }
      }
      else {
        completed = false;
      }
      {
        IDisplay display = new Display ("ReasonGroup");
        display.Pattern = "<%NameOrTranslation%>";
        InsertDefaultValue (display);
      }
      if (migrations.Contains (802)) {
        IDisplay display = new Display ("ReasonGroup");
        display.Pattern = "<%Display%> - <%DescriptionOrTranslation%>";
        display.Variant = "Long";
        display.Description = "Reason group with its description";
        InsertDefaultValue (display);
      }
      else {
        completed = false;
      }
      {
        IDisplay display = new Display ("Tool");
        display.Pattern = "(<%Diameter4%> <%Radius4%>)";
        display.Variant = "Size";
        display.Description = "Only the size of the tool";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Tool");
        display.Pattern = "<%Code%>: <%Name%> <%Display_Size%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Sequence");
        display.Pattern = "<%Name%> <%Tool.Display%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Sequence");
        display.Pattern = "<%Display%>";
        display.Variant = "SequenceSlot";
        display.Description = "To be used by SequenceSlot";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("SequenceSlot");
        display.Pattern = "<%Sequence.Display_SequenceSlot%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Service");
        display.Pattern = "<%Name%> on <%Computer.Display%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("SimpleOperation");
        display.Pattern = "<%Operation.Display%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("Shift");
        display.Pattern = "<%Code%> <%Name%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("TaskFull");
        display.Pattern = "<%WorkOrder.Display%>/<%Component.Display%>/<%Operation.Display%> on <%Machine.Display%>";
        InsertDefaultValue (display);
      }
      {
        IDisplay display = new Display ("User");
        display.Pattern = "<%Login%> (<%Name%>)";
        InsertDefaultValue (display);
      }
      if (migrations.Contains (512)) {
        IDisplay display = new Display ("ToolPosition");
        display.Pattern = "T<%ToolNumber%>";
        InsertDefaultValue (display);
      }
      else {
        completed = false;
      }
      if (migrations.Contains (518)) { // CncAlarmSeverity
        {
          IDisplay display = new Display ("CncAlarmSeverity");
          display.Pattern = "<%Name%>";
          InsertDefaultValue (display);
        }
        {
          IDisplay display = new Display ("CncAlarmSeverity");
          display.Pattern = "(<%Display%>)";
          display.Variant = "Brackets";
          display.Description = "To be used in CurrentCncAlarm or CncAlarm";
          InsertDefaultValue (display);
        }
        {
          IDisplay display = new Display ("CncAlarm");
          display.Pattern = "<%Message%> <%CncAlarmSeverity.Display_Brackets%>";
          InsertDefaultValue (display);
        }
      }
      else {
        completed = false;
      }

      if (migrations.Contains (522)) { // CurrentCncAlarm
        IDisplay display = new Display ("CurrentCncAlarm");
        display.Pattern = "<%Message%> <%CncAlarmSeverity.Display_Brackets%>";
        InsertDefaultValue (display);
      }
      else {
        completed = false;
      }

      return completed;
    }

    private void InsertDefaultValue (IDisplay display)
    {
      Debug.Assert (!"".Equals (display.Variant));
      if ("".Equals (display.Variant)) {
        log.ErrorFormat ("InsertDefaultValue: display with a variant which is an empty string, reset it to null");
        display.Variant = null;
      }

      var existing = FindWithTableVariant (display.Table, display.Variant);
      if (null == existing) { // the display does not exist => create it
        log.InfoFormat ("InsertDefaultValue: " +
                        "add table={0} pattern={1} variant={2}",
                        display.Table, display.Pattern, display.Variant);
        MakePersistent (display);
      }
      else { // null != existing
        if ((null != existing.Variant) && existing.Variant.Equals ("")) { // Correct the variant value
          existing.Variant = null;
          MakePersistent (existing);
        }
        if (!string.IsNullOrEmpty (display.Pattern)) { // null != existing and a default pattern is defined
          var oldPattern = existing.Pattern;
          if (string.IsNullOrEmpty (oldPattern)
              || string.IsNullOrEmpty (oldPattern.Trim ())) { // Empty pattern, check its default value
            var resetEmptyDisplayPattern = Lemoine.Info.ConfigSet.LoadAndGet<bool> (RESET_EMPTY_DISPLAY_PATTERN_KEY, RESET_EMPTY_DISPLAY_PATTERN_DEFAULT);
            if (resetEmptyDisplayPattern) {
              log.WarnFormat ("InsertDefaultValue: " +
                              "empty pattern for table={0} variant={2} " +
                              "=> update it to {1}",
                             display.Table, display.Pattern, display.Variant);
              MakeTransient (existing);
              ModelDAOHelper.DAOFactory.FlushData ();
              MakePersistent (display);
            }
            else {
              log.WarnFormat ("InsertDefaultValue: empty pattern for table={0} variant={1} but leave it unchanged",
                display.Table, display.Variant);
            }
          }
        }
      }
      ModelDAOHelper.DAOFactory.FlushData ();
    }
    #endregion // DefaultValues

    /// <summary>
    /// Find the IDisplay object for the given table and the null variant
    /// 
    /// Return null if not found
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="table">not null or empty</param>
    /// <returns></returns>
    public IDisplay FindWithTable (string table)
    {
      Debug.Assert (!string.IsNullOrEmpty (table));

      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Display> ()
        .Add (Restrictions.Eq ("Table", table))
        .Add (Restrictions.IsNull ("Variant"))
        .SetCacheable (true)
        .UniqueResult<IDisplay> ();
      if (null != result) {
        return result;
      }
      else { // Try with the empty variant (not null)
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Display> ()
          .Add (Restrictions.Eq ("Table", table))
          .Add (Restrictions.Eq ("Variant", ""))
          .SetCacheable (true)
          .UniqueResult<IDisplay> ();
      }
    }

    /// <summary>
    /// Find the IDisplay object for the specified table and variant
    /// 
    /// Return null if not found
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="table">not null or empty</param>
    /// <param name="variant">nullable</param>
    /// <returns></returns>
    public IDisplay FindWithTableVariant (string table, string variant)
    {
      Debug.Assert (!string.IsNullOrEmpty (table));

      if (!string.IsNullOrEmpty (variant)) {
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Display> ()
          .Add (Restrictions.Eq ("Table", table))
          .Add (Restrictions.Eq ("Variant", variant))
          .SetCacheable (true)
          .UniqueResult<IDisplay> ();
      }
      else {
        return FindWithTable (table);
      }
    }

    /// <summary>
    /// Find the IDisplay object for the given table
    /// 
    /// If the variant is not found, the generic display is returned
    /// 
    /// Return null if not found
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="table">not null or empty</param>
    /// <param name="variant">nullable</param>
    /// <returns></returns>
    public IDisplay FindWithTableVariantWithDefault (string table, string variant)
    {
      Debug.Assert (!string.IsNullOrEmpty (table));

      if (!string.IsNullOrEmpty (variant)) {
        IDisplay display = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Display> ()
          .Add (Restrictions.Eq ("Table", table))
          .Add (Restrictions.Eq ("Variant", variant))
          .SetCacheable (true)
          .UniqueResult<IDisplay> ();
        if (null != display) {
          return display;
        }
      }
      // Fallback: try without the variant
      return FindWithTable (table);
    }

    /// <summary>
    /// Get the pattern to use for the given table and pattern
    /// 
    /// If the specified table was not found,
    /// return an empty string
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public string GetPattern (string table)
    {
      IDisplay display = FindWithTable (table);
      if (null == display) {
        log.ErrorFormat ("GetPattern: " +
                         "no pattern for table {0} " +
                         "=> return an empty string",
                         table);
        return "";
      }
      else {
        return display.Pattern;
      }
    }

    /// <summary>
    /// Get the pattern to use for the given table, pattern and display
    /// 
    /// If the specified table was not found,
    /// return an empty string
    /// </summary>
    /// <param name="table"></param>
    /// <param name="variant"></param>
    /// <returns></returns>
    public string GetPatternWithDefault (string table, string variant)
    {
      IDisplay display = FindWithTableVariantWithDefault (table, variant);
      if (null == display) {
        log.ErrorFormat ("GetPattern: " +
                         "no pattern for table {0} and variant {1}" +
                         "=> return an empty string",
                         table, variant);
        return "";
      }
      else {
        return display.Pattern;
      }
    }
  }
}
