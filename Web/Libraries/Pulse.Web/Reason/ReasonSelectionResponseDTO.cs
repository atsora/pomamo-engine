// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;
using Pulse.Web.CommonResponseDTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Reponse DTO for ReasonSelection service
  /// </summary>
  [Api ("ReasonSelection Response DTO")]
  public class ReasonSelectionResponseDTO
  {
    ILog log = LogManager.GetLogger<ReasonSelectionResponseDTO> ();

    /// <summary>
    /// reason Id
    /// 
    /// 0 in case of a machine state template
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Classification ID
    /// 
    /// Reason ID or MST + machine state template ID
    /// </summary>
    public string ClassificationId { get; set; }

    /// <summary>
    /// Additional data
    /// </summary>
    public IDictionary<string, object> Data { get; set; }

    /// <summary>
    /// Text
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Long display: reason with its description
    /// </summary>
    public string LongDisplay { get; set; }

    /// <summary>
    /// Description of the reason
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Color
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Recommended reason score to apply
    /// </summary>
    public double ReasonScore { get; set; }

    /// <summary>
    /// Do not allow to set details
    /// </summary>
    public bool NoDetails { get; set; }

    /// <summary>
    /// Are details required for this reason ?
    /// </summary>
    public bool DetailsRequired { get; set; }

    /// <summary>
    /// Display of associated reason group
    /// </summary>
    public string ReasonGroupDisplay { get; set; }

    /// <summary>
    /// Reason group long display: reason group with its description
    /// </summary>
    public string ReasonGroupLongDisplay { get; set; }

    /// <summary>
    /// Description of the reason group
    /// </summary>
    public string ReasonGroupDescription { get; set; }

    /// <summary>
    /// Id of associated reason group
    /// </summary>
    public int ReasonGroupId { get; set; }

    #region IEquatable implementation
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (ReasonSelectionResponseDTO other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ReasonSelectionResponseDTO other = obj as ReasonSelectionResponseDTO;
      if (null == other) {
        return false;
      }
      return object.Equals (this.ClassificationId, other.ClassificationId);
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * this.ClassificationId.GetHashCode ();
      }
      return hashCode;
    }
    #endregion // IEquatable implementation
  }

  /// <summary>
  /// Assembler for ReasonSelectionResponseDTO.
  /// </summary>
  public class ReasonSelectionResponseDTOAssembler : IGenericDTOAssembler<ReasonSelectionResponseDTO, Lemoine.Model.IReasonSelection>
  {
    ILog log = LogManager.GetLogger<ReasonSelectionResponseDTOAssembler> ();

    /// <summary>
    /// ReasonSelectionDTO assembler
    /// </summary>
    /// <param name="reasonSelection"></param>
    /// <returns></returns>
    public ReasonSelectionResponseDTO Assemble (Lemoine.Model.IReasonSelection reasonSelection)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var reason = reasonSelection.Reason;
        var machineStateTemplate = reasonSelection.MachineStateTemplate;
        var reasonSelectionResponseDTO = new ReasonSelectionResponseDTO ();
        reasonSelectionResponseDTO.Id = reason?.Id ?? 0;
        reasonSelectionResponseDTO.ClassificationId = reasonSelection.ClassificationId;
        if (!string.IsNullOrEmpty (reasonSelection.AlternativeText)) {
          reasonSelectionResponseDTO.Display = reasonSelection.AlternativeText;
          reasonSelectionResponseDTO.LongDisplay = string.IsNullOrEmpty (reasonSelection.AlternativeLongText)
            ? reasonSelection.AlternativeText
            : reasonSelection.AlternativeLongText;
        }
        else if (null != reason) {
          reasonSelectionResponseDTO.Display = reason.Display;
          reasonSelectionResponseDTO.LongDisplay = reason.LongDisplay;
        }
        else if (null != machineStateTemplate) {
          reasonSelectionResponseDTO.Display = machineStateTemplate.Display;
          reasonSelectionResponseDTO.LongDisplay = machineStateTemplate.Display;
        }
        else {
          log.Fatal ($"Assemble: reason or machineStateTemplate must not be null");
          throw new InvalidOperationException ("Assemble: reason and machineStateTemplate are null");
        }
        if (null != reason) {
          reasonSelectionResponseDTO.Description = string.IsNullOrEmpty (reasonSelection.AlternativeDescription)
              ? reason.DescriptionOrTranslation
              : reasonSelection.AlternativeDescription;
          reasonSelectionResponseDTO.Color = reason.Color;
        }
        else if (null != machineStateTemplate) {
          reasonSelectionResponseDTO.Description = string.IsNullOrEmpty (reasonSelection.AlternativeDescription)
            ? ""
            : reasonSelection.AlternativeDescription;
          reasonSelectionResponseDTO.Color = ColorGenerator.GetColor ("MachineStateTemplate", machineStateTemplate.Id);
          // TODO: store the color
        }
        else {
          log.Fatal ($"Assemble: reason or machineStateTemplate must not be null");
          throw new InvalidOperationException ("Assemble: reason and machineStateTemplate are null");
        }

        var reasonGroup = reasonSelection.ReasonGroup;
        reasonSelectionResponseDTO.ReasonGroupId = reasonGroup.Id;
        // The extensions may not initialize the reason group
        reasonGroup = ModelDAOHelper.DAOFactory.ReasonGroupDAO.FindById (reasonGroup.Id);
        reasonSelectionResponseDTO.ReasonGroupDisplay = reasonGroup.Display;
        reasonSelectionResponseDTO.ReasonGroupLongDisplay = reasonGroup.LongDisplay;
        reasonSelectionResponseDTO.ReasonGroupDescription = reasonGroup.DescriptionOrTranslation;

        reasonSelectionResponseDTO.ReasonScore = reasonSelection.ReasonScore;
        reasonSelectionResponseDTO.NoDetails = reasonSelection.NoDetails;
        reasonSelectionResponseDTO.DetailsRequired = reasonSelection.DetailsRequired;
        reasonSelectionResponseDTO.Data = reasonSelection.Data;
        return reasonSelectionResponseDTO;
      }
    }

    /// <summary>
    /// ReasonSelectionDTO list assembler
    /// </summary>
    /// <param name="reasonSelections"></param>
    /// <returns></returns>
    public IEnumerable<ReasonSelectionResponseDTO> Assemble (IEnumerable<IReasonSelection> reasonSelections)
    {
      List<ReasonSelectionResponseDTO> reasonSelectionDTOList = new List<ReasonSelectionResponseDTO> ();
      foreach (IReasonSelection reasonSelection in reasonSelections.GroupSameReason ()) {
        reasonSelectionDTOList.Add (Assemble (reasonSelection));
      }
      return reasonSelectionDTOList;
    }
  }
}
