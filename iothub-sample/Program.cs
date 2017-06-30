using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;

using Microsoft.Azure.Devices.Common.Exceptions;
using Newtonsoft.Json;
using System.Text;

namespace iothub_sample
{
    class Program
    {
        static RegistryManager registryManager;
        static string iotHubConnectionString = "HostName=muru-IoThub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=gkMVe8gqEqZlVsyaYujb4BvWyXDdyyIn53bVEFJVxZc=";

        static string iotHubUri = "muru-IoThub.azure-devices.net";
        static string deviceId = "muru-device1";
        static Device device;
        static DeviceClient deviceClient;

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            var deviceKey = AddDeviceAsync(deviceId).Result;

            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), Microsoft.Azure.Devices.Client.TransportType.Mqtt);

            Console.WriteLine("Client started sending messages...");

            SendMessageToCloudAsync();

            Console.ReadLine();

        }

        private static async Task<string> AddDeviceAsync(string deviceId)
        {
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
  
            Console.WriteLine("device key {0} = ", device.Authentication.SymmetricKey.PrimaryKey);

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        private static async void SendMessageToCloudAsync()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                var telemetryDataPoint = new
                {
                    deviceId = deviceId,
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }

        }
    }
}
