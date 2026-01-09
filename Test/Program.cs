// See https://aka.ms/new-console-template for more information
using Hygrometer;

Console.WriteLine("Hello, World!");
IHygrometer hygrometer = new Hygrometer.Hygrometer(new Communication.Bus.PhysicalPort.SerialPort("COM3", 38400));
await hygrometer.OpenAsync();
var rs = await hygrometer.Read("01");

Console.ReadLine();