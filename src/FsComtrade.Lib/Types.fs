
namespace FsComtrade.Lib.Types

module TypesModule = 

    type  FilePath = 
       | FullFilePath of string 
       | DirectoryAndFileName of string * string

    let splitter = ','

    type RevisionYear = 
        | Year1991 = 1991
        | Year1999 = 1999

    type PhaseIdentifier = 
        | Primary = 'P'
        | Secondary = 'S'

    type NormalChannelState = 
        | One = 1
        | Zero = 0

    type AnalogChannelInfo = {
        Index : int;
        Identifier : string;
        Phase : string Option;
        CircuitComponent : string Option;
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
        Index : int;
        Identifier : string;
        Phase : string Option;
        CircuitComponent : string Option;
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
