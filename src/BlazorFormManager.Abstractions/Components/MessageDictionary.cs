using System.Collections.Generic;
using System.Linq;

namespace BlazorFormManager.Components
{
    /// <summary>
    /// Represents a dictionary that encapsulates data used to convey various messages.
    /// </summary>
    public class MessageDictionary : Dictionary<MessageType, ComponentMessage>, IEnumerable<ComponentMessage>
    {        
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDictionary"/> class.
        /// </summary>
        public MessageDictionary()
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Adds or updates a message of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the message to add or update.</param>
        /// <param name="message">The message text.</param>
        /// <param name="image">The image associated with the message.</param>
        public void Add(MessageType type, string message, ComponentImage? image = null)
        {
            base.Add(type, new(type, message, image));
        }

        /// <summary>
        /// Determines whether the underlying dictionary contains a
        /// message of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the message to locate in the dictionary.</param>
        /// <returns></returns>
        public bool Contains(MessageType type) => ContainsKey(type);

        /// <summary>
        /// Returns a message of the specified <paramref name="type"/>, if available.
        /// </summary>
        /// <param name="type">The type of the message to retrieve.</param>
        /// <returns></returns>
        public ComponentMessage? Get(MessageType type)
        {
            if (ContainsKey(type))
                return this[type];
            return null;
        }

        /// <summary>
        /// Determines whether the collection contains any <see cref="ComponentImage"/>
        /// with a non-blank <see cref="ComponentImage.Src"/> property value.
        /// </summary>
        /// <returns></returns>
        public bool HasImage() => Values.Any(v => !string.IsNullOrWhiteSpace(v.Image?.Src));

        /// <summary>
        /// Determines whether the collections contains any <see cref="ComponentImage"/>
        /// with a non-blank <see cref="ComponentImage.Src"/> property value corresponding
        /// to the specified <paramref name="types"/>.
        /// </summary>
        /// <param name="types">The types of the messages to look up.</param>
        /// <returns></returns>
        public bool HasImage(params MessageType[] types) => 
            Values.Any(v => types.Contains(v.Type) && !string.IsNullOrWhiteSpace(v.Image?.Src));

        /// <summary>
        /// Returns the first <see cref="ComponentMessage"/> that 
        /// matches any of the specified <paramref name="types"/>.
        /// </summary>
        /// <param name="types">The types of the messages to look up.</param>
        /// <returns></returns>
        public ComponentMessage? FirstOrDefault(params MessageType[] types)
        {
            foreach (var type in types)
            {
                if (Contains(type))
                    return this[type];
            }
            return null;
        }

        /// <summary>
        /// Returns the first danger or warning message.
        /// </summary>
        /// <returns></returns>
        public ComponentMessage? DangerOrWarning() => 
            FirstOrDefault(MessageType.Danger, MessageType.Warning);

        /// <summary>
        /// Returns the first success or info message.
        /// </summary>
        /// <returns></returns>
        public ComponentMessage? SuccessOrInfo() =>
            FirstOrDefault(MessageType.Success, MessageType.Info);

        IEnumerator<ComponentMessage> IEnumerable<ComponentMessage>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        #endregion
    }
}
