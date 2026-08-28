// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for <see cref="SequenceKind">
  /// 
  /// The values are explicitly added so that they keep the same case as in the C# enum
  /// </summary>
  public class SequenceKindGraphType : EnumerationGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceKindGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceKindGraphType ()
    {
      Name = "SequenceKind";
      Add ("Machining", SequenceKind.Machining, "Machining");
      Add ("Stop", SequenceKind.Stop, "Stop (M0, ...)");
      Add ("OptionalStop", SequenceKind.OptionalStop, "Optional stop (M1, /M0)");
      Add ("NonMachining", SequenceKind.NonMachining, "Non-machining sequence");
      Add ("AutoPalletChange", SequenceKind.AutoPalletChange, "Pallet change: automatic if the pallet is ready, else the machine stops. Usually triggered by M60");
    }
  }
}
