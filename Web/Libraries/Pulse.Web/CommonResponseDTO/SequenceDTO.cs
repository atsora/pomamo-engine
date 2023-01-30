// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business;
using Lemoine.Business.Operation;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for Sequence
  /// </summary>
  [Api("Sequence Response DTO")]
  public class SequenceDTO
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public SequenceDTO () { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sequence"></param>
    internal protected SequenceDTO (Lemoine.Model.ISequence sequence)
    {
      this.Id = ((Lemoine.Collections.IDataWithId<int>)sequence).Id;
      this.Order = sequence.Order;
      this.Display = sequence.Display;
      this.Kind = sequence.Kind.ToString ();
      
      var sequenceStandardTime = ServiceProvider
        .Get(new SequenceStandardTime (sequence));
      if (sequenceStandardTime.HasValue) {
        this.StandardDuration = sequenceStandardTime.Value.TotalSeconds;
      }
    }
    
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Sequence order
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Display of machine
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Kind of the sequence:
    /// <item>Machining</item>
    /// <item>Stop</item>
    /// <item>OptionalStop</item>
    /// <item>NonMachining</item>
    /// <item>AutoPalletChange</item>
    /// </summary>
    public string Kind { get; set; }
    
    /// <summary>
    /// Machining duration in seconds
    /// 
    /// Taken from the database, or guessed from the number of sequences in the operation
    /// </summary>
    public double? StandardDuration { get; set; }
  }
  
  /// <summary>
  /// Assembler for SequenceDTO
  /// </summary>
  public class SequenceDTOAssembler : IGenericDTOAssembler<SequenceDTO, Lemoine.Model.ISequence>
  {
    /// <summary>
    /// SequenceDTO assembler
    /// </summary>
    /// <param name="sequence">nullable</param>
    /// <returns></returns>
    public SequenceDTO Assemble(Lemoine.Model.ISequence sequence)
    {
      if (null == sequence) {
        return null;
      }
      SequenceDTO sequenceDTO = new SequenceDTO(sequence);
      return sequenceDTO;
    }
    
    /// <summary>
    /// SequenceDTO list assembler (default display)
    /// </summary>
    /// <param name="sequences"></param>
    /// <returns></returns>
    public IEnumerable<SequenceDTO> Assemble(IEnumerable<Lemoine.Model.ISequence> sequences)
    {
      Debug.Assert (null != sequences);
      
      IList<SequenceDTO> result = new List<SequenceDTO> ();
      foreach (var sequence in sequences) {
        result.Add (Assemble (sequence));
      }
      return result;
    }
  }
}
