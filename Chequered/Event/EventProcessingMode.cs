namespace Chequered.Event
{
    /// <summary>
    ///   EventProcessingMode allows your application to determine how the <see cref="EventBus" /> shall handle recursive event
    ///   sending situations.
    ///   For example, let's say an object sends an event A, which two callbacks listen to. These callbacks will be called in
    ///   an arbitrary order. If the first callback sends another event of type B, then what should happen first: should the
    ///   remaining callback for the A event be processed first, or should the listeners to the event B be processed first?
    ///   Chequered allows you to suit this behaviour to your application's needs.
    /// </summary>
    public enum EventProcessingMode
    {
        /// <summary>
        ///   Event processing will be delayed until processing of the current event has finished. This is the default behaviour in
        ///   Chequered.
        ///   If an event handler for an event A fires an event B, all remaining handlers for A will be processed before the
        ///   processing of B begins.
        /// </summary>
        BreadthFirst,

        /// <summary>
        ///   Events will always be processed immediately. This is the default behaviour you are used to from most event bus
        ///   implementations.
        ///   If an event handler for an event A fires an event B, all handlers for B will be processed before the processing of A
        ///   continues.
        /// </summary>
        DepthFirst,
    }
}