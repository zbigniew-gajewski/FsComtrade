
namespace FsComtrade.Lib.Types

module TypesModule = 

    type  FilePath = 
       | FullFilePath of string 
       | DirectoryAndFileName of string * string

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

    type AnalogSample = {
        Number : int;
        TimeStamp : uint32
        SampleValue : float
    }

    type DigitalSample = {
        Number : int;
        TimeStamp : uint32
        SampleValue : Bit
    }

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
        Samples : AnalogSample array;
    } 

    type DigitalChannelInfo = {
        ChannelInfo : ChannelInfo;
        NormalState : NormalChannelState;
        Samples : DigitalSample array;
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
        AnalogNumberOfChannels : int;
        DigitalNumberOfChannels : int;
        AnalogChannels : AnalogChannelInfo []; 
        DigitalChannels : DigitalChannelInfo []; 
        NominalFrequencyHz : float;
        SamplingRates : SamplingRateInfo;
        FirstSampleTimeStamp : System.DateTime;
        TriggerPointTimeStamp : System.DateTime;
        FileType : FileType;
        MultiplicationFactor : float;
    }
