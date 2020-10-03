﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Server.Common.Applications;
using Unity;
using Microsoft.Extensions.Logging;
using Core.Server.Shared.Resources;
using Core.Server.Shared.Query;

namespace Core.Server.Web.Controllers
{
    public class BatchController<TCreateResource, TUpdateResource, TResource, TApplication>
        : RestController<TCreateResource, TUpdateResource, TResource, TApplication>
        where TCreateResource : CreateResource
        where TUpdateResource : UpdateResource
        where TResource : Resource
        where TApplication : IBatchApplication<TCreateResource, TUpdateResource, TResource>

    {
        [HttpGet("batch")]
        public virtual async Task<ActionResult<IEnumerable<TResource>>> BatchGet(string[] ids)
        {
            return await Application.BatchGet(ids);
        }

        [HttpPost("batch")]
        public virtual async Task<ActionResult<IEnumerable<TResource>>> BatchCreate(TCreateResource[] resources)
        {
            return await Application.BatchCreate(resources);
        }

        [HttpPut("batch")]
        public virtual async Task<ActionResult<IEnumerable<TResource>>> BatchUpdate(TUpdateResource[] resources)
        {
            return await Application.BatchUpdate(resources);
        }

        [HttpDelete("batch")]
        public virtual async Task<ActionResult> BatchDelete(string[] ids)
        {
            return await Application.BatchDelete(ids);
        }
    }
}
