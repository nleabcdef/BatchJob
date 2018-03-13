using System;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Dto for messaging hook.
    /// </summary>
    public class MessageHook
    {
        public MessageType Type { get; set; }
        public string Purpose { get; set; }
        public string Text { get; set; }
        public DateTime CreatedUtc { get; protected set; }
        public string DateTimeStampUtc => CreatedUtc.ToString(_dateFormat);

        const string _dateFormat = "yyyy-MM-ddTHH:mm:fffffffZ";
        const string _format = "{0} #[ Purpose: {1} - Type: {2} - Message: {3} ]#";

        protected MessageHook() => CreatedUtc = DateTime.UtcNow;
        
        public MessageHook(string text, string purpose, MessageType type)
            : this()
        {
            Text = string.IsNullOrWhiteSpace(text) ? throw new ArgumentNullException(nameof(text)) : text;
            Purpose = string.IsNullOrWhiteSpace(purpose) ? throw new ArgumentNullException(nameof(purpose)) : purpose;
            Type = type;
        }

        public override string ToString()
        {
            return string.Format(_format, DateTimeStampUtc, Purpose, Type, Text);
        }
    }
}
