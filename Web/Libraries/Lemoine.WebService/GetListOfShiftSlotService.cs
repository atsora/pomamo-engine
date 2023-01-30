// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.DTO;
using Lemoine.Collections;
using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetListOfShiftSlot service
  /// </summary>
  public class GetListOfShiftSlotService: GenericCachedService<Lemoine.DTO.GetListOfShiftSlot>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetListOfShiftSlotService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GetListOfShiftSlotService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)//RR to change
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetListOfShiftSlot request)
    {
      DateTime dateOfRequest = DateTime.UtcNow;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {

        DateTime? datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.Begin);
        DateTime beginDateTime = datetime.HasValue ? datetime.Value : dateOfRequest;
        
        datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.End);
        DateTime endDateTime =  datetime.HasValue ? datetime.Value :  dateOfRequest;
        
        if (beginDateTime > endDateTime)  {
          // bad range
          return ServiceHelper.BadDateTimeRange(beginDateTime, endDateTime);
        }
        
        IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
          .FindOverlapsRange (new UtcDateTimeRange (beginDateTime, endDateTime));
        
        if ( slots.Count == 0 ) {
          // NO SHIFT DEFINED
          return ServiceHelper.NoShiftSlotErrorDTO();
        } //else 
        
        if (object.Equals (beginDateTime, dateOfRequest) && slots[0].BeginDateTime.HasValue) {
          beginDateTime = slots[0].BeginDateTime.Value;
        }
        if ( (endDateTime == dateOfRequest) && (slots[0].EndDateTime.HasValue) ) {
          endDateTime = slots[0].EndDateTime.Value;
        }
          
        Lemoine.DTO.ListOfShiftRangeDTO listOfShiftRangeDTO =
          (new Lemoine.DTO.ListOfShiftRangeDTOAssembler())
          .Assemble(new Tuple<DateTime,DateTime,IList<IShiftSlot> >(beginDateTime, endDateTime, slots));
        
        return listOfShiftRangeDTO;
      }
    }
  }
}
