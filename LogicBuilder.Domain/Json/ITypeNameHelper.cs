using System;

namespace LogicBuilder.Domain.Json
{
    internal interface ITypeNameHelper
    {
        /// <summary>
        /// Gets the key "Namespace.TypeName, AssemblySimpleName" given the assembly qualified name.
        /// </summary>
        /// <param name="assemblyQualifiedName"></param>
        /// <returns></returns>
        string? GetKey(string assemblyQualifiedName);

        /// <summary>
        /// Key is "Namespace.TypeName, AssemblySimpleName" so that version, culture and public key token changes do not break persisted JSON.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetKey(Type type);

        /// <summary>
        /// Ignores commas for generic type arguments e.g. in the following case the first comma will be two characters before "System.Linq.Expressions"
        /// System.Linq.IQueryable`1[[Enrollment.Domain.Entities.LookUpsModel, Enrollment.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Linq.Expressions, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        int IndexOfTopLevelComma(string value, int startIndex);
    }
}
