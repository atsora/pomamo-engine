module Atsora.CncFSharp

open Lemoine.Core.Log
open Lemoine.Threading

type CncModule(name) =
  let mutable cncAcquisitionId = 0 in
  let mutable logger = LogManager.GetLogger (name: string)
  let mutable dataHandler: IChecked = null

  member val Logger = logger

  member this.CncAcquisitionId
    with get () =
      cncAcquisitionId
    and set id =
      logger <- LogManager.GetLogger (sprintf "%s.%d" name id)
      cncAcquisitionId <- id

  member val CncAcquisitionName = "" with get, set

  member this.SetDataHandler dh =
    dataHandler <- dh

  member this.PauseCheck() =
    if dataHandler <> null then dataHandler.PauseCheck ()

  member this.ResumeCheck() =
    if dataHandler <> null then dataHandler.ResumeCheck ()

  member this.SetActive() = 
    if dataHandler <> null then dataHandler.SetActive ()
