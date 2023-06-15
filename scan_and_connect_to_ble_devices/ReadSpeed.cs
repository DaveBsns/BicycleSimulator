using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace BLE
{
    class ReadSpeed
    {
        private static float previousSpeedInKph = 0f;
        private static int nonIncreasingCounter = 0;

        static DeviceInformation device = null;

        public static string MY_SERVICE_ID = "ee0c";

        public static string ANGLE = "00002720-0000-1000-8000-00805f9b34fb";

        public static string FITNESSMACHINE_UUID = "00001826-0000-1000-8000-00805f9b34fb";
        public static string INDOOR_BIKE_DATA = "00002ad2-0000-1000-8000-00805f9b34fb";


        public static string SPEED_UUID = "00001816-0000-1000-8000-00805f9b34fb";
        public static string SPEED_MEASUREMENT_CHARACTERISTIC_UUID = "00002a5b-0000-1000-8000-00805f9b34fb";


        public static string POWER_UUID = "00001818-0000-1000-8000-00805f9b34fb";
        public static string POWER_MEASUREMENT_CHARACTERISTIC_UUID = "00002a63-0000-1000-8000-00805f9b34fb";
        //private static object selectedCharacteristic;

        static async Task Main(string[] args)
        {
            // Query for extra properties you want returned
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // EnumerationCompleted and Stopped are optional to implement.
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            deviceWatcher.Start();
            while (true)
            {
                if (device == null)
                {
                    Thread.Sleep(200);
                    // Console.WriteLine($"Searching for [{device.Name}]");

                }
                else
                {

                    BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.Id);

                    GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync();

                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n \t Paired successfully with [{device.Name}]");
                        Console.ResetColor();
                        var services = result.Services;
                        //Console.WriteLine("\n Device UUID's \n ********");

                        foreach (var service in services)
                        {
                            /*
                            Console.WriteLine($"{service.Uuid}");
                            Console.WriteLine("----------");
                            */

                            if (service.Uuid == new Guid(FITNESSMACHINE_UUID))
                            {
                                GattCharacteristicsResult charactiristicResult = await service.GetCharacteristicsAsync();

                                if (charactiristicResult.Status == GattCommunicationStatus.Success)
                                {

                                    var characteristics = charactiristicResult.Characteristics;

                                    foreach (var characteristic in characteristics)
                                    {
                                        
                                        Console.WriteLine(characteristic.Uuid);
                                        Console.WriteLine(characteristic.UserDescription);
                                        Console.WriteLine("\n ---------------------");
                                        
                                        
                                        if (characteristic.Uuid == new Guid(INDOOR_BIKE_DATA))
                                        {
                                            Console.WriteLine("Detected the uuid");
                                            // Subscribe to value changed event for speed characteristic
                                            characteristic.ValueChanged += SpeedCharacteristic_ValueChanged;

                                            // Enable notifications for the speed characteristic
                                            GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                                GattClientCharacteristicConfigurationDescriptorValue.Notify);

                                            if (status == GattCommunicationStatus.Success)
                                            {
                                                Console.WriteLine("Speed notifications enabled.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Failed to enable speed notifications.");
                                            }
                                        }
                                        
                                        
                                     
                                    }
                                    Console.WriteLine("End of foreachh");
                                }
                            }
                                        


                        }


                        Console.WriteLine("\n press any key to exit");
                        Console.ReadKey();
                        break;
                    }

                }


            }
            deviceWatcher.Stop();
        }
        

        private static void SpeedCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            

            byte[] data = new byte[args.CharacteristicValue.Length];
            Windows.Storage.Streams.DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);

            int result = BitConverter.ToInt16(data, 2);
            double output = result * 0.001;
            if (output < 0)
            {
                output = output * (-1);
            }

            // Assuming the speed data is in a specific format, parse and print the speed value
            // Modify the parsing logic based on the data format used by your device

            int distanceValue = (data[1] << 8) | data[0];  // Example parsing logic for 16-bit little-endian data
            int bit0 = data[0];
            int bit1 = data[1];
            int bit2 = data[2];
            int bit3 = data[3];
            int bit4 = data[4];
            int bit5 = data[5];
            /*
            int timer = 0;
            foreach(var bit in data)
            {
                Console.WriteLine("Bit" + timer+ ": "+bit);
                timer++;
            }
            Console.WriteLine("-------------------------------");
            */

            Console.WriteLine("Speed: " + output);

        }

        private static void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            /*var reader = DataReader.FromBuffer(args.CharacteristicValue);
            var flags = reader.ReadByte();
            var value = reader.ReadByte();
            Console.WriteLine($"{flags} - {value}");*/
        }

        private static void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            // throw new NotImplementedException();
        }

        private static void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            //throw new NotImplementedException();
        }

        private static void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //throw new NotImplementedException();
        }

        private static void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //throw new NotImplementedException();
        }

        private static void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            Console.WriteLine(args.Name);
            if (args.Name == "DIRETO XR")
                device = args;
            /* else if (args.Name == "HEADWIND BC55")
                 device = args;*/

        }

    }
}