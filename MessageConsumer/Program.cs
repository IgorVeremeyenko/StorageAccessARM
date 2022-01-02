using MessageConsumer.Services;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MessageConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            //ProcessQueueAsync();
            Task.Run(() => ProcessQueueAsync());
            Console.ReadKey();
        }

        //private static async void ProcessQueueAsync()
        private static async Task ProcessQueueAsync()
        {
            StorageService storageService = new StorageService();
            var queue = storageService.Queue1;
            var table = storageService.Table1;

            while (true)
            {
                var messagesResponse = await queue.ReceiveMessagesAsync();
                var messages = messagesResponse.Value;
                if (messages.Any())
                {
                    foreach (var message in messages)
                    {

                        string messageString = message.MessageText;
                        if (message.DequeueCount > 2)
                        {
                            var badMessage = new BadMessage
                            {
                                PartitionKey = "BadMessages",
                                RowKey = Guid.NewGuid().ToString(),
                                Text = messageString
                            };
                            var insertOperation = TableOperation.Insert(badMessage);

                            await table.ExecuteAsync(insertOperation);

                            var response = await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                            Console.WriteLine($"Message \"{messageString}\" deleted");
                        }
                        else
                        {
                            try
                            {
                                var requestMessage = JsonConvert.DeserializeObject<ServiceRequestMessage>(messageString);
                                Console.WriteLine($"Broken item: {requestMessage.ItemName}{Environment.NewLine}Problem description: {requestMessage.Problem}");
                                var response = await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Tried to process bad message");
                            }
                        }
                    }
                }
                else
                    await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }

    public class ServiceRequestMessage
    {
        public string ItemName { get; set; }
        public string Problem { get; set; }
    }
}
