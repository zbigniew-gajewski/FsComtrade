namespace FsComtrade.Lib.Mappers

module CfgModule =

    open System
    open System.IO
    open FsComtrade.Lib.Types.TypesModule

    let getCfgFileLines (fileSource : FilePath) =
        match fileSource with
        | FullFilePath (fullFilePath) -> 
            File.ReadAllLines fullFilePath
        | DirectoryAndFileName (fileDirectory, fileName) -> 
            File.ReadAllLines (Path.Combine (fileDirectory, fileName)) 
        
    let getRevisionYear (revisionYearString : string) = 
        match  int revisionYearString with
        | 1991 -> RevisionYear.Year1991        
        | 1999 -> RevisionYear.Year1999
        | 2001 -> RevisionYear.Year2001
        | 2013 -> RevisionYear.Year2013
        | _ -> RevisionYear.Year1991 

    let numberOfChannelsFromString (numberOfAnalogChannelsString : string) = // e.g. "12A"  or "3D"
        let stringLength = numberOfAnalogChannelsString.Length
        numberOfAnalogChannelsString.Substring(0, stringLength - 1) 
        |> int
    
    let mapPhase (analogChannelInfoString : string) = 
        match analogChannelInfoString with
        | "" -> None;
        | _ -> Some analogChannelInfoString;

    let mapCircuitComponent (cuircitComponentString : string) = 
        match cuircitComponentString with
        | "" -> None;
        | _ -> Some cuircitComponentString
        
    let mapPrimarySecondaryIdentifier (primarySecondaryIdentifierString : string) = 
        match primarySecondaryIdentifierString with 
        | "P" -> PhaseIdentifier.Primary
        | "S" -> PhaseIdentifier.Secondary
        | _ -> PhaseIdentifier.Primary;

    let mapAnalogChannel (analogChannelInfoString : string) = 
        let analogChannelInfoStringSplitted = analogChannelInfoString.Split(splitter)
        {
            Index  = analogChannelInfoStringSplitted.[0] |> int;
            Identifier  = analogChannelInfoStringSplitted.[1];
            Phase = analogChannelInfoStringSplitted.[2] |> mapPhase
            CircuitComponent = analogChannelInfoStringSplitted.[3] |> mapCircuitComponent
            Unit = analogChannelInfoStringSplitted.[4]; 
            MultiplierA = analogChannelInfoStringSplitted.[5] |> float;
            OffsetAdderB = analogChannelInfoStringSplitted.[6] |> float; 
            TimeSkew = analogChannelInfoStringSplitted.[7] |> float; 
            MinDataValue = analogChannelInfoStringSplitted.[8] |> float;  
            MaxDataValue = analogChannelInfoStringSplitted.[9] |> float; 
            PrimaryFactor = analogChannelInfoStringSplitted.[10] |> float; 
            SecondaryFactor = analogChannelInfoStringSplitted.[11] |> float; 
            PrimarySecondaryIdentifier = analogChannelInfoStringSplitted.[12] |> mapPrimarySecondaryIdentifier; 
        }
    
    let mapNormalState (normalStateString : string) =
        match normalStateString with
            | "1" -> NormalChannelState.One
            | "0" -> NormalChannelState.Zero
            | _ -> NormalChannelState.One    

    let mapDigitalChannel (digitalChannelInfoString : string) = 
        let digitalChannelInfoStringSplitted = digitalChannelInfoString.Split(splitter)
        {
            Index = digitalChannelInfoStringSplitted.[0] |> int;
            Identifier = digitalChannelInfoStringSplitted.[1];
            Phase = digitalChannelInfoStringSplitted.[2] |> mapPhase
            CircuitComponent = digitalChannelInfoStringSplitted.[3] |> mapCircuitComponent
            NormalState = digitalChannelInfoStringSplitted.[4] |> mapNormalState
        }

    let mapSamplingRates (samplingRateString : string) =
        let samplingRateSplitted = samplingRateString.Split(splitter)
        // result
        {
            SampleRateHz = samplingRateSplitted.[0] |> float
            LastSampleNumber = samplingRateSplitted.[1] |> int
        }

    let mapSecondsFraction (secondsFractionString : string) = 
        match secondsFractionString.Length with
        | 3 -> int secondsFractionString 
        | 6 -> int secondsFractionString / 1000
        | 9 -> int secondsFractionString / 1000_000
        | _ -> int (secondsFractionString.Substring(0,3))

    let mapDateTime (dateTimeString : string, revisionYear : RevisionYear) = 
        
        let dateTimeStringSplitted = dateTimeString.Split(splitter)
        
        let dateSplitted = dateTimeStringSplitted.[0].Split('/')
        let (day, month, year) = 
            if revisionYear = RevisionYear.Year1991 then
                (int dateSplitted.[1], int dateSplitted.[0], int ("19" + dateSplitted.[2])) 
            else
                (int dateSplitted.[0], int dateSplitted.[1], int dateSplitted.[2]) 
        
        let timeSplitted = dateTimeStringSplitted.[1].Split(':')
        let hours = int timeSplitted.[0]
        let minutes = int timeSplitted.[1]
        
        let secondsSplitted = timeSplitted.[2].Split('.')
        let seconds = int secondsSplitted.[0]
        let milliseconds = secondsSplitted.[1] |> mapSecondsFraction        

        // result DateTime
        DateTime(year, month, day, hours, minutes, seconds, milliseconds)    

    let mapSamplingRateInfo (cfgFileLines : string[], numberOfSamplingRatesLineIndex : int, numberOfSamplingRates : int) =
        match numberOfSamplingRates with
        | 0 -> 
            EmptySamplingRateInfo 0
        | _ -> 
            let samplingRates = 
                let firstSamplingRateLineIndex = numberOfSamplingRatesLineIndex + 1
                let lastSamplingRateLineIndex = numberOfSamplingRatesLineIndex + numberOfSamplingRates
                cfgFileLines.[firstSamplingRateLineIndex..lastSamplingRateLineIndex]
                |> Array.map mapSamplingRates
            FullSamplingRateInfo (numberOfSamplingRates, samplingRates)

    let mapFileType (fileTypeString : string) = 
        let fileTypeStringUpper = fileTypeString.ToUpper()
        match fileTypeStringUpper with 
        | "ASCII" -> FileType.ASCII
        | "BINARY" -> FileType.BINARY
        | _ -> FileType.ASCII

    let mapCfgFile (cfgFileLines : string []) = 

        // Station name, Id, revision year (line 0)
        let firstLine = cfgFileLines.[0]
        let firstLineSplitted = firstLine.Split(splitter) 

        let stationName = firstLineSplitted.[0]
        let recordingDeviceId = firstLineSplitted.[1]
        let revisionYear = getRevisionYear firstLineSplitted.[2]

        // Number of channels (line 1)
        let numberOfChannelsLine = cfgFileLines.[1]
        let numberOfChannelsLineSplitted = numberOfChannelsLine.Split(splitter)

        let totalNumberOfChannels = int numberOfChannelsLineSplitted.[0]
        let analogChannelsNumber = numberOfChannelsLineSplitted.[1] |> numberOfChannelsFromString
        let digitalChannelsNumber = numberOfChannelsLineSplitted.[2] |> numberOfChannelsFromString

        // Analog and digital channel line indexes
        let firstAnalogChannelLineIndex = 2
        let lastAnalogChannelLineIndex = firstAnalogChannelLineIndex + analogChannelsNumber - 1

        let firstDigitalChannelLineIndex = lastAnalogChannelLineIndex + 1
        let lastDigitalChannelLineIndex = firstDigitalChannelLineIndex + digitalChannelsNumber - 1
        
        // analog channels
        let analogChannels = 
            cfgFileLines.[firstAnalogChannelLineIndex..lastAnalogChannelLineIndex] // great feature [fromIndex..toIndex]
            |> Array.map mapAnalogChannel

        // digital channels
        let digitalChannels = 
            cfgFileLines.[firstDigitalChannelLineIndex..lastDigitalChannelLineIndex]
            |> Array.map mapDigitalChannel

        // Line frequency
        let nominalFrequencyHzLineIndex = lastDigitalChannelLineIndex + 1
        let nominalFrequencyHz = 
            cfgFileLines.[nominalFrequencyHzLineIndex]
            |> float

        // Line numberOfSamplingRates
        let numberOfSamplingRatesLineIndex = nominalFrequencyHzLineIndex + 1
        let numberOfSamplingRates = 
            cfgFileLines.[numberOfSamplingRatesLineIndex]
            |> int
        
        let samplingRateInfo =  
            (cfgFileLines, numberOfSamplingRatesLineIndex, numberOfSamplingRates)
            |> mapSamplingRateInfo 
        
        // Time Stamps
        let firstSampleTimeStampLineIndex = numberOfSamplingRatesLineIndex + numberOfSamplingRates + 1

        let firstSampleTimeStamp =
            (cfgFileLines.[firstSampleTimeStampLineIndex], revisionYear)
            |> mapDateTime

        let triggerPointTimeStampLineIndex = firstSampleTimeStampLineIndex + 1
        
        let triggerPointTimeStamp =
            (cfgFileLines.[triggerPointTimeStampLineIndex], revisionYear) 
            |> mapDateTime

        // File type
        let fileTypeLineIndex = triggerPointTimeStampLineIndex + 1
        
        let fileType = 
            cfgFileLines.[fileTypeLineIndex] 
            |> mapFileType

        // Multiplication factor
        let multiplicationFactorLineIndex = fileTypeLineIndex + 1
        
        let multiplicationFactor = 
            cfgFileLines.[multiplicationFactorLineIndex]
            |> float

        // result : CfgFile
        { 
            StationName = stationName; 
            RecordingDeviceId = recordingDeviceId; 
            RevisionYear = revisionYear;
            TotalNumberOfChannels = totalNumberOfChannels;
            AnalogNumberOfChannels = analogChannelsNumber;
            DigitalNumberOfChannels = digitalChannelsNumber;
            AnalogChannels = analogChannels;
            DigitalChannels = digitalChannels;
            NominalFrequencyHz = nominalFrequencyHz;
            SamplingRates = samplingRateInfo;
            FirstSampleTimeStamp = firstSampleTimeStamp;
            TriggerPointTimeStamp = triggerPointTimeStamp;
            FileType = fileType;
            MultiplicationFactor = multiplicationFactor
        }        
        
       