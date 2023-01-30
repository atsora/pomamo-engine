(*
 * Copyright (C) 2009-2023 Lemoine Automation Technologies
 *
 * SPDX-License-Identifier: Apache-2.0
 *)

namespace Lemoine.Plugin.MachiningProcessMilestones

open NHibernate.Criterion
open Lemoine.GDBPersistentClasses
open Lemoine.Database.Persistent
open Lemoine.Model
open Lemoine.FSharp.Model.Bound
open Lemoine.FSharp.Model.Range

type MilestoneDAO() =
  inherit VersionableNHibernateDAO<Milestone, Milestone, int> ()
  // Note: the table is not partitioned, to be able to use FindById without any machine parameter,
  //       inherit VersionableNHibernateDAO instead of VersionableByMachineNhibernateDAO
  
  let whereDateTimeInRange (criteria: NHibernate.ICriteria) = function
    | Empty -> criteria.Add (Expression.Sql ("FALSE")) |> ignore
    | Bounds (l, li, u, ui) ->
      begin
        match (l, li) with
        | MinusInfinity, _ -> ()
        | PlusInfinity, _ -> criteria.Add (Expression.Sql ("FALSE")) |> ignore // TODO: raise exception
        | Bound (x), true -> criteria.Add (Restrictions.Ge ("DateTime", x)) |> ignore
        | Bound (x), false -> criteria.Add (Restrictions.Gt ("DateTime", x)) |> ignore
        match (u, ui) with
        | MinusInfinity, _ -> criteria.Add (Expression.Sql ("FALSE")) |> ignore // TODO: raise exception
        | PlusInfinity, _ -> ()
        | Bound (x), true -> criteria.Add (Restrictions.Le ("DateTime", x)) |> ignore
        | Bound (x), false -> criteria.Add (Restrictions.Lt ("DateTime", x)) |> ignore
      end

  member this.FindInRangeAsync (machine:IMachine) = function
    | Empty -> task { return [] :> seq<Milestone>; }
    | Bounds (l, li, u, ui) as r ->
      task {
        let criteria = NHibernateHelper.GetCurrentSession().CreateCriteria<Milestone> () in
        criteria.Add (Restrictions.Eq ("Machine.Id", machine.Id)) |> ignore;
        whereDateTimeInRange criteria r;
        criteria.AddOrder (Order.Asc ("DateTime")) |> ignore;
        let! result = criteria.ListAsync<Milestone> () in
        return result :> seq<Milestone>
      }
