// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Stamping;
using Lemoine.Stamping.Stampers;

namespace Lem_TestHeidenhainParser.Console
{
  /// <summary>
  /// Stamper
  /// </summary>
  public sealed class TestStamperFromString
    : IStamper
    , IDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (TestStamperFromString).FullName);

    readonly TextReader m_reader;
    readonly TextWriter m_writer;
    readonly QueueStamper m_readerWriter;
    bool m_closed = false;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public TestStamperFromString ()
    {
      var i = 0;
      var s = $"""
{i++} BEGIN PGM TEST MM
{i++} BLK FORM 0.1 Z X+0 Y+0 Z-40
{i++} BLK FORM 0.2 X+100 Y+100 Z+0
;INFOCUS START
FN 16: F-PRINT TNC:\INFOCUS\RESULT.A / TNC:\INFOCUS\1243202.TXT
FN 16: F-PRINT TNC:\INFOCUS\RESULT.A / TNC:\INFOCUS\37.N
;INFOCUS END
{i++} PLANE RESET STAY
{i++} FUNCTION RESET TCPM
{i++} CYCL DEF 247 DATUM SETTING~
   Q339=+1 ; DATUM NUMBER
{i++} ; TEST
{i++} ;
{i++} PATTERN DEF POS1 (X+10 Y+10 Z+0) POS2 (X+10 Y+90 Z+0)
{i++} LBL "RESET" ; RESET
{i++} CYCL DEF 7.0 DATUM SHIFT
{i++} CYCL DEF 7.1 X0.000
{i++} CYCL DEF 7.2 Y0.000
{i++} CYCL DEF 7.3 Z0.000
{i++} LBL 0
{i++} ;
{i++} * TOOLPATH  : F10
{i++} ; PARAMETERIC FEEDRATE DEFINITION
{i++}     Q1=3000; PLUNGE FEED RATE
{i++}     Q2=3000; CUTTING FEED RATE
{i++} M03
{i++} TOOL CALL 1 Z S3500
{i++} FN 0: Q5 = +60
{i++} FN 1: Q5 = -Q5 + -3
{i++} L Z+250 R0 FMAX
{i++} L B0.0 C0.0 FMAX 
{i++} TOOL CALL 92 Z S2800 DL+0.0 DR+0.0
{i++} L Z-5 R0 F1000 M13
{i++} L X+5 R- F500
{i++} CALL PGM SUBPGM
{i++} L IY+5 R-
{i++} CYCL DEF 200 DRILLING
    Q200=+2 ;SET-UP CLEARANCE
    Q201=-20 ;DEPTH
    Q206=+150 ;FEED RATE FOR PLNGNG
    Q202=+5 ;PLUNGING DEPTH
    Q210=+0 ;DWELL TIME AT TOP
    Q203=+0 ;SURFACE COORDINATE
    Q204=+50 ;2ND SET-UP CLEARANCE
    Q211=+0 ;DWELL TIME AT DEPTH
    Q395=+0 ;DEPTH REFERENCE
{i++} CALL LBL 1
{i++} CYCL CALL
{i++} L Z+250 R0 FMAX M30
{i++} LBL 1
{i++} L IY-5
{i++} LBL0
{i++} END PGM TEST MM

""";
      m_reader = new StringReader (s);
      m_writer = new StringWriter ();
      m_readerWriter = new QueueStamper (m_reader, m_writer);
    }

    public CancellationTokenSource ParsingCancellation => m_readerWriter.ParsingCancellation;

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync (CancellationToken cancellationToken = default)
    {
      await m_readerWriter.StartAsync (cancellationToken);
    }

    public StamperLineFeed LineFeed
    {
      get => m_readerWriter.LineFeed;
      set {
        m_readerWriter.LineFeed = value;
      }
    }

    public TextReader Reader => m_readerWriter;

    public void AddLine (string line)
    {
      m_readerWriter.AddLine (line);
    }

    public async Task AddLineAsync (string line)
    {
      await m_readerWriter.AddLineAsync (line);
    }

    public void Skip (int endPosition)
    {
      m_readerWriter.Skip (endPosition);
      m_writer.Flush ();
    }

    public async Task SkipAsync (int endPosition, CancellationToken cancellationToken = default)
    {
      await m_readerWriter.SkipAsync (endPosition, cancellationToken);
      await m_writer.FlushAsync ();
    }

    public async Task SkipAsync (CancellationToken cancellationToken = default)
    {
      await m_readerWriter.SkipAsync (cancellationToken);
      await m_writer.FlushAsync ();
    }

    public void Release (int endPosition)
    {
      m_readerWriter.Release (endPosition);
      m_writer.Flush ();
    }

    public async Task ReleaseAsync (int endPosition, CancellationToken cancellationToken = default)
    {
      await m_readerWriter.ReleaseAsync (endPosition, cancellationToken);
      await m_writer.FlushAsync ();
    }

    public void Release ()
    {
      m_readerWriter.Release ();
      m_writer.Flush ();
    }

    public async Task ReleaseAsync (CancellationToken cancellationToken = default)
    {
      await m_readerWriter.ReleaseAsync (cancellationToken);
      await m_writer.FlushAsync ();
    }

    public void Complete ()
    {
      m_readerWriter.Complete ();
    }

    public async Task CompleteAsync (CancellationToken cancellationToken = default)
    {
      await m_readerWriter.CompleteAsync (cancellationToken);
    }

    public bool Completed => m_readerWriter.Completed;

    public void CloseAll ()
    {
      if (!m_closed) {
        m_writer.Flush ();

        var s = m_writer.ToString ();
        System.Console.Write (s);

        m_readerWriter.Dispose ();
        m_writer.Dispose ();
        m_reader.Dispose ();

        m_closed = true;
      }
    }

    public void Dispose ()
    {
      CloseAll ();
    }
  }
}
