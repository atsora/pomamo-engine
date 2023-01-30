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
  /// Graphql type for <see cref="IProject"/>
  /// </summary>
  public class ProjectGraphType : ObjectGraphType<IProject>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProjectGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ProjectGraphType ()
    {
      Name = "Project";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      Field<ListGraphType<WorkInfoGraphType>> ("parents").Resolve (ctx => ctx.Source.WorkOrders);
      Field<ListGraphType<WorkInfoGraphType>, ICollection<IComponent>> ("children").Resolve (ctx => ctx.Source.Components);
      Field<CustomerGraphType, ICustomer> ("customer");
      Field<NonNullGraphType<UtcDateTimeGraphType>, DateTime> ("creationDateTime");
      Field<NonNullGraphType<UtcDateTimeGraphType>, DateTime> ("reactivationDateTime");
      Field<UtcDateTimeGraphType, DateTime?> ("archiveDateTime");

      Interface<DisplayableInterface> ();
      Interface<WorkInfoInterface> ();
    }
  }
}
