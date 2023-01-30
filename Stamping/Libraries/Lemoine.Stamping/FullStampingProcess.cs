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
using Lemoine.Core.Plugin;

namespace Lemoine.Stamping
{
  /// <summary>
  /// FullStampingProcess
  /// </summary>
  public sealed class FullStampingProcess
  {
    readonly ILog log = LogManager.GetLogger (typeof (FullStampingProcess).FullName);

    readonly TypeLoader m_typeLoader;
    readonly IStampingFileFlow m_stampingFileFlow;
    readonly IStamper m_stamper;
    readonly IStampingParser m_stampingParser;
    readonly StampingData m_stampingData;
    readonly IStampingApplicationBuilder m_stampingApplicationBuilder;
    readonly IStampingEventHandlersProvider m_stampingEventHandlersProvider;

    /// <summary>
    /// Constructor
    /// </summary>
    public FullStampingProcess (TypeLoader typeLoader, IStampingFileFlow stampingFileFlow, IStamper stamper, IStampingParser stampingParser, StampingData stampingData, IStampingApplicationBuilder stampingApplicationBuilder, IStampingEventHandlersProvider stampingEventHandlersProvider)
    {
      m_typeLoader = typeLoader;
      m_stampingFileFlow = stampingFileFlow;
      m_stamper = stamper;
      m_stampingParser = stampingParser;
      m_stampingData = stampingData;
      m_stampingApplicationBuilder = stampingApplicationBuilder;
      m_stampingEventHandlersProvider = stampingEventHandlersProvider;
    }

    void Configure ()
    {
      foreach (var eventHandlerType in m_stampingEventHandlersProvider.EventHandlerTypes) {
        m_stampingApplicationBuilder.UseEventHandler (eventHandlerType);
      }
    }

    /// <summary>
    /// Stamp
    /// </summary>
    public async Task StampAsync (CancellationToken cancellationToken = default)
    {
      Configure ();
      var eventHandler = m_stampingApplicationBuilder.EventHandler;

      cancellationToken.ThrowIfCancellationRequested ();

      m_stampingData.Source = m_stampingFileFlow.InputFilePath; // Adds also FileName
      m_stampingData.Destination = m_stampingFileFlow.OutputFilePath;

      m_stamper.LineFeed = m_stampingParser.LineFeed;

      cancellationToken.ThrowIfCancellationRequested ();

      try {
        await m_stamper.StartAsync (cancellationToken);
        try {
          await m_stampingParser.ParseAsync (m_stamper, eventHandler, m_stampingData, m_stamper.ParsingCancellation.Token);
        }
        catch (TaskCanceledException ex) {
          log.Debug ($"StampAsync: parsing was cancelled", ex);
        }
        catch (Exception ex) {
          log.Error ($"StampAsync: ParseAsync failed with exception", ex);
          throw;
        }
        await m_stamper.CompleteAsync ();
      }
      catch (Exception ex) {
        log.Error ($"StampAsync: exception => raise OnStampingFailure", ex);
        m_stamper.CloseAll ();
        await m_stampingFileFlow.OnFailureAsync ();
        return;
      }
      m_stamper.CloseAll ();
      await m_stampingFileFlow.OnSuccessAsync ();
    }
  }
}
