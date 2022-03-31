using System.Collections.Generic;

namespace Carfamsoft.Model2View.Shared.Collections
{
    /// <summary>
    /// Represents a dictionary of objects whose keys are an object's 
    /// properties' name and values their corresponding values.
    /// </summary>
    public sealed class ObjectDictionary : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDictionary"/>
        /// class using the specified object.
        /// </summary>
        /// <param name="obj">A non-nullable object.</param>
        public ObjectDictionary(object obj)
        {
            var props = obj.GetType().GetProperties();
            foreach (var pi in props) Add(pi.Name, pi.GetValue(obj));
        }
    }
}
