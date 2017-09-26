open System
open FsComtrade.Lib
open FsComtrade.Lib.Types.TypesModule
open FsComtrade.Lib.Mappers.MappersModule

[<EntryPoint>]
let main argv =
    let comtradeFile = 
        ("..\FsComtrade\src\Resources\TestFiles", "TestFile01")
        |> mapComtradeFile
      
    printfn "%s\n %s\n %s\n %s\n %s\n %s\n %s\n %s\n %s\n %s\n %s\n" 
        comtradeFile.CfgFile.StationName 
        comtradeFile.CfgFile.RecordingDeviceId 
        (comtradeFile.CfgFile.RevisionYear.ToString())
        (comtradeFile.CfgFile.TotalNumberOfChannels.ToString()) 
        (comtradeFile.CfgFile.NumberOfAnalogChannels.ToString()) 
        (comtradeFile.CfgFile.NumberOfDigitalChannels.ToString())
        (comtradeFile.CfgFile.NominalFrequencyHz.ToString())
        (comtradeFile.CfgFile.FirstSampleTimeStamp.ToString())
        (comtradeFile.CfgFile.TriggerPointTimeStamp.ToString())
        (comtradeFile.CfgFile.FileType.ToString())
        (comtradeFile.CfgFile.MultiplicationFactor.ToString())
    0 // return an integer exit code
