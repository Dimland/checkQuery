using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    const string ServiceBusConnectionString = "Your connection string"; 
    static string[] QueueNames = new string[] { "Your queues" };

    static IQueueClient[] queueClients;

    public static async Task Main(string[] args)
    {
        queueClients = new IQueueClient[QueueNames.Length];

        for (int i = 0; i < QueueNames.Length; i++)
        {
            queueClients[i] = new QueueClient(ServiceBusConnectionString, QueueNames[i]);
            RegisterOnMessageHandlerAndReceiveMessages(queueClients[i]);
        }

        Console.WriteLine("======================================================");
        Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
        Console.WriteLine("======================================================");

        Console.ReadKey();

        foreach (var client in queueClients)
        {
            await client.CloseAsync();
        }
    }

    static void RegisterOnMessageHandlerAndReceiveMessages(IQueueClient queueClient)
    {
        var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
        {
            MaxConcurrentCalls = 1,
            AutoComplete = false
        };

        queueClient.RegisterMessageHandler((message, token) => ProcessMessagesAsync(message, token, queueClient), messageHandlerOptions);
    }

    static async Task ProcessMessagesAsync(Message message, CancellationToken token, IQueueClient queueClient)
    {
        Console.WriteLine($"{queueClient.QueueName}\nReceived message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
    }

    static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
    {
        Console.WriteLine($"Message handler encountered an exception: {exceptionReceivedEventArgs.Exception}.");
        return Task.CompletedTask;
    }
}