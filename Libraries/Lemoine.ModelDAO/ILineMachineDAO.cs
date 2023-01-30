// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ILineMachine.
  /// </summary>
  public interface ILineMachineDAO: IGenericDAO<ILineMachine, int>
  {
    /// <summary>
    /// Find all LineMachine for a specific machine
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IList<ILineMachine> FindAllByMachine(IMachine machine);
    
    /// <summary>
    /// Find all LineMachine for a specific line
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="line">not null</param>
    /// <returns></returns>
    IList<ILineMachine> FindAllByLine(ILine line);
    
    /// <summary>
    /// Find all LineMachine for a specific line and a specific machine
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IList<ILineMachine> FindAllByLineMachine (ILine line, IMachine machine);

    /// <summary>
    /// Find all LineMachine for a specific line and a specific machine
    /// with an eager fetch of the operation
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IList<ILineMachine> FindAllByLineMachineWithOperation (ILine line, IMachine machine);
    
    /// <summary>
    /// Find all LineMachine for a specific machine and operation
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    IList<ILineMachine> FindAllByMachineOperation (IMachine machine, IOperation operation);

    /// <summary>
    /// Find all LineMachine for a specific operation
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    IList<ILineMachine> FindAllByOperation (IOperation operation);
    
    /// <summary>
    /// Find all LineMachine for a specific line and operation
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    IList<ILineMachine> FindAllByLineOperation (ILine line, IOperation operation);
  }
}
