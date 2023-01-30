// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for Component
  /// </summary>
  [Api("Component Response DTO")]
  public class ComponentDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of machine
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Documentation path
    /// </summary>
    public string DocumentLink { get; set; }
  }
  
  /// <summary>
  /// Assembler for ComponentDTO
  /// </summary>
  public class ComponentDTOAssembler : IGenericDTOAssembler<ComponentDTO, Lemoine.Model.IComponent>
  {
    readonly ILog log = LogManager.GetLogger<ComponentDTOAssembler> ();

    /// <summary>
    /// ComponentDTO assembler
    /// </summary>
    /// <param name="component">nullable</param>
    /// <returns></returns>
    public ComponentDTO Assemble(Lemoine.Model.IComponent component)
    {
      if (null == component) {
        return null;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (component)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.ComponentDTO.Assemble")) {
            var initialized = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.ComponentDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)component).Id);
            if (null == initialized) {
              log.Error ($"Assemble: component with id {((Lemoine.Collections.IDataWithId<int>)component).Id} does not exist");
              return null;
            }
            else {
              return Assemble (initialized);
            }
          }
        }
      }
      ComponentDTO componentDTO = new ComponentDTO();
      componentDTO.Id = ((Lemoine.Collections.IDataWithId<int>)component).Id;
      componentDTO.Display = component.Display;
      componentDTO.DocumentLink = component.DocumentLink;
      return componentDTO;
    }
    
    /// <summary>
    /// ComponentDTO list assembler (default display)
    /// </summary>
    /// <param name="components"></param>
    /// <returns></returns>
    public IEnumerable<ComponentDTO> Assemble(IEnumerable<Lemoine.Model.IComponent> components)
    {
      Debug.Assert (null != components);
      
      IList<ComponentDTO> result = new List<ComponentDTO> ();
      foreach (var component in components) {
        result.Add (Assemble (component));
      }
      return result;
    }
  }
}
