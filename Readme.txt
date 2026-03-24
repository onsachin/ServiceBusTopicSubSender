1. Topic: The entity where the publisher sends messages.
2. Subscription: A "virtual queue" that receives a copy of every message sent to the topic (or a filtered subset).
3. Publisher: The C# application sending the message.
4. Subscriber: The C# application receiving the message from a specific subscription.

5. Install nuget package
   dotnet add package Azure.Messaging.ServiceBus
   
6. 