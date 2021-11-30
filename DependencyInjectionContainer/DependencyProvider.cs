﻿using DependencyInjectionContainer.CustomAttributes;
using DependencyInjectionContainer.CustomExceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public class DependencyProvider
    {
        private readonly DependenciesConfiguration _dependencyConfiguration;

        public DependencyProvider(DependenciesConfiguration dependencyConfiguration)
        {
            _dependencyConfiguration = dependencyConfiguration;
        }

        internal object Resolve(ParameterInfo parameter)
        {
            var name = parameter.GetCustomAttribute<DependencyKeyAttribute>()?.Key;
            return Resolve(parameter.ParameterType, name);
        }

        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        public TInterface Resolve<TInterface>(object name)
        {
            return (TInterface)Resolve(typeof(TInterface), name);
        }

        public object Resolve(Type @interface, object key = null)
        {
            if (typeof(IEnumerable).IsAssignableFrom(@interface))
            {
                return ResolveAll(@interface.GetGenericArguments()[0]);
            }
            var dependency = GetDependency(@interface, key);

            return ResolveDependency(dependency);
        }

        public IEnumerable<T> ResolveAll<T>()
            where T : class
        {
            return (IEnumerable<T>)ResolveAll(typeof(T));
        }

        public IEnumerable<object> ResolveAll(Type @interface)
        {
            if (_dependencyConfiguration.TryGetAll(@interface, out var dependencies))
            {
                var collection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(@interface));

                foreach (var dependency in dependencies)
                {
                    collection.Add(ResolveDependency(dependency));
                }

                return (IEnumerable<object>)collection;
            }

            return null;
        }

        private object ResolveDependency(Dependency dependency)
        {
            if (_dependencyConfiguration.IsExcluded(dependency.Type))
                throw new DependencyException($"Dependency type {dependency.Type} leads to recursion!");
            _dependencyConfiguration.ExcludeType(dependency.Type);

            object result = null;
            if (dependency.LifeCycle == LifeCycle.InstancePerDependency)
            {
                result = Creator.CreateInstance(dependency.Type, _dependencyConfiguration);
            }
            else if (dependency.LifeCycle == LifeCycle.Singleton)
            {
                lock (dependency)
                {
                    if (dependency.Instance == null)
                    {
                        result = Creator.CreateInstance(dependency.Type, _dependencyConfiguration);
                        dependency.Instance = result;
                    }
                    else
                    {
                        result = dependency.Instance;
                    }
                }
            }
            _dependencyConfiguration.RemoveFromExcluded(dependency.Type);

            return result;
        }

        private Dependency GetNamedDependency(Type @interface, object key)
        {
            if (_dependencyConfiguration.TryGetAll(@interface, out var namedDependencies))
            {
                foreach (var dependency in namedDependencies)
                {
                    if (key.Equals(dependency.Key)) return dependency;
                }
            }

            throw new DependencyException($"Dependency with [{key}] key for type {@interface} is not registered");
        }

        private Dependency GetDependency(Type @interface, object key = null)
        {
            if (@interface.IsGenericType &&
                _dependencyConfiguration.TryGet(@interface.GetGenericTypeDefinition(), out var genericDependency))
            {
                if (key != null)
                {
                    genericDependency = GetNamedDependency(@interface.GetGenericTypeDefinition(), key);
                }
                var genericType = genericDependency.Type.MakeGenericType(@interface.GenericTypeArguments);
                if (genericDependency.Instance == null)
                {
                    genericDependency.Instance = Creator.CreateInstance(genericType, _dependencyConfiguration);
                }
                
                var tempGenericDependency = new Dependency(genericType, genericDependency.LifeCycle, genericDependency.Key)
                { 
                    Instance = genericDependency.Instance 
                };

                return tempGenericDependency;
            }

            if (key != null) return GetNamedDependency(@interface, key);

            if (_dependencyConfiguration.TryGet(@interface, out var dependency))
            {
                return dependency;
            }

            throw new DependencyException($"Dependency for type {@interface} is not registered");
        }
    }
}
