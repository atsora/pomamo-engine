(*
* Copyright (C) 2009-2023 Lemoine Automation Technologies
*
* SPDX-License-Identifier: Apache-2.0
*)

namespace Lemoine.FSharp.UnitTests

open System
open NUnit.Framework

open Lemoine.Math

[<TestFixture>]
type Math_UnitTest () =

  [<Test>]
  member this.TestComputeDistanceFromPositions() =
    let r = computeDistanceFromPositions [ 3.<inch>; 4.<inch> ] in
    Assert.IsTrue(5.<inch> = r)

  [<Test>]
  member this.TestComputeAngleDegrees() =
    let v1 = computeVector [ 0.; 0.] [ 0.; 5. ] in
    let v2 = computeVector [ 0.; 0.] [ 3.; 3. ] in
    let a = computeAngleDegrees v1 v2 in
    Assert.IsTrue(45. = a)
