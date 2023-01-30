// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Pulse.Web
{
  /// <summary>
  /// DTO assembler interface from S (model) to T (dto)
  /// </summary>
  public interface IGenericDTOAssembler<T,S>
    : Lemoine.Extensions.Web.Responses.IGenericDTOAssembler<T,S>
  {
  }
}

