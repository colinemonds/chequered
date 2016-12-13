namespace Chequered.Event
{
    /// <summary>
    ///   An event bus (also known as message bus, event aggregator) allows you to decouple occurences of events from the logic
    ///   that responds to such occurences.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        ///   Get a handle for an event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event that you want to send or listen for.</typeparam>
        /// <returns>An object that you can use to send and receive events of type TEvent.</returns>
        IEventHandle<TEvent> GetHandle<TEvent>();
    }
}