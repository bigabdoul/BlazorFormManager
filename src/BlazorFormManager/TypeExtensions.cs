using System;

namespace BlazorFormManager
{
    /// <summary>
    /// Provides extension methods for types.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Generates a globally unique identifier for the specified type.
        /// </summary>
        /// <param name="type">The type whose name will be prefixed to the identifier.</param>
        /// <returns></returns>
        public static string GenerateId(this Type type) => type.Name.GenerateId();
        
        /// <summary>
        /// Generates a globally unique identifier for the specified string.
        /// </summary>
        /// <param name="name">The name used to prefix the identifier.</param>
        /// <returns></returns>
        public static string GenerateId(this string name) => $"{name}_{Guid.NewGuid().GetHashCode():X}";
    }
}
