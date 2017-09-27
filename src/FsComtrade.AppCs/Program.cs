using System;
using FsComtrade.Lib.Mappers;

namespace FsComtrade.AppCs
{
    class Program
    {
        static void Main(string[] args)
        {
            var comtradeFile = MappersModule.mapComtradeFile("..\\FsComtrade\\src\\Resources\\TestFiles", "TestFile01");

            Console.WriteLine(comtradeFile.CfgFile.StationName);
            Console.WriteLine(comtradeFile.CfgFile.RecordingDeviceId);
            Console.WriteLine(comtradeFile.CfgFile.RevisionYear);
            Console.WriteLine(comtradeFile.CfgFile.TotalNumberOfChannels);
            Console.WriteLine(comtradeFile.CfgFile.NumberOfAnalogChannels);
            Console.WriteLine(comtradeFile.CfgFile.NumberOfDigitalChannels);
            Console.WriteLine(comtradeFile.CfgFile.NominalFrequencyHz);
            Console.WriteLine(comtradeFile.CfgFile.FirstSampleTimeStamp);
            Console.WriteLine(comtradeFile.CfgFile.TriggerPointTimeStamp);
            Console.WriteLine(comtradeFile.CfgFile.FileType);
            Console.WriteLine(comtradeFile.CfgFile.MultiplicationFactor);
        }
    }
}
