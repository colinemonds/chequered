using System;

namespace Chequered.Event
{
    //We do not want this type parameter to be variant since the entire point of IEventHandle is to force API consumers to 
    //explicitly state the types of their events.
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IEventHandle<TEvent>
    {
        /// <summary>
        ///   Broadcast an event to all listeners.
        ///   BEWARE: This method will FailFast() your application if any event handler throws an exception. Use the overloaded
        ///   version of Send() to avoid that behaviour.
        /// </summary>
        /// <param name="ev">The event you want to send.</param>
        void Send(TEvent ev);

        /// <summary>
        ///   Broadcast an event to all listeners.
        /// </summary>
        /// <param name="ev">The event you want to send.</param>
        /// <param name="onException">The action to execute when an exception occurs in the event handler.</param>
        void Send(TEvent ev, Action<Exception> onException);

        /// <summary>
        ///   Subscribe to events of this handle's type.
        /// </summary>
        /// <param name="callback">The method, delegate or lambda to call when an event of that type is fired.</param>
        /// <returns>A thread-safe token that you can dispose to cancel the subscription.</returns>
        IDisposable Subscribe(Action<TEvent> callback);

        /// <summary>
        ///   Subscribe to a single occurence of this handle's event type.
        /// </summary>
        /// <param name="callback">The method, delegate or lambda to call when an event of that type is fired.</param>
        /// <returns>
        ///   A thread-safe token that you can dispose to cancel the subscription. That token will automatically be disposed after
        ///   the event has fired once, but you can safely dispose it again (to no effect).
        /// </returns>
        IDisposable SubscribeOnce(Action<TEvent> callback);
    }
}