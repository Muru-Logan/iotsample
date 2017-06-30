using System;
using System.Threading.Tasks;

using Microsoft.ServiceBus.Messaging;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace iothub_cloud
{
    class Program
    {
        static string iotHubConnectionString = "HostName=muru-IoThub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=gkMVe8gqEqZlVsyaYujb4BvWyXDdyyIn53bVEFJVxZc=";
        static string iotD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Receiving messages, press Ctrl+C to quit");

            eventHubClient = EventHubClient.CreateFromConnectionString(iotHubConnectionString, iotD2cEndpoint);

            var cancelTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancelTokenSource.Cancel();
                Console.WriteLine("Quiting...");
            };

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            var tasks = new List<Task>();

            foreach (var partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cancelTokenSource.Token));
            }

            Task.WaitAll(tasks.ToArray());
        }



        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);

            while (true)
            {
                if (ct.IsCancellationRequested) break;
                var eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);


            }
        }
    }
}
