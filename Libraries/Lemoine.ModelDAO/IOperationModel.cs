// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Model
{
  /// <summary>
  /// Operation model
  /// </summary>
  public interface IOperationModel : IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IOperationModel>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated operation
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Associated revision
    /// </summary>
    IOperationRevision Revision { get; }

    /// <summary>
    /// Description of the revision
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Archive date/time if the operation model is archived
    /// </summary>
    DateTime? ArchiveDateTime { get; set; }

    /// <summary>
    /// Code that will be used in a variable for example
    /// </summary>
    string Code { get; set; }

    /// <summary>
    /// Default model
    /// </summary>
    bool Default { get; set; }

    /// <summary>
    /// CAD Model Name
    /// 
    /// nullable
    /// </summary>
    string CadModelName { get; set; }

    /// <summary>
    /// Associated sequence operation models
    /// </summary>
    IEnumerable<ISequenceOperationModel> SequenceOperationModels { get; }

    // TODO: sequences

    // TODO: Iso file ?

    // TODO: Machine filter ?
  }

  /// <summary>
  /// Extension to class <see cref="IOperationModel"/>
  /// </summary>
  public static class OperationModelExtensions
  {
    /// <summary>
    /// Get the available path numbers
    /// </summary>
    /// <param name="operationModel"></param>
    /// <returns></returns>
    public static IEnumerable<int?> GetPathNumbers (this IOperationModel operationModel)
    {
      return operationModel.SequenceOperationModels
        .Select (x => x.PathNumber)
        .Distinct ();
    }

    /// <summary>
    /// Get the list of sequences that are associated to an operation model
    /// </summary>
    /// <param name="operationModel"></param>
    /// <param name="pathNumber">Path number</param>
    /// <returns></returns>
    public static IEnumerable<ISequence> GetSequences (this IOperationModel operationModel, int? pathNumber = null)
    {
      return operationModel.SequenceOperationModels
        .Where (x => object.Equals (pathNumber, x.PathNumber))
        .OrderBy (x => x.Order)
        .Select (x => x.Sequence);
    }

    /// <summary>
    /// Get the <see cref="IOperationDuration"/> that are associated to this operation model
    /// </summary>
    /// <param name="operationModel"></param>
    /// <returns></returns>
    public static IEnumerable<IOperationDuration> GetDurations (this IOperationModel operationModel)
    {
      return operationModel.Operation
        .Durations
        .Where (x => (x.OperationModel is null) || (x.OperationModel.Id == operationModel.Id));
    }
  }
}
