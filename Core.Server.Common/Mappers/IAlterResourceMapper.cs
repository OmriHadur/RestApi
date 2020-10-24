﻿using Core.Server.Common.Entities;
using Core.Server.Shared.Resources;
using System.Threading.Tasks;

namespace Core.Server.Common.Mappers
{
    public interface IAlterResourceMapper<TCreateResource, TUpdateResource, TResource, TEntity>
        : IResourceMapper<TResource, TEntity>
        where TCreateResource : CreateResource
        where TUpdateResource : UpdateResource
        where TResource : Resource
        where TEntity : Entity
    {
        Task<TEntity> Map(TCreateResource resource);
        Task Map(TCreateResource resource, TEntity entity);

        Task Map(TUpdateResource resource, TEntity entity);
    }
}
