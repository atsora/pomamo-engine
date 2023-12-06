// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Stamping.StampLineCreators;

namespace Lemoine.Stamping.StampingParsers
{
  /// <summary>
  /// Parse a file identifying basic comments on file: only comments that start with ; or with ()
  /// </summary>
  public class BasicCommentOnlyParser : IStampingParser
  {
    readonly ILog log = LogManager.GetLogger (typeof (BasicCommentOnlyParser).FullName);

    const double STAMP_VALUE_TEST = 1234.567;

    readonly IStampVariablesGetter? m_stampVariablesGetter;
    readonly ILineFormatter? m_lineFormatter;

    /// <summary>
    /// Constructor
    /// </summary>
    public BasicCommentOnlyParser (IStampVariablesGetter stampVariablesGetter, ILineFormatter lineFormatter)
    {
      m_stampVariablesGetter = stampVariablesGetter;
      m_lineFormatter = lineFormatter;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public BasicCommentOnlyParser ()
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
      case 'N' or 'n' or 'O' or 'o' when withLeadingNorO:
        await ParseLineAsync (line.Substring (1), false, true, stamper, stampingEventHandler, cancellationToken);
        break;
      case >= '0' and <= '9' when withLeadingNumber:
        await ParseLineAsync (line.Substring (1), false, true, stamper, stampingEventHandler, cancellationToken);
        break;
      case ' ':
        await ParseLineAsync (line.Substring (1), withLeadingNorO, withLeadingNumber && withLeadingNorO, stamper, stampingEventHandler, cancellationToken);
        break;
      case ';' or '(':
        stampVariable = ParseComment (line, stampingEventHandler);
        break;
      case '/' when line.StartsWith ("//"): // Roeders
        ParseComment (line, stampingEventHandler);
        break;
      case 'E' when line.StartsWith ("END PGM"): // Heidenhain
        stampingEventHandler.EndProgram (true, 0, false);
        break;
      case 'E' when line.StartsWith ("ERPSTATE"): // Roeders
        if (log.IsTraceEnabled) {
          log.Trace ($"ParseLineAsync: {line} => Roeders erpstate");
        }
        stampVariable = IsStamp (line);
        break;
      case 'F' when line.StartsWith ("FN 16:"): // Heidenhain
        if (log.IsTraceEnabled) {
          log.Trace ($"ParseLineAsync: {line} => Heidenhain F-PRINT");
        }
        stampVariable = IsStamp (line);
        if (stampVariable) {
          break;
        }
        else {
          goto default;
        }
      case 'P' when line.StartsWith ("PAR["): // DElectron
        if (log.IsTraceEnabled) {
          log.Trace ($"ParseLineAsync: {line} => DElectron PAR");
        }
        stampVariable = IsStamp (line);
        if (stampVariable) {
          break;
        }
        else {
          goto default;
        }
      case 'V' when line.StartsWith ("VC"): // Okuma
        if (log.IsTraceEnabled) {
          log.Trace ($"ParseLineAsync: {line} => Okuma VC");
        }
        stampVariable = IsStamp (line);
        if (stampVariable) {
          break;
        }
        else {
          goto default;
        }
      case '#':
        if (log.IsTraceEnabled) {
          log.Trace ($"ParseLineAsync: {line} => variable");
        }
        stampVariable = IsStamp (line); // Other option:  IsKeyEqualValueStampVariable (line.Substring (1));
        break;
      case '[':
        if (line.StartsWith ("[OT,")) { // Toshiba
          if (log.IsTraceEnabled) {
            log.Trace ($"ParseLineAsync: {line} => Toshiba dprint");
          }
          stampVariable = IsStamp (line);
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

    bool ParseComment (string s, IStampingEventHandler stampingEventHandler)
    {
      if (0 == s.Length) {
        return false;
      }

      switch (s.First ()) {
      case ';': {
        var comment = s.Substring (1).Trim ();
        if (0 < comment.Length) {
          if (comment.StartsWith ('(')) {
            ParseComment (comment, stampingEventHandler);
          }
          else {
            stampingEventHandler.SetComment (comment);
          }
        }
      }
      break;
      case '(': {
        var comment = s.Substring (1).TrimEnd (')', ' ').Trim ();
        if (0 < comment.Length) {
          stampingEventHandler.SetComment (comment);
          return IsKeyEqualValueStampVariable (comment); // e.g. for Perle
        }
      }
      break;
      case '/': {
        if (s.StartsWith ("//")) { // For Roeders
          var comment = s.Substring (2).Trim ();
          stampingEventHandler.SetComment (comment);
        }
      }
      break;
      default:
        break;
      }
      return false;
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
      if ( (0 < valuePosition) && (valuePosition < line.Length)) {
        var testSubString = test.Substring (0, valuePosition);
        var lineSubString = line.Substring (0, valuePosition);
        return string.Equals (testSubString, lineSubString);
      }
      else {
        return false;
      }
    }

    bool IsKeyEqualValueStampVariable (string s)
    {
      if (m_stampVariablesGetter is null) {
        return false;
      }
      var keyValue = s.Split ('=', 2, StringSplitOptions.TrimEntries);
      if (2 == keyValue.Length) {
        return m_stampVariablesGetter.IsStampVariable (keyValue[0]);
      }
      else {
        return false;
      }
    }
  }
}
