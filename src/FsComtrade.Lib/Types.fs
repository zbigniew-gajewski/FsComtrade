
namespace FsComtrade.Lib.Types

module TypesModule = 

    type  FilePath = 
       | FullFilePath of string //FullFilePath
       | DirectoryAndFileName of string * string // FileDirectoryWithFileName

    let splitter = ','

    type RevisionYear = 
        | Year1991 = 1991
        | Year1999 = 1999
        | Year2001 = 2001
        | Year2013 = 2013

    type PhaseIdentifier = 
        | Primary = 'P'
        | Secondary = 'S'

    type NormalChannelState = 
        | One = 1
        | Zero = 0

    type AnalogChannelInfo = {
        Index : int; // 1-999999
        Identifier : string; // 1-128 characters
        Phase : string Option; // 0-2 characters
        CircuitComponent : string Option; // 0-64 characters
        Unit : string; // 1-32 characters
        MultiplierA : float; //1-32 characters
        OffsetAdderB : float; //1-32 characters 
        TimeSkew : float; // 1-32 characters
        MinDataValue : float; //1-13 characters (-3.4028235E38 to 3.4028235E38)
        MaxDataValue : float; //1-13 characters (-3.4028235E38 to 3.4028235E38)
        PrimaryFactor : float; //1-32 characters
        SecondaryFactor : float; //1-32 characters
        PrimarySecondaryIdentifier : PhaseIdentifier; //'P' or 'S'
    }

    type DigitalChannelInfo = {
        Index : int; // 1-999999
        Identifier : string; // 1-128 characters
        Phase : string Option; // 0-2 characters
        CircuitComponent : string Option; // 0-64 characters
        NormalState : NormalChannelState; // Normal (1) or Abnormal (0)
    }

    type SamplingRate = {
        SampleRateHz : float;
        LastSampleNumber : int; 
    }

    type SamplingRateInfo =
        | EmptySamplingRateInfo of int 
        | FullSamplingRateInfo of int * SamplingRate array // exists when NumberOfSamplingRates > 0

    type FileType = 
        | ASCII
        | BINARY
        | BINARY32
        | FLOAT32
           
    type CfgFile = { 
        StationName : string; // 0-64 characters
        RecordingDeviceId : string; // 0-64 characters
        RevisionYear :  RevisionYear; // 1991, 1999, 2013
        TotalNumberOfChannels : int; // 1-999999
        AnalogNumberOfChannels : int; // 1-999999
        DigitalNumberOfChannels : int; // 1-999999; 
        AnalogChannels : AnalogChannelInfo []; 
        DigitalChannels : DigitalChannelInfo []; 
        NominalFrequencyHz : float; //50, 60, 16.7 
        SamplingRates : SamplingRateInfo; // only nrates or nrates with an array of sampling rates
        FirstSampleTimeStamp : System.DateTime;
        TriggerPointTimeStamp : System.DateTime;
        FileType : FileType;
        MultiplicationFactor : float;
    }
