// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Extensions.Analysis;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using Pulse.Extensions.Extension;
using System.Text.RegularExpressions;
using Pulse.PluginImplementation.Analysis;
using System.Linq;
using Lemoine.Extensions.Analysis.Detection;

namespace Lemoine.Plugins.UnitTests
{
  /// <summary>
  /// Test of <see cref="ProgramCommentParser"/>
  /// </summary>
  public class ProgramCommentParser_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProgramCommentParser_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ProgramCommentParser_UnitTest ()
    { }

    [Test]
    public void TestGetOperation ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart), true);
            var regex = new Regex ("Part=(?<partName>[0-9A-Za-z]+) Op=(?<opName>[0-9A-Za-z]+) Qty=(?<qty>[0-9]+)");
            {
              var programCommentParser = new ProgramCommentParser (regex, "Part=P123 Op=Op123 Qty=2", new List<IRegexMatchToFileOpDataExtension> ());
              var operation = programCommentParser.GetOperation ();
              ModelDAOHelper.DAOFactory.Flush ();
              Assert.That (operation, Is.Not.Null);
              Assert.That (operation.Name, Is.EqualTo ("Op123"));
              Assert.That (operation.IntermediateWorkPieces.Single ().OperationQuantity, Is.EqualTo (2));
              var part = operation
                .IntermediateWorkPieces
                .SelectMany (x => x.ComponentIntermediateWorkPieces)
                .Select (x => x.Component.Part)
                .Single ();
              Assert.That (part, Is.Not.Null);
              Assert.That (part.Name, Is.EqualTo ("P123"));
            }
            {
              var programCommentParser = new ProgramCommentParser (regex, "Part=P123 Op=Op123 Qty=2", new List<IRegexMatchToFileOpDataExtension> ());
              var operation = programCommentParser.GetOperation ();
              ModelDAOHelper.DAOFactory.Flush ();
              Assert.That (operation, Is.Not.Null);
              Assert.That (operation.Name, Is.EqualTo ("Op123"));
              Assert.That (operation.IntermediateWorkPieces.Single ().OperationQuantity, Is.EqualTo (2));
              var part = operation
                .IntermediateWorkPieces
                .SelectMany (x => x.ComponentIntermediateWorkPieces)
                .Select (x => x.Component.Part)
                .Single ();
              Assert.That (part, Is.Not.Null);
              Assert.That (part.Name, Is.EqualTo ("P123"));
            }
            var regex2 = new Regex ("Part=(?<partCode>[0-9A-Za-z]+) Op=(?<opCode>[0-9A-Za-z]+) Qty=(?<qty1>[0-9]+)/(?<qty2>\\d+)");
            {
              var programCommentParser = new ProgramCommentParser (regex2, "Part=P234 Op=Op234 Qty=2/3", new List<IRegexMatchToFileOpDataExtension> ());
              var operation = programCommentParser.GetOperation ();
              ModelDAOHelper.DAOFactory.Flush ();
              Assert.That (operation, Is.Not.Null);
              Assert.That (operation.Code, Is.EqualTo ("Op234"));
              Assert.That (operation.IntermediateWorkPieces.Single ().OperationQuantity, Is.EqualTo (5));
              var part = operation
                .IntermediateWorkPieces
                .SelectMany (x => x.ComponentIntermediateWorkPieces)
                .Select (x => x.Component.Part)
                .Single ();
              Assert.That (part, Is.Not.Null);
              Assert.That (part.Code, Is.EqualTo ("P234"));
            }
            {
              var programCommentParser = new ProgramCommentParser (regex2, "Part=P234 Op=Op234 Qty=2/3", new List<IRegexMatchToFileOpDataExtension> ());
              var operation = programCommentParser.GetOperation ();
              ModelDAOHelper.DAOFactory.Flush ();
              Assert.That (operation, Is.Not.Null);
              Assert.That (operation.Code, Is.EqualTo ("Op234"));
              Assert.That (operation.IntermediateWorkPieces.Single ().OperationQuantity, Is.EqualTo (5));
              var part = operation
                .IntermediateWorkPieces
                .SelectMany (x => x.ComponentIntermediateWorkPieces)
                .Select (x => x.Component.Part)
                .Single ();
              Assert.That (part, Is.Not.Null);
              Assert.That (part.Code, Is.EqualTo ("P234"));
            }
          }
          finally {
            Lemoine.Info.ConfigSet.ResetForceValues ();
            transaction.Rollback ();
          }
        }
      }
    }

    [Test]
    public void TestGetOp1Op2 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart), true);
            var regex = new Regex ("Part=(?<partName>\\w+) Op=(?<op1Name>\\w+)/(?<op2Name>\\w+) Qty=(?<qty1>\\d+)/(?<qty2>\\d+)");
            {
              var programCommentParser = new ProgramCommentParser (regex, "Part=P123 Op=Op1/Op2 Qty=2/2", new List<IRegexMatchToFileOpDataExtension> ());
              var operation = programCommentParser.GetOperation ();
              ModelDAOHelper.DAOFactory.Flush ();
              Assert.That (operation, Is.Not.Null);
              Assert.That (operation.Name, Is.EqualTo ("Op1/Op2"));
              Assert.That (operation.IntermediateWorkPieces, Has.Count.EqualTo (2));
              Assert.That (operation.IntermediateWorkPieces.First ().Name, Is.EqualTo ("Op1"));
              Assert.That (operation.IntermediateWorkPieces.First ().OperationQuantity, Is.EqualTo (2));
              Assert.That (operation.IntermediateWorkPieces.Last ().Name, Is.EqualTo ("Op2"));
              Assert.That (operation.IntermediateWorkPieces.Last ().OperationQuantity, Is.EqualTo (2));
              var part = operation
                .IntermediateWorkPieces
                .SelectMany (x => x.ComponentIntermediateWorkPieces)
                .Select (x => x.Component.Part)
                .Distinct ()
                .Single ();
              Assert.That (part, Is.Not.Null);
              Assert.That (part.Name, Is.EqualTo ("P123"));
            }
            {
              var programCommentParser = new ProgramCommentParser (regex, "Part=P123 Op=Op1/Op2 Qty=2/2", new List<IRegexMatchToFileOpDataExtension> ());
              var operation = programCommentParser.GetOperation ();
              ModelDAOHelper.DAOFactory.Flush ();
              Assert.That (operation, Is.Not.Null);
              Assert.That (operation.Name, Is.EqualTo ("Op1/Op2"));
              Assert.That (operation.IntermediateWorkPieces, Has.Count.EqualTo (2));
              Assert.That (operation.IntermediateWorkPieces.First ().Name, Is.EqualTo ("Op1"));
              Assert.That (operation.IntermediateWorkPieces.First ().OperationQuantity, Is.EqualTo (2));
              Assert.That (operation.IntermediateWorkPieces.Last ().Name, Is.EqualTo ("Op2"));
              Assert.That (operation.IntermediateWorkPieces.Last ().OperationQuantity, Is.EqualTo (2));
              var part = operation
                .IntermediateWorkPieces
                .SelectMany (x => x.ComponentIntermediateWorkPieces)
                .Select (x => x.Component.Part)
                .Distinct ()
                .Single ();
              Assert.That (part, Is.Not.Null);
              Assert.That (part.Name, Is.EqualTo ("P123"));
            }
          }
          finally {
            Lemoine.Info.ConfigSet.ResetForceValues ();
            transaction.Rollback ();
          }
        }
      }
    }

    [Test]
    public void TestGetOp1Op2withSequences ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            Lemoine.Info.ConfigSet.ForceValue (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart), true);
            var regex = new Regex ("Part=(?<partName>\\w+) Op=(?<op1Name>\\w+)/(?<op2Name>\\w+) Qty=(?<qty1>\\d+)/(?<qty2>\\d+)");
            {
              var programCommentParser = new ProgramCommentParser (regex, "Part=P123 Op=Op1/Op2 Qty=2/2", new List<IRegexMatchToFileOpDataExtension> ()) {
                CreateSequences = true
              };
              var operation = programCommentParser.GetOperation ();
              ModelDAOHelper.DAOFactory.Flush ();
              Assert.That (operation, Is.Not.Null);
              Assert.That (operation.Name, Is.EqualTo ("Op1/Op2"));
              Assert.That (operation.IntermediateWorkPieces, Has.Count.EqualTo (2));
              Assert.That (operation.IntermediateWorkPieces.First ().Name, Is.EqualTo ("Op1"));
              Assert.That (operation.IntermediateWorkPieces.First ().OperationQuantity, Is.EqualTo (2));
              Assert.That (operation.IntermediateWorkPieces.Last ().Name, Is.EqualTo ("Op2"));
              Assert.That (operation.IntermediateWorkPieces.Last ().OperationQuantity, Is.EqualTo (2));
              var part = operation
                .IntermediateWorkPieces
                .SelectMany (x => x.ComponentIntermediateWorkPieces)
                .Select (x => x.Component.Part)
                .Distinct ()
                .Single ();
              Assert.That (part, Is.Not.Null);
              Assert.That (part.Name, Is.EqualTo ("P123"));
              Assert.That (operation.Sequences, Has.Count.EqualTo (2));
              Assert.That (operation.Sequences.First ().Name, Is.EqualTo ("Op1"));
              Assert.That (operation.Sequences.Last ().Name, Is.EqualTo ("Op2"));
            }
            {
              var programCommentParser = new ProgramCommentParser (regex, "Part=P123 Op=Op1/Op2 Qty=2/2", new List<IRegexMatchToFileOpDataExtension> ()) {
                CreateSequences = true
              };
              var operation = programCommentParser.GetOperation ();
              ModelDAOHelper.DAOFactory.Flush ();
              Assert.That (operation, Is.Not.Null);
              Assert.That (operation.Name, Is.EqualTo ("Op1/Op2"));
              Assert.That (operation.IntermediateWorkPieces, Has.Count.EqualTo (2));
              Assert.That (operation.IntermediateWorkPieces.First ().Name, Is.EqualTo ("Op1"));
              Assert.That (operation.IntermediateWorkPieces.First ().OperationQuantity, Is.EqualTo (2));
              Assert.That (operation.IntermediateWorkPieces.Last ().Name, Is.EqualTo ("Op2"));
              Assert.That (operation.IntermediateWorkPieces.Last ().OperationQuantity, Is.EqualTo (2));
              var part = operation
                .IntermediateWorkPieces
                .SelectMany (x => x.ComponentIntermediateWorkPieces)
                .Select (x => x.Component.Part)
                .Distinct ()
                .Single ();
              Assert.That (part, Is.Not.Null);
              Assert.That (part.Name, Is.EqualTo ("P123"));
              Assert.That (operation.Sequences, Has.Count.EqualTo (2));
              Assert.That (operation.Sequences.First ().Name, Is.EqualTo ("Op1"));
              Assert.That (operation.Sequences.Last ().Name, Is.EqualTo ("Op2"));
            }
          }
          finally {
            Lemoine.Info.ConfigSet.ResetForceValues ();
            transaction.Rollback ();
          }
        }
      }
    }
  }
}
