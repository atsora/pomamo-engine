// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Business
{
  /// <summary>
  /// Small class for using a custom service provider (typically in unit tests)
  /// Usage:
  /// using (new CustomServiceProvider (new YourCustomService()))
  /// {
  ///   (code here)
  /// }
  /// </summary>
  public class CustomServiceProvider: IDisposable
  {
    /// <summary>
    /// Constructor, setting the new service provider and asking for an exclusivity to this thread
    /// (other tests could be run at the same time, this custom service provider must not interfer)
    /// </summary>
    /// <param name="customService"></param>
    public CustomServiceProvider (IService customService)
    {
      Lemoine.Business.ServiceProvider.SetCustomService (customService);
    }
   
    /// <summary>
    /// Destructor, restoring the default service provider and unlocking the other threads
    /// </summary>
    public void Dispose ()
    {
      Lemoine.Business.ServiceProvider.RemoveCustomService ();
    }
  }
}
