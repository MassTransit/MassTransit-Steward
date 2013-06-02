namespace MassTransit.Steward.Contracts
{
    using System;


    /// <summary>
    /// When access to a resource fails, this event should be published. This event
    /// may be correlated to a message.
    /// </summary>
    public interface ResourceUsageFailed 
    {
        /// <summary>
        /// A unique identifier for the event published
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// The CommandId (if present) being executed when the resource access failed
        /// </summary>
        Guid DispatchId { get; }

        /// <summary>
        /// The timestamp for when the resource access failed
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// The resource which was being accessed when the failure occurred
        /// </summary>
        Uri Resource { get; }

        /// <summary>
        /// The reason reported for the resource being unavailable
        /// </summary>
        string Reason { get; }
    }
}