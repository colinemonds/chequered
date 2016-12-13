# chequered
Chequered is a pattern library.

It implements a variety of design patterns in C# to help you build more robust and maintainable software. It aims to be not tied to any particular framework, technology, or use case, and is distributed as a PCL (Portable Class Library) to ensure maximum availability on all modern .NET platforms.

# Features
Chequered currently ships with the following features:

### Observer pattern
An event bus implementation. Essentially, this is what you are used to from PRISM, but it has the following enhancements:
* The chequered event bus is thread safe.
* Subscribing to events gives you a subscription token, which enables you to unsubscribe more easily.
* The chequered event bus properly handles exceptions raised by event handlers instead of swallowing them or canceling event processing for the event that raised them.
* There is a conveniece method for the case where you need to subscribe to only a single occurence of an event.

```csharp
var eventBus = new EventBus();
eventBus.GetHandle<int>.SubscribeOnce(i => Console.WriteLine($"Received integer {i}"));
//outputs "Received integer 42"
eventBus.GetHandle<int>.Send(42);
//nobody listens to this guy and nothing happens
eventBus.GetHandle<int>.Send(14);
```

### State pattern
A state machine, which chequered calls a "state manager". This object is not thread safe.
* Each object state is reified as a distinct object.
* Ships with optional push/pop semantics (actually a pushdown automaton).
* Allows you to do anything with the current state, then change the state depending on the computation's result.
