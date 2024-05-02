namespace BlazorFormManager.Components
{
    /// <summary>
    /// Encapsulates data used to convey various messages.
    /// </summary>
    public class ComponentMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentMessage"/> class.
        /// </summary>
        public ComponentMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentMessage"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="message">The message.</param>
        /// <param name="image">The image associated with the message.</param>
        public ComponentMessage(MessageType type, string message, ComponentImage? image = null)
        {
            Type = type;
            Message = message;
            Image = image;
        }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        public virtual MessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public virtual string? Message { get; set; }

        /// <summary>
        /// Gets or sets the image associated with the message.
        /// </summary>
        public virtual ComponentImage? Image { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Message}";

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentMessage"/> class 
        /// of type <see cref="MessageType.Info"/> using the specified parameters.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="image">The image associated with the message.</param>
        /// <returns>An initialized instance of the <see cref="ComponentMessage"/> class.</returns>
        public static ComponentMessage Info(string message, ComponentImage? image = null)
            => new(MessageType.Info, message, image);

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentMessage"/> class 
        /// of type <see cref="MessageType.Success"/> using the specified parameters.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="image">The image associated with the message.</param>
        /// <returns>An initialized instance of the <see cref="ComponentMessage"/> class.</returns>
        public static ComponentMessage Success(string message, ComponentImage? image = null)
            => new(MessageType.Success, message, image);

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentMessage"/> class 
        /// of type <see cref="MessageType.Warning"/> using the specified parameters.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="image">The image associated with the message.</param>
        /// <returns>An initialized instance of the <see cref="ComponentMessage"/> class.</returns>
        public static ComponentMessage Warning(string message, ComponentImage? image = null)
            => new(MessageType.Warning, message, image);

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentMessage"/> class 
        /// of type <see cref="MessageType.Danger"/> using the specified parameters.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="image">The image associated with the message.</param>
        /// <returns>An initialized instance of the <see cref="ComponentMessage"/> class.</returns>
        public static ComponentMessage Danger(string message, ComponentImage? image = null)
            => new(MessageType.Danger, message, image);
    }
}
