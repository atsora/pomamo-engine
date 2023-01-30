// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReason.
  /// </summary>
  public interface IReasonDAO: IGenericUpdateDAO<IReason, int>
  {
    /// <summary>
    /// Find a reason with the specified code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    IReason FindByCode (string code);    
    
    /// <summary>
    /// FindAll implementation
    /// with an eager fetch of the corresponding ReasonGroup
    /// </summary>
    /// <returns></returns>
    IList<IReason> FindAllWithReasonGroup ();
    
    /// <summary>
    /// FindAll implementation
    /// with an eager fetch of the corresponding ReasonGroup
    /// and restricting the result set to a given reason group
    /// </summary>
    /// <param name="reasonGroup"></param>
    /// <returns></returns>
    IList<IReason> FindAllWithReasonGroup (IReasonGroup reasonGroup);
  }
}
