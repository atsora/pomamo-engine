// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for <see cref="IJob"/>
  /// </summary>
  public class JobGraphType : ObjectGraphType<IJob>
  {
    readonly ILog log = LogManager.GetLogger (typeof (JobGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public JobGraphType ()
    {
      Name = "Job";
      Field<NonNullGraphType<IdGraphType>> ("id").Resolve (ctx => ctx.Source.ProjectId);
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      Field<ListGraphType<WorkInfoGraphType>> ("parents").Resolve (ctx => null);
      Field<ListGraphType<WorkInfoGraphType>, ICollection<IComponent>> ("children")
        .Resolve (ctx => ctx.Source.Components);
      Field<CustomerGraphType, ICustomer> ("customer");
      Field<UtcDateTimeGraphType, DateTime?> ("deliveryDate");
      Field<NonNullGraphType<WorkOrderStatusGraphType>, IWorkOrderStatus> ("status");
      Field<NonNullGraphType<UtcDateTimeGraphType>, DateTime> ("creationDateTime");
      Field<NonNullGraphType<UtcDateTimeGraphType>, DateTime> ("reactivationDateTime");
      Field<UtcDateTimeGraphType, DateTime?> ("archiveDateTime");
      Field<int> ("quantity", nullable: true);

      Interface<DisplayableInterface> ();
      Interface<WorkInfoInterface> ();
    }
  }
}
