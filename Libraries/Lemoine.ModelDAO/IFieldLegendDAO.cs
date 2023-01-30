// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IFieldLegend.
  /// </summary>
  public interface IFieldLegendDAO: IGenericUpdateDAO<IFieldLegend, int>
  {
    /// <summary>
    /// Find all the FieldLegend items for the specified field
    /// </summary>
    /// <param name="field">not null</param>
    /// <returns></returns>
    IList<IFieldLegend> FindAllWithField (IField field);
    
    /// <summary>
    /// Find all the FieldLegend items for the specified field
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="fieldId">not 0</param>
    /// <returns></returns>
    IList<IFieldLegend> FindAllWithField (int fieldId);

      /// <summary>
    /// Find FieldLegend item for a specified field and value
    /// </summary>
    /// <param name="field"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    IFieldLegend FindWithFieldAndValue(IField field, double fieldValue);
  }
}
