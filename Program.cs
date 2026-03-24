using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using AzureServiceBusSamples.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

string topicName="testtopic";
string subscriptionName = "testsub";
var cstr= builder.Configuration.GetValue<string>("ServiceBusConnectionString");
app.MapGet("/createtopic", async () =>
    {
        ServiceBusAdministrationClient adminClient = new ServiceBusAdministrationClient(cstr);

       if (!await adminClient.TopicExistsAsync(topicName))
       {
            await adminClient.CreateTopicAsync(topicName);
            Console.WriteLine("Topic created"); }

       if (!await adminClient.SubscriptionExistsAsync(topicName, subscriptionName)) {
            await adminClient.CreateSubscriptionAsync(topicName, subscriptionName);
            Console.WriteLine("Subscription created");
       }
       return await Task.FromResult("topic and subscription created");
    })
    .WithName("createtopic");

app.MapPost("/sendmessage", async ([FromBody]UserInfo user) =>
{

    ServiceBusClient serviceBusClient = new ServiceBusClient(cstr);
    ServiceBusSender sender = serviceBusClient.CreateSender(topicName);
    string jsonString = JsonSerializer.Serialize(user);
    await sender.SendMessageAsync(new ServiceBusMessage(jsonString));
   return await Task.FromResult("message sent: "+jsonString);
});

app.MapGet("/recievemessage", async () =>
{
    ServiceBusClient serviceBusClient = new ServiceBusClient(cstr);
   
   //Recieve singe message
/*   await using ServiceBusReceiver receiver = serviceBusClient.CreateReceiver(topicName, subscriptionName);
   
   ServiceBusReceivedMessage message= await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
   if (message != null)
   {
       Console.WriteLine(message.Body);
       await receiver.CompleteMessageAsync(message);
   }*/

    var processor = serviceBusClient.CreateProcessor(topicName, subscriptionName);
   processor.ProcessMessageAsync += async args =>
   {
      var messageBody = args.Message.Body.ToString();
      Console.WriteLine($"Received message: {messageBody}");
      await args.CompleteMessageAsync(args.Message);
   };

   processor.ProcessErrorAsync += args =>
   {
       Console.WriteLine(args.Exception.ToString());
       return Task.CompletedTask;
   };
   await processor.StartProcessingAsync();
});
app.Run();
