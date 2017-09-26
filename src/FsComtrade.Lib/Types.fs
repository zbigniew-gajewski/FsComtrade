
namespace FsComtrade.Lib.Types

module TypesModule = 

    // type  FilePath = 
    //    | FullFilePath of string 
    //    | DirectoryAndFileNameNoExtension of string * string

    let splitter = ','

    type RevisionYear = 
        | Year1991
        | Year1999

    type PhaseIdentifier = 
        | Primary
        | Secondary

    type NormalChannelState = 
        | One
        | Zero

    type Bit = 
        | Zero = 0b0
        | One = 0b1

    type ChannelInfo = {
        Index : int;
        Identifier : string;
        Phase : string Option;
        CircuitComponent : string Option;
    }

    type AnalogChannelInfo = {
        ChannelInfo : ChannelInfo;
        Unit : string;
        MultiplierA : float;
        OffsetAdderB : float;
        TimeSkew : float;
        MinDataValue : float;
        MaxDataValue : float;
        PrimaryFactor : float;
        SecondaryFactor : float;
        PrimarySecondaryIdentifier : PhaseIdentifier;
    } 

    type DigitalChannelInfo = {
        ChannelInfo : ChannelInfo;
        NormalState : NormalChannelState;
    }

    type SamplingRate = {
        SampleRateHz : float;
        LastSampleNumber : int; 
    }

    type SamplingRateInfo =
        | EmptySamplingRateInfo of int 
        | FullSamplingRateInfo of int * SamplingRate array

    type FileType = 
        | ASCII
        | BINARY
           
    type CfgFile = { 
        StationName : string;
        RecordingDeviceId : string;
        RevisionYear :  RevisionYear;
        TotalNumberOfChannels : int;
        NumberOfAnalogChannels : int;
        NumberOfDigitalChannels : int;
        AnalogChannels : AnalogChannelInfo array; 
        DigitalChannels : DigitalChannelInfo array; 
        NominalFrequencyHz : float;
        SamplingRates : SamplingRateInfo;
        FirstSampleTimeStamp : System.DateTime;
        TriggerPointTimeStamp : System.DateTime;
        FileType : FileType;
        MultiplicationFactor : float;
    }

    type SampleLine = {
        Number : int;
        TimeStamp : uint64;
        AnalogSampleValues : float array;
        DigitalSampleValues : Bit array;
    }

    type DatFile = {
        SampleLines : SampleLine array;
    }

    type ComtradeFile = {
        CfgFile : CfgFile;
        DatFile : DatFile;
    }
