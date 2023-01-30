// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Superclass for mutable not nullable types.
  /// </summary>
  [Serializable]
  public abstract partial class AdvancedMutableType : AdvancedNullableType
  {
    /// <summary>
    /// Initialize a new instance of the NotNullableMutableType class using a 
    /// <see cref="SqlType"/>. 
    /// </summary>
    /// <param name="sqlType">The underlying <see cref="SqlType"/>.</param>
    /// <param name="nullableColumn"></param>
    protected AdvancedMutableType (SqlType sqlType, bool nullableColumn = true)
      : base (sqlType, nullableColumn)
    {
    }

    /// <summary>
    /// Gets the value indicating if this IType is mutable.
    /// </summary>
    /// <value>true - a <see cref="MutableType"/> is mutable.</value>
    /// <remarks>
    /// This has been "sealed" because any subclasses are expected to be mutable.  If
    /// the type is immutable then they should inherit from <see cref="ImmutableType"/>.
    /// </remarks>
    public override sealed bool IsMutable
    {
      get { return true; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="original"></param>
    /// <param name="target"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <param name="copiedAlready"></param>
    /// <returns></returns>
    public override object Replace (object original, object target, ISessionImplementor session, object owner,
                     IDictionary copiedAlready)
    {
      if (IsEqual (original, target)) {
        return original;
      }

      return DeepCopy (original, session.Factory);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public abstract object DeepCopyNotNull (object value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public override object DeepCopy (object value, ISessionFactoryImplementor factory)
    {
      return (value == null) ? null : DeepCopyNotNull (value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="original"></param>
    /// <param name="target"></param>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <param name="copiedAlready"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<object> ReplaceAsync (object original, object target, ISessionImplementor session, object owner,
                     IDictionary copiedAlready, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) {
        return Task.FromCanceled<object> (cancellationToken);
      }
      try {
        return Task.FromResult<object> (Replace (original, target, session, owner, copiedAlready));
      }
      catch (Exception ex) {
        return Task.FromException<object> (ex);
      }
    }
  }
}
