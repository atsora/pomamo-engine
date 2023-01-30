// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Parse a file identifying basic comments on Fidia NC program: only comments that start with / or with ()
  /// </summary>
  public class FidiaCommentOnlyParser : IStampingParser
  {
    readonly ILog log = LogManager.GetLogger (typeof (FidiaCommentOnlyParser).FullName);

    readonly IStampVariablesGetter? m_stampVariablesGetter;

    int m_stampLineNumber = 0;
    bool m_activeStamp = false;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="stampVariablesGetter"></param>
    public FidiaCommentOnlyParser (IStampVariablesGetter stampVariablesGetter)
    {
      m_stampVariablesGetter = stampVariablesGetter;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public FidiaCommentOnlyParser ()
    {
      m_stampVariablesGetter = null;
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
        return;
      case >= '0' and <= '9' when withLeadingNumber:
        await ParseLineAsync (line.Substring (1), false, true, stamper, stampingEventHandler, cancellationToken);
        return;
      case ' ':
        await ParseLineAsync (line.Substring (1), withLeadingNorO, false, stamper, stampingEventHandler, cancellationToken);
        return;
      case '(' when line.StartsWith ("(AUCREG 97)") && (0 == m_stampLineNumber) && !m_activeStamp:
        ++m_stampLineNumber;
        m_activeStamp = true;
        return;
      case '(':
      case '/': 
        m_stampLineNumber = 0;
        m_activeStamp = false;
        ParseComment (line, stampingEventHandler);
        break;
      case 'G' when line.StartsWith ("G201 H0") && (0 == m_stampLineNumber) && !m_activeStamp:
        ++m_stampLineNumber;
        m_activeStamp = true;
        return;
      case 'A' when line.StartsWith ("AUCREG") && (1 == m_stampLineNumber) && m_activeStamp:
        // Test variable
        if (m_stampVariablesGetter is not null) {
          if (line.StartsWith ($"AUCREG {m_stampVariablesGetter.SequenceStampVariable}=")) {
            ++m_stampLineNumber;
            return;
          }
        }
        goto default;
      case '>' when line.Equals ("> G203") && (2 == m_stampLineNumber && m_activeStamp):
        stampVariable = true;
        m_stampLineNumber = 0;
        m_activeStamp = false;
        break;
      case 'M' when line.StartsWith ("M950 H") && (1 == m_stampLineNumber) && m_activeStamp:
        stampVariable = true;
        m_stampLineNumber = 0;
        m_activeStamp = false;
        break;
      default:
        if (log.IsTraceEnabled) {
          log.Trace ($"ParseLineAsync: {line} => machining");
        }
        m_stampLineNumber = 0;
        m_activeStamp = false;
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
      case '(': {
        var comment = s.Substring (1).TrimEnd (')', ' ').Trim ();
        if (0 < comment.Length) {
          stampingEventHandler.SetComment (comment);
        }
      }
      break;
      case '/': {
        var comment = s.Substring (2).Trim ();
        stampingEventHandler.SetComment (comment);
      }
      break;
      default:
        break;
      }
    }
  }
}
