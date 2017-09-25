open System
open FsComtrade.Lib
open FsComtrade.Lib.Types.TypesModule
open FsComtrade.Lib.Mappers.CfgModule

[<EntryPoint>]
let main argv =
    let cfgFile = 
        (FullFilePath "..\FsComtrade\src\Resources\TestFiles\Test.cfg")
        |> getCfgFileLines
        |> mapCfgFile
      
    printfn "%s\n %s\n %s\n %s\n %s\n %s\n %s\n %s\n %s\n %s\n %s\n" 
        cfgFile.StationName 
        cfgFile.RecordingDeviceId 
        (cfgFile.RevisionYear.ToString())
        (cfgFile.TotalNumberOfChannels.ToString()) 
        (cfgFile.AnalogNumberOfChannels.ToString()) 
        (cfgFile.DigitalNumberOfChannels.ToString())
        (cfgFile.NominalFrequencyHz.ToString())
        (cfgFile.FirstSampleTimeStamp.ToString())
        (cfgFile.TriggerPointTimeStamp.ToString())
        (cfgFile.FileType.ToString())
        (cfgFile.MultiplicationFactor.ToString())
    0 // return an integer exit code
