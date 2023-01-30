// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;



namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Assembler for TranslationDTO.
  /// </summary>
  public class TranslationDTOAssembler
    : IGenericDTOAssembler<TranslationDTO, Lemoine.Model.ITranslation>
  {
    /// <summary>
    /// TranslationDTO assembler
    /// </summary>
    /// <param name="translation"></param>
    /// <returns></returns>
    public TranslationDTO Assemble(Lemoine.Model.ITranslation translation)
    {
      TranslationDTO translationDTO = new TranslationDTO();
      translationDTO.Locale = translation.Locale;
      translationDTO.TranslationKey = translation.TranslationKey;
      translationDTO.TranslationValue = translation.TranslationValue;
      return translationDTO;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="translations"></param>
    /// <returns></returns>
    public IEnumerable<TranslationDTO> Assemble (IEnumerable<Lemoine.Model.ITranslation> translations)
    {
      return translations.Select (translation => Assemble(translation));
    }
  }
}
