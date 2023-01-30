// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Interface to implement to manage config errors
  /// 
  /// There must be a default constructor with no argument
  /// </summary>
  public interface IConfigErrorFileFlow
  {
    /// <summary>
    /// Method that is run in case of a config error
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <returns></returns>
    Task OnConfigError (string inputFilePath, string outputFilePath);
  }
}
