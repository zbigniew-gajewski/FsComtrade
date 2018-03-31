namespace Comtrade.LibFs.Types

open System 
  
module TypesModule = 

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
        | Zero = 0
        | One = 1

    type ChannelInfo = {
        Index : int
        Identifier : string
        Phase : string Option
        CircuitComponent : string Option
    }

    type AnalogChannelInfo = {
        ChannelInfo : ChannelInfo
        Unit : string
        MultiplierA : float
        OffsetAdderB : float
        TimeSkew : float
        MinDataValue : float
        MaxDataValue : float
        PrimaryFactor : float
        SecondaryFactor : float
        PrimarySecondaryIdentifier : PhaseIdentifier
    }

    type DigitalChannelInfo = {
        ChannelInfo : ChannelInfo
        NormalState : NormalChannelState
    }

    type SamplingRate = {
        SampleRateHz : float
        LastSampleNumber : int
    }

    type SamplingRateInfo = 
        | EmptySamplingRateInfo of  int
        | FullSamplingRateInfo of int * SamplingRate array

    type FileType = 
        | ASCII
        | BINARY

    type DateTimeWithNanoseconds = {
        DateTimeWithSeconds : DateTime
        Nanoseconds : int
    }

    type TimePrcission = 
        | Milliseconds
        | Microseconds
        | Nanoseconds

    type CfgFile = {
        StationName : string
        RecordingDeviceId : string
        RevisionYear : RevisionYear
        TotalNumberOfChannels : int
        NumberOfAnalogChannels : int
        NumberOfDigitalChannels : int
        AnalogChannels : AnalogChannelInfo array
        DigitalChannels : DigitalChannelInfo array
        NominalFrequencyHz : float
        SamplingRates : SamplingRateInfo
        TimePrecission : TimePrcission
        FirstSampleTimeStamp : DateTimeWithNanoseconds
        TriggerPointTimeStamp : DateTimeWithNanoseconds
        FileType : FileType
        MultiplicationFactor : float
    }

    type SampleLine = {
        Number : int
        TimeStamp : uint64
        AnalogSampleValues : float array
        DigitalSampleValues : Bit array
    }

    type DatFile = {
        SampleLines : SampleLine array
    }

    type ComtradeFile = {
        CfgFile : CfgFile
        DatFile : DatFile
    }


