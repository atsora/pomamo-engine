// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.Responses
{
  /// <summary>
  /// DTO assembler interface from S (model) to T (dto)
  /// </summary>
  public interface IGenericDTOAssembler<T,S>
  {
    /// <summary>
    /// Maps IEnumerable of S to IEnumerable of T
    /// </summary>
    /// <param name="sourceList"></param>
    /// <returns></returns>
    IEnumerable<T> Assemble(IEnumerable<S> sourceList);
    
    /// <summary>
    /// Maps S to T
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    T Assemble(S source);
  }
}

