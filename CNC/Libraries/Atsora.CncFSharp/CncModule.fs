module Atsora.CncFSharp

open Lemoine.Core.Log
open Lemoine.Threading

type CncModule(name) =
  let mutable cncAcquisitionId = 0 in
  let mutable logger = LogManager.GetLogger (name: string)
  let mutable dataHandler: IChecked = null

  member this.Logger with get () = logger

  member val BaseLogCategory = name

  abstract member GetLogCategory: unit -> string
  default this.GetLogCategory () =
    sprintf "%s.%d" this.BaseLogCategory cncAcquisitionId

  abstract member UpdateLogger: unit -> unit
  default this.UpdateLogger () =
    let s = this.GetLogCategory () in
    logger <- LogManager.GetLogger s

  member this.CncAcquisitionId
    with get () =
      cncAcquisitionId
    and set id =
      cncAcquisitionId <- id
      this.UpdateLogger ()

  member val CncAcquisitionName = "" with get, set

  member this.SetDataHandler dh =
    dataHandler <- dh

  member this.PauseCheck() =
    if dataHandler <> null then dataHandler.PauseCheck ()

  member this.ResumeCheck() =
    if dataHandler <> null then dataHandler.ResumeCheck ()

  member this.SetActive() = 
    if dataHandler <> null then dataHandler.SetActive ()
