// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IField.
  /// </summary>
  public interface IFieldDAO: IGenericUpdateDAO<IField, int>
  {
    /// <summary>
    /// Find by code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    IField FindByCode (string code);
    
    /// <summary>
    /// Find all active fields
    /// </summary>
    /// <returns></returns>
    IList<IField> FindAllActive();

    /// <summary>
    /// Find all active fields with an eager fetch of the unit
    /// </summary>
    /// <returns></returns>
    IList<IField> FindAllActiveWithUnit ();
  }
}
