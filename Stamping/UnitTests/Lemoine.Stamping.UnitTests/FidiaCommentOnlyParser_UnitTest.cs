// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Stamping.StampingParsers;
using Lemoine.Stamping.StampLineCreators;
using Lemoine.Stamping.StampVariablesGetters;
using NUnit.Framework;

namespace Lemoine.Stamping.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class FidiaCommentOnlyParser_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (FidiaCommentOnlyParser_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public FidiaCommentOnlyParser_UnitTest ()
    { }

    [Test]
    public async Task TestSkipStamps ()
    {
      var stampVariablesGetter = new StampVariablesGetter ();
      stampVariablesGetter.SequenceStampVariable = "33";
      var parser = new FidiaCommentOnlyParser (stampVariablesGetter);
      var input =
        """
        O124
        G0
        (AUCREG 97) 
        M950 H123.34
        (AUCREG)
        G201 H0
        AUCREG 33=2308.3409
        > G203
        G1

        """;
      var expectedOutput =
        """
        O124
        G0
        (AUCREG)
        G1

        """;

      using (var stamper = new Lemoine.Stamping.Stampers.StringToStringStamper (input)) {
        var eventHandler = new Lemoine.Stamping.StampingEventHandlers.LogEvents (stamper);
        var stampingData = new StampingData ();

        try {
          await stamper.StartAsync ();
          try {
            await parser.ParseAsync (stamper, eventHandler, stampingData);
          }
          catch (TaskCanceledException ex) {
            log.Debug ($"StampAsync: parsing was cancelled", ex);
          }
          catch (Exception ex) {
            log.Error ($"StampAsync: ParseAsync failed with exception", ex);
            throw;
          }
          await stamper.CompleteAsync ();
        }
        catch (Exception ex) {
          log.Error ($"StampAsync: exception => raise OnStampingFailure", ex);
        }
        stamper.CloseAll ();
        // TODO: check why there is an extra line ending at the end
        Assert.That (stamper.Output.ReplaceLineEndings (), Is.EqualTo (expectedOutput.ReplaceLineEndings ()));
      }
    }
  }
}
