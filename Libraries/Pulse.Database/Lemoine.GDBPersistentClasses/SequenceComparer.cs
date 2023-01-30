// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of SequenceComparer.
  /// </summary>
  public class SequenceComparer : ISequenceComparer
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (SequenceComparer).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    #endregion // Constructors

    #region Methods
    /// Lower order sequences returned first,
    /// based on Ids if order is equal
    /// Comparison of sequences is only relevant for two sequences
    /// of the same operation
    public int Compare(ISequence seq1, ISequence seq2)
    {
      int compareOrder = seq1.Order.CompareTo(seq2.Order);
      return (compareOrder != 0 ? compareOrder : ((Lemoine.Collections.IDataWithId)seq1).Id.CompareTo(((Lemoine.Collections.IDataWithId)seq2).Id));
    }
    #endregion // Methods
  }
}
