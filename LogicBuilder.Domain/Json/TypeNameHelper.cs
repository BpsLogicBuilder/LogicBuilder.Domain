using System;

namespace LogicBuilder.Domain.Json
{
    internal class TypeNameHelper : ITypeNameHelper
    {
        /// <summary>
        /// Gets the key "Namespace.TypeName, AssemblySimpleName" given the assembly qualified name.
        /// </summary>
        /// <param name="assemblyQualifiedName"></param>
        /// <returns></returns>
        public string? GetKey(string assemblyQualifiedName)
        {
            int typeNameEnd = IndexOfTopLevelComma(assemblyQualifiedName, 0);
            if (typeNameEnd < 0)
                return null;

            int assemblyNameEnd = IndexOfTopLevelComma(assemblyQualifiedName, typeNameEnd + 1);
            string typeName = assemblyQualifiedName.Substring(0, typeNameEnd).Trim();
            string assemblyName = (assemblyNameEnd < 0
                ? assemblyQualifiedName.Substring(typeNameEnd + 1)
                : assemblyQualifiedName.Substring(typeNameEnd + 1, assemblyNameEnd - typeNameEnd - 1)).Trim();

            if (typeName.Length == 0 || assemblyName.Length == 0)
                return null;

            return $"{typeName}, {assemblyName}";
        }

        /// <summary>
        /// Key is "Namespace.TypeName, AssemblySimpleName" so that version, culture and public key token changes do not break persisted JSON.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetKey(Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// Ignores commas for generic type arguments e.g. in the following case the first comma will be two characters before "System.Linq.Expressions"
        /// System.Linq.IQueryable`1[[Enrollment.Domain.Entities.LookUpsModel, Enrollment.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Linq.Expressions, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public int IndexOfTopLevelComma(string value, int startIndex)
        {
            int depth = 0;
            for (int i = startIndex; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '[':
                        depth++;
                        break;
                    case ']':
                        depth--;
                        break;
                    case ',' when depth == 0:
                        return i;
                }
            }

            return -1;
        }
    }
}
