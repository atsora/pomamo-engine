// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Web implementation of IOperation
  ///
  /// Only Id, Display and DocumentLink are set, from the /Data/Operation/FindById web service (OperationDTO)
  /// </summary>
  public class Operation : IOperation
  {
    public int Id { get; set; }

    public string Display { get; set; }

    public string GetDisplay (string variant) => throw new NotImplementedException ();

    public int Version => throw new NotImplementedException ();

    public string[] Identifiers => throw new NotImplementedException ();

    public ISimpleOperation SimpleOperation => throw new NotImplementedException ();

    public string LongDisplay => throw new NotImplementedException ();

    public string ShortDisplay => throw new NotImplementedException ();

    public string Name { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public string Code { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public string ExternalCode { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public string DocumentLink { get; set; }

    public IOperationType Type { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public IOperationRevision ActiveRevision => throw new NotImplementedException ();

    public IList<IOperationRevision> Revisions => throw new NotImplementedException ();

    public IOperationModel DefaultActiveModel => throw new NotImplementedException ();

    /// <summary>
    /// Not deserialized: in OperationDTO, it is an int and not a TimeSpan
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public TimeSpan? MachiningDuration { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public TimeSpan? SetUpDuration { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public TimeSpan? TearDownDuration { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public TimeSpan? LoadingDuration { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public TimeSpan? UnloadingDuration { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public DateTime CreationDateTime => throw new NotImplementedException ();

    public bool Lock { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public IMachineFilter MachineFilter { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public ICollection<IIntermediateWorkPiece> IntermediateWorkPieces => throw new NotImplementedException ();

    public ICollection<IIntermediateWorkPiece> Sources => throw new NotImplementedException ();

    public ICollection<IPath> Paths => throw new NotImplementedException ();

    public ICollection<ISequence> Sequences => throw new NotImplementedException ();

    public ICollection<IStamp> Stamps => throw new NotImplementedException ();

    public DateTime? ArchiveDateTime { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public ICollection<IOperationDuration> Durations => throw new NotImplementedException ();

    public void AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece) => throw new NotImplementedException ();

    public void AddSource (IIntermediateWorkPiece intermediateWorkPiece) => throw new NotImplementedException ();

    public void RemoveSource (IIntermediateWorkPiece intermediateWorkPiece) => throw new NotImplementedException ();

    public void RemovePath (IPath path) => throw new NotImplementedException ();

    public int GetTotalNumberOfIntermediateWorkPieces () => throw new NotImplementedException ();

    public bool Equals (IOperation other) => other != null && (Id == ((Lemoine.Collections.IDataWithId<int>)other).Id);

    public override bool Equals (object obj) => obj is IOperation other && Equals (other);

    public override int GetHashCode () => Id.GetHashCode ();

    public void Unproxy () => throw new NotImplementedException ();
  }
}
