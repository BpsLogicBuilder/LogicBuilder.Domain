using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LogicBuilder.Domain.Json
{
    internal class TypeHelper(ITypeNameHelper typeNameHelper) : ITypeHelper
    {
        private readonly ITypeNameHelper typeNameHelper = typeNameHelper;

        public IReadOnlyDictionary<string, Type> BuildKnownTypes<T>(IEnumerable<Type> types)
        {
            return types.Where(t => IsAllowedType<T>(t)).Aggregate
            (
                new Dictionary<string, Type>(StringComparer.Ordinal),
                (dictionary, type) =>
                {
                    dictionary[typeNameHelper.GetKey(type)] = type;
                    return dictionary;
                }
            );
        }

        public bool IsAllowedType<T>(Type? type)
        {
            return type != null
                && !type.IsAbstract
                && !type.IsInterface
                && !type.ContainsGenericParameters
                && typeof(T).IsAssignableFrom(type);
        }

        public IEnumerable<Type> LoadTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }

        public Type? ResolveType(string typeString, IReadOnlyDictionary<string, Type> knownTypes)
        {
            string? key = typeNameHelper.GetKey(typeString);
            return key != null && knownTypes.TryGetValue(key, out Type? type) ? type : null;
        }
    }
}
