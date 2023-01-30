// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Conversion;
using Lemoine.Core.Log;

namespace Lemoine.Core.Plugin
{
  /// <summary>
  /// Reflection utility methods
  /// </summary>
  public static class Reflection
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Reflection).FullName);

    /// <summary>
    /// Invoke the best method that matches the method name using the default auto-converter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object InvokeMethodAuto<T> (T instance, string methodName, params object[] parameters)
      where T : class
    {
      var autoConverters = new IAutoConverter[] { new DefaultAutoConverter () };
      return InvokeMethodAuto<T> (instance, autoConverters, null, methodName, parameters);
    }

    /// <summary>
    /// Invoke the best method that matches the method name using the provided auto-converters
    /// 
    /// If no auto-converter is provided, no conversion is tried
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="autoConverters">from the most restristive to the least restrictive</param>
    /// <param name="bindingFlags">Optional</param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object InvokeMethodAuto<T> (T instance, IEnumerable<IAutoConverter> autoConverters, BindingFlags? bindingFlags, string methodName, params object[] parameters)
      where T : class
    {
      if (string.IsNullOrEmpty (methodName)) {
        log.Error ("InvokeMethodAuto: methodName is not provided");
        throw new ArgumentNullException ("methodName is not provided", "methodName");
      }

      var type = instance.GetType ();
      return InvokeMethodAutoOnType (type, instance, autoConverters, bindingFlags, methodName, parameters);
    }

    /// <summary>
    /// Invoke the a static method matching the specified parameters with no auto-converter
    /// </summary>
    /// <param name="assemblyLoader"></param>
    /// <param name="qualifiedClassName"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object InvokeStaticMethod (IAssemblyLoader assemblyLoader, string qualifiedClassName, string methodName, params object[] parameters)
    {
      return InvokeStaticMethod (assemblyLoader, qualifiedClassName, null, methodName, parameters);
    }

    /// <summary>
    /// Invoke the a static method matching the specified parameters with no auto-converter
    /// </summary>
    /// <param name="assemblyLoader"></param>
    /// <param name="qualifiedClassName"></param>
    /// <param name="bindingFlags"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object InvokeStaticMethod (IAssemblyLoader assemblyLoader, string qualifiedClassName, BindingFlags? bindingFlags, string methodName, params object[] parameters)
    {
      if (string.IsNullOrEmpty (methodName)) {
        log.Error ("RunStaticMethod: methodName is not provided");
        throw new ArgumentNullException ("methodName is not provided", "methodName");
      }

      var type = GetTypeFromQualifiedName (assemblyLoader, qualifiedClassName);

      IEnumerable<MethodInfo> methods;
      if (bindingFlags.HasValue) {
        methods = type.GetMethods (bindingFlags.Value | BindingFlags.Static)
          .Where (m => m.Name.Equals (methodName));
      }
      else {
        methods = type.GetMethods (BindingFlags.Public | BindingFlags.Static)
          .Where (m => m.Name.Equals (methodName));
      }
      if (!methods.Any ()) {
        log.Error ($"RunStaticMethod: no method with name {methodName} in type {type}");
        throw new InvalidOperationException ("No method with the specified name in the specified type");
      }

      Exception lastException = null;
      foreach (var method in methods) {
        try {
          return method.Invoke (null, parameters);
        }
        catch (Exception ex) {
          log.Warn ($"RunStaticMethod: using {method} with no conversion failed", ex);
          lastException = ex;
        }
      }
      Debug.Assert (null != lastException);
      log.Error ($"RunStaticMethod: no method with name {methodName} could be applied with the provided parameters");
      throw new ArgumentException ("No method could be run with the provided parameters", "parameters", lastException);
    }

    /// <summary>
    /// Invoke the best method that matches the method name using the default auto-converter
    /// </summary>
    /// <returns></returns>
    public static object InvokeStaticMethodAuto (IAssemblyLoader assemblyLoader, string qualifiedClassName, string methodName, params object[] parameters)
    {
      var autoConverters = new IAutoConverter[] { new DefaultAutoConverter () };
      return InvokeStaticMethodAuto (assemblyLoader, qualifiedClassName, autoConverters, null, methodName, parameters);
    }

    /// <summary>
    /// Invoke a best static method that matches the parameters using the specified auto-converters
    /// </summary>
    /// <param name="assemblyLoader">Recommended not null</param>
    /// <param name="qualifiedClassName"></param>
    /// <param name="autoConverters"></param>
    /// <param name="bindingFlags"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object InvokeStaticMethodAuto (IAssemblyLoader assemblyLoader, string qualifiedClassName, IEnumerable<IAutoConverter> autoConverters, BindingFlags? bindingFlags, string methodName, params object[] parameters)
    {
      if (string.IsNullOrEmpty (methodName)) {
        log.Error ("InvokeStaticMethodAuto: methodName is not provided");
        throw new ArgumentNullException ("methodName is not provided", "methodName");
      }

      var type = GetTypeFromQualifiedName (assemblyLoader, qualifiedClassName);
      return InvokeMethodAutoOnType (type, null, autoConverters, (bindingFlags ?? BindingFlags.Public) | BindingFlags.Static, methodName, parameters);
    }

    static Type GetTypeFromQualifiedName (IAssemblyLoader assemblyLoader, string qualifiedClassName)
    {
      if (assemblyLoader is null) {
        log.Warn ($"InvokeStaticMethodAuto: assemblyLoader is null. This is better to set one to get {qualifiedClassName}");
        return Type.GetType (qualifiedClassName, true);
      }
      else {
        var typeLoader = new TypeLoader (assemblyLoader);
        return typeLoader.GetType (qualifiedClassName);
      }
    }

    static object InvokeMethodAutoOnType (Type type, object instance, IEnumerable<IAutoConverter> autoConverters, BindingFlags? bindingFlags, string methodName, params object[] parameters)
    {
      IEnumerable<MethodInfo> methods;
      if (bindingFlags.HasValue) {
        methods = type.GetMethods (bindingFlags.Value)
          .Where (m => m.Name.Equals (methodName));
      }
      else {
        methods = type.GetMethods ()
          .Where (m => m.Name.Equals (methodName));
      }
      if (!methods.Any ()) {
        log.Error ($"InvokeMethodAuto: no method with name {methodName} in {instance}");
        throw new InvalidOperationException ("No method with the specified name in the input instance");
      }

      foreach (var autoConverter in autoConverters) {
        var matchingMethods = methods
          .Where (m => m.IsParameterMatch (autoConverter, parameters));
        var count = matchingMethods.Count ();
        if (0 == count) {
          if (log.IsDebugEnabled) {
            log.Debug ($"InvokeMethodAuto: no matching method with auto-converter {autoConverter} => try the next one");
          }
          continue;
        }
        if (1 < count) {
          log.Warn ($"InvokeMethodAuto: more than one matching method in {autoConverter} => consider the first one");
        }
        var method = matchingMethods.First ();
        return method.InvokeAutoConvert (autoConverter, instance, parameters);
      }

      // No auto-converter matches
      if (autoConverters.Any ()) {
        log.Warn ($"InvokeMethodAuto: no auto-converter matched");
      }
      Exception lastException = null;
      foreach (var method in methods) {
        try {
          return method.Invoke (instance, parameters);
        }
        catch (Exception ex) {
          log.Warn ($"InvokeMethodAuto: using {method} with no conversion failed", ex);
          lastException = ex;
        }
      }
      Debug.Assert (null != lastException);
      log.Error ($"InvokeMethodAuto: no method with name {methodName} could be applied with the provided parameters");
      throw new ArgumentException ("No method could be run with the provided parameters", "parameters", lastException);
    }
  }
}
