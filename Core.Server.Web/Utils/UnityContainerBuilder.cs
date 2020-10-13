﻿using Core.Server.Application.Helpers;
using Core.Server.Common;
using Core.Server.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace Core.Server.Web.Utils
{
    public class UnityContainerBuilder
    {
        private readonly Dictionary<Type, Type> interfaceToType;
        private readonly IUnityContainer container;
        private readonly IReflactionHelper reflactionHelper;
        public UnityContainerBuilder(IUnityContainer container, IReflactionHelper reflactionHelper)
        {
            this.container = container;
            this.reflactionHelper = reflactionHelper;
            interfaceToType = new Dictionary<Type, Type>();
        }
        public void ConfigureContainer()
        {
            AddAllTypesForBundles();
            AddInjectTypes();
            //AddInjectWithNameClasses();
        }

        private void AddAllTypesForBundles()
        {
            var resourcesBoundles = reflactionHelper.GetResourcesBoundles();
            var genricTypesWithNameForBundle = GetInjectBoundleWithNameForBundle();

            foreach (var resourcesBoundle in resourcesBoundles) 
                foreach (var genricTypeWithNameForBundle in genricTypesWithNameForBundle)
                    AddGenricTypeWithNameForBundle(genricTypeWithNameForBundle, resourcesBoundle);
        }

        private void AddGenricTypeWithNameForBundle(Type genricTypeForBundle, ResourceBoundle resourcesBoundle)
        {
            var filledGenericType = reflactionHelper.FillGenericType(genricTypeForBundle, resourcesBoundle);
            foreach (var interfaceType in filledGenericType.GetInterfaces())
                InjectWithName(filledGenericType, interfaceType);
        }

        private IEnumerable<Type> GetInjectBoundleWithNameForBundle()
        {
            return reflactionHelper.GetGenericTypesWithAttribute<InjectBoundleWithNameAttribute>();
        }

        private void AddInjectTypes()
        {
            var classTypes = reflactionHelper.GetTypesWithAttribute<InjectAttribute>();
            foreach (var injectedType in classTypes)
                AddTypesInterfaces(injectedType);
        }

        private void InjectWithName(Type type,Type interfaceType)
        {
            var argsNames = type.GetGenericArguments().Select(t => t.Name);
            var argsNamesAsString= string.Join(",", argsNames);
            var name = $"{type.Name}<{argsNamesAsString}>";
            container.RegisterType(interfaceType, type, name);
        }

        private void AddTypesInterfaces(Type type)
        {
            foreach (var interType in type.GetInterfaces())
            {
                if (!interType.IsGenericType || !type.IsGenericType)
                    container.RegisterType(interType, type);
                else
                {
                    var interGenericType = interType.GetGenericTypeDefinition();
                    var typeArgs = type.GetGenericArguments();
                    var interArgs= interType.GetGenericArguments();
                   if (typeArgs.Length== interArgs.Length)
                        container.RegisterType(interGenericType, type);
                    else
                        RegisterFactory(type, interGenericType, typeArgs);
                }
            }
                
        }

        private void RegisterFactory(Type type, Type interGenericType, Type[] typeArgs)
        {
            container.RegisterFactory(interGenericType, (uc, interTypeWithGeneric, obj) =>
            {
                var typeGenericType = GetTypeGenericType(type, typeArgs, interTypeWithGeneric);
                return uc.Resolve(typeGenericType);
            });
        }

        private Type GetTypeGenericType(Type type, Type[] typeArgs, Type interTypeWithGeneric)
        {
            if (interfaceToType.ContainsKey(interTypeWithGeneric))
                return interfaceToType[interTypeWithGeneric];
            var firstGen = interTypeWithGeneric.GetGenericArguments().First();
            var prefix = reflactionHelper.GetPrefixName(firstGen);
            var args = GetArguments(typeArgs, prefix).ToArray();
            var typeGenericType = type.MakeGenericType(args);
            interfaceToType.Add(interTypeWithGeneric, typeGenericType);
            return typeGenericType;
        }

        private IEnumerable<Type> GetArguments(Type[] typeArgs, string prefix)
        {
            foreach (var type in typeArgs)
            {
                var typeName = prefix + type.Name.Substring(1);
                yield return reflactionHelper.GetTypeByName(typeName);
            }
        }
    }
}