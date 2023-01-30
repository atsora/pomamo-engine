// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Lemoine.Cnc.Asp
{
  /// <summary>
  /// Type for a single response request in the Cnc Core service
  /// </summary>
  [Serializable]
  public class SingleResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="acquisitionIdentifier">nullable</param>
    /// <param name="moduleref">not null</param>
    /// <param name="instruction">not null</param>
    /// <param name="method">nullable</param>
    /// <param name="property">Optional</param>
    /// <param name="param">Optional</param>
    /// <param name="success">Optional: for json deserialization</param>
    /// <param name="result">Optional: for json deserialization</param>
    /// <param name="error">Optional: for json deserialization</param>
    public SingleResponse (string acquisitionIdentifier, string moduleref, string instruction, string method, string property = null, string param = null, bool success = false, object result = null, string error = null)
    {
      Debug.Assert (null != moduleref);
      Debug.Assert (null != instruction);

      this.AcquisitionIdentifier = acquisitionIdentifier;
      this.Moduleref = moduleref;
      this.Instruction = instruction;
      this.Method = method;
      this.Property = property;
      this.Param = param;
      this.Success = success;
      this.Result = result;
      this.Error = error;
    }

    /// <summary>
    /// Acquisition identifier
    /// 
    /// Nullable
    /// </summary>
    public string AcquisitionIdentifier;

    /// <summary>
    /// Module reference
    /// 
    /// Not nullable
    /// </summary>
    public string Moduleref;

    /// <summary>
    /// Instruction
    /// 
    /// Not nullable
    /// </summary>
    public string Instruction;

    /// <summary>
    /// Method
    /// 
    /// Nullable
    /// </summary>
    public string Method;

    /// <summary>
    /// Property
    /// 
    /// Nullable
    /// </summary>
    public string Property;

    /// <summary>
    /// Parameter
    /// 
    /// Nullable
    /// </summary>
    public string Param;

    /// <summary>
    /// Success
    /// </summary>
    public bool Success;

    /// <summary>
    /// Result
    /// 
    /// Nullable
    /// </summary>
    public object Result;

    /// <summary>
    /// Error string
    /// 
    /// Nullable
    /// </summary>
    public string Error;

    /// <summary>
    /// Set the success flag in case of a set request
    /// </summary>
    public void SetSuccess ()
    {
      this.Result = null;
      this.Success = true;
    }

    /// <summary>
    /// Set a result
    /// </summary>
    /// <param name="result"></param>
    public void SetResult (object result)
    {
      this.Result = result;
      this.Success = true;
    }

    /// <summary>
    /// Set a missing acquisition identifier error
    /// </summary>
    /// <param name="acquisitionIdentifier">Optional</param>
    public void SetMissingAcquisition (string acquisitionIdentifier = null)
    {
      this.Success = false;
      var a = (acquisitionIdentifier is null) ? this.AcquisitionIdentifier : acquisitionIdentifier;
      this.Error = $"Acquisition {a} not found";
    }

    /// <summary>
    /// Set a missing module reference error
    /// </summary>
    /// <param name="moduleref">Optional</param>
    public void SetMissingModule (string moduleref = null)
    {
      this.Success = false;
      var r = (moduleref is null) ? this.Moduleref : moduleref;
      this.Error = $"Module with reference {r} not found";
    }

    /// <summary>
    /// Set a missing method error
    /// </summary>
    /// <param name="method">Optional</param>
    public void SetMissingMethod (string method = null)
    {
      this.Success = false;
      var m = (method is null) ? this.Method : method;
      this.Error = $"Method {m} not found";
    }

    /// <summary>
    /// Set a missing property error
    /// </summary>
    /// <param name="property">Optional</param>
    public void SetMissingProperty (string property = null)
    {
      this.Success = false;
      var p = (property is null) ? this.Property : property;
      this.Error = $"Method {p} not found";
    }

    /// <summary>
    /// Set a start error
    /// </summary>
    public void SetStartError ()
    {
      this.Success = false;
      this.Error = "Start error";
    }

    /// <summary>
    /// Set a specific error
    /// </summary>
    /// <param name="error">not null</param>
    public void SetError (string error)
    {
      Debug.Assert (null != error);

      this.Success = false;
      this.Error = error;
    }
  }
}
