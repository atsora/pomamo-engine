// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.StampingParsers
{
  /// <summary>
  /// Parse a file identifying basic comments on Selca NC program: only comments that start with [
  /// </summary>
  public class SelcaCommentOnlyParser : IStampingParser
  {
    readonly ILog log = LogManager.GetLogger (typeof (SelcaCommentOnlyParser).FullName);

    const double STAMP_VALUE_TEST = 1234.567;

    readonly IStampVariablesGetter? m_stampVariablesGetter;
    readonly ILineFormatter? m_lineFormatter;

    /// <summary>
    /// Constructor
    /// </summary>
    public SelcaCommentOnlyParser (IStampVariablesGetter stampVariablesGetter, ILineFormatter lineFormatter)
    {
      m_stampVariablesGetter = stampVariablesGetter;
      m_lineFormatter = lineFormatter;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SelcaCommentOnlyParser ()
    {
      m_stampVariablesGetter = null;
      m_lineFormatter = null;
    }

    /// <summary>
    /// <see cref="IStampingParser"/>
    /// </summary>
    public StamperLineFeed LineFeed => StamperLineFeed.After;

    /// <summary>
    /// <see cref="IStampingParser"/>
    /// </summary>
    /// <param name="stamper"></param>
    /// <param name="stampingEventHandler"></param>
    /// <param name="stampingData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ParseAsync (IStamper stamper, IStampingEventHandler stampingEventHandler, StampingData stampingData, CancellationToken cancellationToken = default)
    {
      var reader = stamper.Reader;
      if (reader is null) {
        log.Error ($"ParseAsync: reader is null");
        return true;
      }

      stampingEventHandler.StartProgram (true, 0);

      string? line;
      while (!cancellationToken.IsCancellationRequested
        && ((line = await reader.ReadLineAsync ()) is not null)) {
        await ParseLineAsync (line, true, true, stamper, stampingEventHandler, cancellationToken);
      }

      stampingEventHandler.EndProgram (true, 0, true);

      return !cancellationToken.IsCancellationRequested;
    }

    async Task ParseLineAsync (string line, bool withLeadingNorO, bool withLeadingNumber, IStamper stamper, IStampingEventHandler stampingEventHandler, CancellationToken cancellationToken)
    {
      if (0 == line.Length) {
        return;
      }

      var stampVariable = false;
      switch (line.First ()) {
        // Note: not sure if it is part of the syntax to start a line with N or a number
        case 'N' or 'n' or 'O' or 'o' when withLeadingNorO:
          await ParseLineAsync (line.Substring (1), false, true, stamper, stampingEventHandler, cancellationToken);
          return;
        case >= '0' and <= '9' when withLeadingNumber:
          await ParseLineAsync (line.Substring (1), false, true, stamper, stampingEventHandler, cancellationToken);
          return;
        case ' ':
          await ParseLineAsync (line.Substring (1), withLeadingNorO, false, stamper, stampingEventHandler, cancellationToken);
          return;
        case '[' when line.StartsWith ("[ENDSTAMP"):
          stampingEventHandler.EndProgram (true, 0, false);
          break;
        case '[':
          ParseComment (line, stampingEventHandler);
          break;
        case 'M' when line.StartsWith ("M30"):
          stampingEventHandler.EndProgram (true, 0, false);
          break;
        case 'P':
          if (log.IsTraceEnabled) {
            log.Trace ($"ParseLineAsync: {line} => P command");
          }
          stampVariable = IsStamp (line);
          if (stampVariable) {
            break;
          }
          else {
            goto default;
          }
        default:
          if (log.IsTraceEnabled) {
            log.Trace ($"ParseLineAsync: {line} => machining");
          }
          stampingEventHandler.TriggerMachining ();
          break;
      }

      if (stampVariable) {
        await stamper.SkipAsync (cancellationToken);
      }
      else {
        await stamper.ReleaseAsync (cancellationToken);
      }
    }

    void ParseComment (string s, IStampingEventHandler stampingEventHandler)
    {
      if (0 == s.Length) {
        return;
      }

      switch (s.First ()) {
        case '[': {
            var comment = s.Substring (1).TrimEnd (']', ' ').Trim ();
            stampingEventHandler.SetComment (comment);
          }
          break;
        default:
          break;
      }
    }

    public bool IsStamp (string s)
    {
      if (string.IsNullOrEmpty (s.Trim ())) {
        return false;
      }

      if ((m_stampVariablesGetter is not null) && (m_lineFormatter is not null)) {
        if (TestStamp (s, m_stampVariablesGetter.SequenceStampVariable)) {
          return true;
        }
        if (TestStamp (s, m_stampVariablesGetter.StartCycleStampVariable)) {
          return true;
        }
        if (TestStamp (s, m_stampVariablesGetter.StopCycleStampVariable)) {
          return true;
        }
        if (TestStamp (s, m_stampVariablesGetter.MilestoneStampVariable)) {
          return true;
        }
      }
      return false;
    }

    bool TestStamp (string line, string variable)
    {
      if (string.IsNullOrEmpty (variable)) {
        return false;
      }
      if (m_lineFormatter is null) {
        return false;
      }
      var test = m_lineFormatter.CreateLine (variable, STAMP_VALUE_TEST);
      if (test.Contains ('\n')) {
        // TODO: ...
        // Note: this is not supported for the moment
        return false;
      }
      else {
        return TestStampLineWithValue (line, test);
      }
    }

    bool TestStampLineWithValue (string line, string test)
    {
      if (string.IsNullOrEmpty (test)) {
        return false;
      }
      var valuePosition = test.IndexOf (STAMP_VALUE_TEST.ToString (System.Globalization.CultureInfo.InvariantCulture));
      if ((0 < valuePosition) && (valuePosition < line.Length)) {
        var testSubString = test.Substring (0, valuePosition);
        var lineSubString = line.Substring (0, valuePosition);
        return string.Equals (testSubString, lineSubString);
      }
      else {
        return false;
      }
    }
  }
}
