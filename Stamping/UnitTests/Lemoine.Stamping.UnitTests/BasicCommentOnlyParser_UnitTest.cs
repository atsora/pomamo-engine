// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Stamping.Stampers;
using Lemoine.Stamping.StampingParsers;
using Lemoine.Stamping.StampLineCreators;
using Lemoine.Stamping.StampVariablesGetters;
using NUnit.Framework;

namespace Lemoine.Stamping.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class BasicCommentOnlyParser_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (BasicCommentOnlyParser_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public BasicCommentOnlyParser_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestIsStamp ()
    {
      var stampVariablesGetter = new StampVariablesGetter ();
      stampVariablesGetter.SequenceStampVariable = "590";
      var lineFormatter = new StringFormatLineFormatter {
        Format = "VC{0}={1}"
      };
      var basicCommentOnlyParser = new BasicCommentOnlyParser (stampVariablesGetter, lineFormatter);
      Assert.Multiple (() => {
        Assert.That (basicCommentOnlyParser.IsStamp ("(VC=23)"), Is.False);
        Assert.That (basicCommentOnlyParser.IsStamp ("VC300=34.3"), Is.False);
        Assert.That (basicCommentOnlyParser.IsStamp ("VC590=234.234"), Is.True);
      });
    }

    [Test]
    public async Task TestSkipStamps ()
    {
      var stampVariablesGetter = new StampVariablesGetter ();
      stampVariablesGetter.SequenceStampVariable = "590";
      var lineFormatter = new StringFormatLineFormatter {
        Format = "VC{0}={1}"
      };
      var basicCommentOnlyParser = new BasicCommentOnlyParser (stampVariablesGetter, lineFormatter);
      var input =
        """
        O124
        G0
        VC23=235.093
        VC590=234.238
        G1

        """;
      var expectedOutput =
        """
        O124
        G0
        VC23=235.093
        G1

        """;

      using (var stamper = new Lemoine.Stamping.Stampers.StringToStringStamper (input)) {
        var eventHandler = new Lemoine.Stamping.StampingEventHandlers.LogEvents (stamper);
        var stampingData = new StampingData ();

        try {
          await stamper.StartAsync ();
          try {
            await basicCommentOnlyParser.ParseAsync (stamper, eventHandler, stampingData);
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
        Assert.That (stamper.Output.ReplaceLineEndings (), Is.EqualTo (expectedOutput.ReplaceLineEndings ()));
      }
    }
  }
}
