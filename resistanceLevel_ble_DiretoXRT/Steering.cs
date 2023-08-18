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
using System.Text.Json;
using System.IO;
using System;
using System.Net;
using System.Net.Sockets;


namespace BLE {
  class Steering {
    private static byte[] previousData;

    static DeviceInformation device = null;

    private const int Port = 1234;

    public static string MY_SERVICE_ID = "ee0c";

    public static string ANGLE = "00002720-0000-1000-8000-00805f9b34fb";

    public static string FITNESSMACHINE_UUID = "00001826-0000-1000-8000-00805f9b34fb";
    public static string INDOOR_BIKE_DATA = "00002ad2-0000-1000-8000-00805f9b34fb";


    public static string SPEED_UUID = "00001816-0000-1000-8000-00805f9b34fb";
    public static string SPEED_MEASUREMENT_CHARACTERISTIC_UUID = "00002a5b-0000-1000-8000-00805f9b34fb";


    public static string POWER_UUID = "00001818-0000-1000-8000-00805f9b34fb";
    public static string POWER_MEASUREMENT_CHARACTERISTIC_UUID = "00002a63-0000-1000-8000-00805f9b34fb";
    //private static object selectedCharacteristic;

    static async Task OldMain(string[] args) {
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
      while (true) {
        if (device == null) {
          Thread.Sleep(200);
          // Console.WriteLine($"Searching for [{device.Name}]");

        } else {

          BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.Id);

          GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync();

          if (result.Status == GattCommunicationStatus.Success) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n \t Paired successfully with [{device.Name}]");
            Console.ResetColor();
            var services = result.Services;
            //Console.WriteLine("\n Device UUID's \n ********");

            foreach (var service in services) {
              /*
              Console.WriteLine($"{service.Uuid}");
              Console.WriteLine("----------");
              */

              if (service.Uuid == new Guid("347b0001-7635-408b-8918-8ff3949ce592")) {
                GattCharacteristicsResult charactiristicResult = await service.GetCharacteristicsAsync();

                if (charactiristicResult.Status == GattCommunicationStatus.Success) {

                  var characteristics = charactiristicResult.Characteristics;

                  foreach (var characteristic in characteristics) {
                    /*
                    Console.WriteLine(characteristic.Uuid);
                    Console.WriteLine(characteristic.UserDescription);
                    Console.WriteLine("\n ---------------------");
                    */

                    if (characteristic.Uuid == new Guid("347b0030-7635-408b-8918-8ff3949ce592")) {
                      Console.WriteLine("Detected the uuid");
                      // Subscribe to value changed event for speed characteristic
                      characteristic.ValueChanged += SpeedCharacteristic_ValueChanged;

                      // Enable notifications for the speed characteristic
                      /*
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
                     */
                      // Enable notifications for the characteristic
                      GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                          GattClientCharacteristicConfigurationDescriptorValue.Notify);

                      if (status == GattCommunicationStatus.Success) {
                        Console.WriteLine("Notifications enabled.");
                      } else {
                        Console.WriteLine("Failed to enable notifications.");
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


    private static void SpeedCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args) {
      byte[] data = new byte[args.CharacteristicValue.Length];
      Windows.Storage.Streams.DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

      float steering;
      Console.WriteLine(data[3]);
      if (data[3] == 65) {
        steering = 1;
      } else if (data[3] == 193) {
        steering = -1;
      } else {
        steering = 0;
      }

      SendData("127.0.0.1", steering.ToString());

      // Compare current data with previous data
      /*
      if (previousData != null && !ByteArrayEquals(previousData, data))
      {
          Console.WriteLine("Data changed:");
          for (int i = 0; i < data.Length; i++)
          {
              if (previousData[i] != data[i])
              {
                  Console.WriteLine($"Byte {i}: {data[i]} (changed from {previousData[i]})");
              }
          }
      }

      // Update previous data with current data
      previousData = data.Clone() as byte[];
      */



    }

    private static void WriteToFile(string json) {
      while (FileIsLocked(@"C:\Users\ekate\Desktop\MixedRealitz\steering.json", FileAccess.ReadWrite)) {
        Thread.Sleep(1);
      }
      File.WriteAllText(@"C:\Users\ekate\Desktop\MixedRealitz\steering.json", json);
    }

    private static bool FileIsLocked(string filename, FileAccess file_access) {
      // Try to open the file with the indicated access.
      try {
        FileStream fs =
            new FileStream(filename, FileMode.Open, file_access);
        fs.Close();
        return false;
      } catch (IOException) {
        return true;
      } catch (Exception) {
        throw;
      }
    }

    private static void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args) {
      var reader = DataReader.FromBuffer(args.CharacteristicValue);
      var flags = reader.ReadByte();
      var value = reader.ReadByte();
      Console.WriteLine($"{flags} - {value}");
    }

    private static void DeviceWatcher_Stopped(DeviceWatcher sender, object args) {
      // throw new NotImplementedException();
    }

    private static void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args) {
      //throw new NotImplementedException();
    }

    private static void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args) {
      //throw new NotImplementedException();
    }

    private static void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args) {
      //throw new NotImplementedException();
    }


    public static void SendData(string ipAddress, string message) {
      UdpClient udpClient = new UdpClient();

      try {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, ipAddress, Port);
        Console.WriteLine("Message sent successfully.");
      } catch (Exception e) {
        Console.WriteLine("Error sending message: " + e.Message);
      } finally {
        udpClient.Close();
      }
    }

    private static void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args) {
      Console.WriteLine(args.Name);
      if (args.Name == "RIZER")
        device = args;

    }


  }
}

