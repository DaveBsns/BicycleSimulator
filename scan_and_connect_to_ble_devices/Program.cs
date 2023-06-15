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
    class Program
    {
        
        static DeviceInformation device = null;

        public static string MY_SERVICE_ID = "ee0c";
        public static string FITNESSMACHINE = "1826";
        //private static object selectedCharacteristic;

        static async Task OldMain(string[] args)
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
                            /*Console.WriteLine($"{service.Uuid}");
                            Console.WriteLine("----------");*/

                            if(service.Uuid == new Guid("00001826-0000-1000-8000-00805f9b34fb"))
                            { 
                            
                                GattCharacteristicsResult charactiristicResult = await service.GetCharacteristicsAsync();

                                

                                    if (charactiristicResult.Status == GattCommunicationStatus.Success)
                                    {
                                        var characteristics = charactiristicResult.Characteristics;
                                        foreach (var characteristic in characteristics)
                                        {
                                            /*var charateristicKey = characteristic.Uuid.ToString("N").Substring(4, 4).ToUpper();
                                            string characteristicName = "Unknown";
                                            if (GattServiceUUID.Lookup.ContainsKey(charateristicKey))
                                                characteristicName = GattServiceUUID.Lookup[charateristicKey].Item2;

                                            Console.WriteLine($"\t [{characteristicName}] - [{characteristic.Uuid}]");

                                            Console.WriteLine("----------");
                                            Console.WriteLine($"\t{characteristic.Uuid}");*/
                                            if (characteristic.Uuid == new Guid("00002ad9-0000-1000-8000-00805f9b34fb"))
                                            {
                                                GattCharacteristicProperties properties = characteristic.CharacteristicProperties;
                                        
                                                if (properties.HasFlag(GattCharacteristicProperties.Indicate))
                                                {
                                                
                                                    // This characteristic supports subscribing to notifications.
                                                    GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                                    GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                                                    if (status == GattCommunicationStatus.Success)
                                                    {
                                                        characteristic.ValueChanged += Characteristic_ValueChanged;
                                                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                                                        Console.WriteLine("\n \t Indications/Notifications turned on for characteristic " + characteristic.Uuid);
                                                        Console.ResetColor();
                                                        // Server has been informed of clients interest.
                                                    }
                                                }
                                                if (properties.HasFlag(GattCharacteristicProperties.Write))
                                                {
                                                
                                                        // This characteristic supports writing to it.
                                                    
                                                        var writer = new DataWriter();
                                                        writer.WriteByte(0x00);

                                                        GattCommunicationStatus resultWrite = await characteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithResponse);
                                                    if(resultWrite == GattCommunicationStatus.Success)
                                                    {
                                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                                    Console.WriteLine("\n \t 0x00 Value written to access control");
                                                    Console.ResetColor();
                                                    }

                                                    /*var writer1 = new DataWriter();
                                                    writer1.WriteByte(0x01);
                                                    GattCommunicationStatus resultWrite1 = await characteristic.WriteValueAsync(writer1.DetachBuffer(), GattWriteOption.WriteWithResponse);
                                                    if (resultWrite1 == GattCommunicationStatus.Success)
                                                    {
                                                        Console.WriteLine("Value 0x01 written to reset");
                                                    }*/

                                                    while (true)
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                                        Console.WriteLine("\n  *** Enter a value between 1-100 to increase or decrease the resistance of the Trainer  OR  enter X to exit *** \n");
                                                        Console.ResetColor();
                                                        var userInput = Console.ReadLine();

                                                        if (userInput.ToUpper() == "X")
                                                        {
                                                            break;
                                                        }
                                                        else if (int.TryParse(userInput, out int value) && value >= 0 && value <= 100)
                                                        {

                                                            //converting the user input to a byte array
                                                            int valueTowrite = int.Parse(userInput);

                                                            byte byteValue = Convert.ToByte(valueTowrite);

                                                            var writer2 = new DataWriter();
                                                            writer2.WriteByte(0x04);
                                                            writer2.WriteByte(byteValue);
                                                            GattCommunicationStatus resultWrite2 = await characteristic.WriteValueAsync(writer2.DetachBuffer(), GattWriteOption.WriteWithResponse);
                                                            if (resultWrite2 == GattCommunicationStatus.Success)
                                                            {
                                                                Console.ForegroundColor = ConsoleColor.Green;
                                                                Console.WriteLine($"\n -------Resistance set to [{userInput}] level-------");
                                                                Console.ResetColor();
                                                            }
                                                        }
                                                        else
                                                        {
                                                        /*Console.WriteLine("Failed to set the resistance enter right values between 1-100 or 'X' to exit the loop");*/
                                                            Console.ForegroundColor = ConsoleColor.Red;
                                                            Console.WriteLine("\n Failed to set the resistance enter right values between 1-100 or 'X' to exit the loop");
                                                            Console.ResetColor();

                                                        }

                                                    }


                                                }
                                            

                                            }

                                        }



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