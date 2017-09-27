namespace FsComtrade.Lib.Mappers

module MappersModule =

    open System
    open System.IO
    open FsComtrade.Lib.Types.TypesModule
       
    let getRevisionYear (revisionYearString : string) = 
        match  int revisionYearString with
        | 1991 -> RevisionYear.Year1991        
        | 1999 -> RevisionYear.Year1999
        | _ -> RevisionYear.Year1991 


    let mapFirstLine (firstLine : string) = 
        let firstLineSplitted = firstLine.Split(splitter) 
        let stationName = firstLineSplitted.[0]
        let recordingDeviceId = firstLineSplitted.[1]
        let revisionYear = getRevisionYear firstLineSplitted.[2]
        stationName, recordingDeviceId, revisionYear

    let mapSingleNumberOfChannels (numberOfChannelsString : string) = // e.g. "12A" or "3D"
        let stringLength = numberOfChannelsString.Length
        numberOfChannelsString.Substring(0, stringLength - 1) |> int

    let mapNumberOfChannels (numberOfChannelsString : string) = // e.g. "15,12A,3D"
        let numberOfChannelsLineSplitted = numberOfChannelsString.Split(splitter)
        let totalNumberOfChannels = numberOfChannelsLineSplitted.[0] |> int
        let numberOfAnalogChannels = numberOfChannelsLineSplitted.[1] |> mapSingleNumberOfChannels
        let numberOfDigitalChannels = numberOfChannelsLineSplitted.[2] |> mapSingleNumberOfChannels
        totalNumberOfChannels, numberOfAnalogChannels, numberOfDigitalChannels

    let mapPhase (phaseString : string) = 
        match phaseString with
        | "" -> None;
        | _ -> Some phaseString;

    let mapCircuitComponent (cuircitComponentString : string) = 
        match cuircitComponentString with
        | "" -> None;
        | _ -> Some cuircitComponentString
        
    let mapPrimarySecondaryIdentifier (primarySecondaryIdentifierString : string) = 
        match primarySecondaryIdentifierString with 
        | "P" -> PhaseIdentifier.Primary
        | "S" -> PhaseIdentifier.Secondary
        | _ -> PhaseIdentifier.Primary;

    let mapChannelInfo (channelInfoStringSplitted : string[]) = 
        {
            Index = channelInfoStringSplitted.[0] |> int;
            Identifier = channelInfoStringSplitted.[1];
            Phase = channelInfoStringSplitted.[2] |> mapPhase
            CircuitComponent = channelInfoStringSplitted.[3] |> mapCircuitComponent
        }

    let mapAnalogChannel (analogChannelInfoString : string) = 
        let analogChannelInfoStringSplitted = analogChannelInfoString.Split(splitter)
        {
            ChannelInfo = analogChannelInfoStringSplitted.[0..3] |> mapChannelInfo
            Unit = analogChannelInfoStringSplitted.[4] 
            MultiplierA = analogChannelInfoStringSplitted.[5] |> float
            OffsetAdderB = analogChannelInfoStringSplitted.[6] |> float 
            TimeSkew = analogChannelInfoStringSplitted.[7] |> float
            MinDataValue = analogChannelInfoStringSplitted.[8] |> float  
            MaxDataValue = analogChannelInfoStringSplitted.[9] |> float 
            PrimaryFactor = analogChannelInfoStringSplitted.[10] |> float 
            SecondaryFactor = analogChannelInfoStringSplitted.[11] |> float 
            PrimarySecondaryIdentifier = analogChannelInfoStringSplitted.[12] |> mapPrimarySecondaryIdentifier 
        }
    
    let mapNormalState (normalStateString : string) =
        match normalStateString with
            | "1" -> NormalChannelState.One
            | "0" -> NormalChannelState.Zero
            | _ -> NormalChannelState.One    

    let mapDigitalChannel (digitalChannelInfoString : string) = 
        let digitalChannelInfoStringSplitted = digitalChannelInfoString.Split(splitter)
        {
            ChannelInfo = digitalChannelInfoStringSplitted.[0..3] |> mapChannelInfo;
            NormalState = digitalChannelInfoStringSplitted.[4] |> mapNormalState;
        }

    let mapSamplingRates (samplingRateString : string) =
        let samplingRateSplitted = samplingRateString.Split(splitter)
        // result
        {
            SampleRateHz = float samplingRateSplitted.[0]
            LastSampleNumber = int samplingRateSplitted.[1]
        }

    let mapNanoseconds (secondsFractionString : string) = 
        match secondsFractionString.Length with
        | 3 -> int secondsFractionString * 1_000_000, TimePrecission.Milliseconds
        | 6 -> int secondsFractionString * 1_000, TimePrecission.Microseconds
        | 9 -> int secondsFractionString, TimePrecission.Nanoseconds
        | _ -> int (secondsFractionString.Substring(0,3)), TimePrecission.Milliseconds

    let mapDateTimeWithNanoseconds (dateTimeString : string, revisionYear : RevisionYear) = 
        
        let dateTimeStringSplitted = dateTimeString.Split(splitter)
        
        let dateSplitted = dateTimeStringSplitted.[0].Split('/')
        let day, month, year = 
            match revisionYear with
            | RevisionYear.Year1991 ->
                int dateSplitted.[1], int dateSplitted.[0], int ("19" + dateSplitted.[2])
            | _ ->
                int dateSplitted.[0], int dateSplitted.[1], int dateSplitted.[2]
        
        let timeSplitted = dateTimeStringSplitted.[1].Split(':')
        let hours = int timeSplitted.[0]
        let minutes = int timeSplitted.[1]
        
        let secondsSplitted = timeSplitted.[2].Split('.')
        let seconds = int secondsSplitted.[0]
        let nanoseconds, timePrecission = secondsSplitted.[1] |> mapNanoseconds

        // result tupple : DateTimeWithNanoseconds * SecondPrecission (for dat file)
        {
            DateTimeWithSeconds = DateTime(year, month, day, hours, minutes, seconds)
            Nanoseconds = nanoseconds
        }, timePrecission       

    let mapSamplingRateInfo (cfgFileLines : string[], numberOfSamplingRatesLineIndex : int, numberOfSamplingRates : int) =
        match numberOfSamplingRates with
        | 0 -> EmptySamplingRateInfo 0
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
        let stationName, recordingDeviceId, revisionYear = 
            cfgFileLines.[0] |> mapFirstLine

        // Number of channels (line 1)       
        let numberOfChannelsLineIndex = 1
        let totalNumberOfChannels, numberOfAnalogChannels, numberOfDigitalChannels = 
            cfgFileLines.[numberOfChannelsLineIndex] |> mapNumberOfChannels
        
        // Analog and digital channel line indexes
        let firstAnalogChannelLineIndex = 2
        let lastAnalogChannelLineIndex = firstAnalogChannelLineIndex + numberOfAnalogChannels - 1
        let firstDigitalChannelLineIndex = lastAnalogChannelLineIndex + 1
        let lastDigitalChannelLineIndex = firstDigitalChannelLineIndex + numberOfDigitalChannels - 1
        
        // analog channels
        let analogChannels = 
            match numberOfAnalogChannels with
            | 0 -> Array.empty<AnalogChannelInfo> // todo: should be None probably
            | _ -> 
                cfgFileLines.[firstAnalogChannelLineIndex..lastAnalogChannelLineIndex]                
                |> Array.map mapAnalogChannel

        // digital channels
        let digitalChannels = 
            match numberOfDigitalChannels with
            | 0 -> Array.empty<DigitalChannelInfo> // todo: should be None probably
            | _ -> 
                cfgFileLines.[firstDigitalChannelLineIndex..lastDigitalChannelLineIndex]
                |> Array.map mapDigitalChannel

        // Line frequency
        let nominalFrequencyHzLineIndex = lastDigitalChannelLineIndex + 1
        let nominalFrequencyHz = 
            float cfgFileLines.[nominalFrequencyHzLineIndex]

        // Line numberOfSamplingRates
        let numberOfSamplingRatesLineIndex = nominalFrequencyHzLineIndex + 1
        let numberOfSamplingRates = 
            int cfgFileLines.[numberOfSamplingRatesLineIndex]
        
        let samplingRateInfo =  
            (cfgFileLines, numberOfSamplingRatesLineIndex, numberOfSamplingRates)
            |> mapSamplingRateInfo 
        
        // Time Stamps
        let firstSampleTimeStampLineIndex = numberOfSamplingRatesLineIndex + numberOfSamplingRates + 1

        let firstSampleTimeStamp, timePrecission =
            (cfgFileLines.[firstSampleTimeStampLineIndex], revisionYear)
            |> mapDateTimeWithNanoseconds

        let triggerPointTimeStampLineIndex = firstSampleTimeStampLineIndex + 1
        
        let triggerPointTimeStamp, timePrecission =
            (cfgFileLines.[triggerPointTimeStampLineIndex], revisionYear) 
            |> mapDateTimeWithNanoseconds

        // File type
        let fileTypeLineIndex = triggerPointTimeStampLineIndex + 1
        
        let fileType = 
            cfgFileLines.[fileTypeLineIndex] 
            |> mapFileType

        // Multiplication factor
        let multiplicationFactorLineIndex = fileTypeLineIndex + 1
        
        let multiplicationFactor = 
            float cfgFileLines.[multiplicationFactorLineIndex]

        // result : CfgFile
        { 
            StationName = stationName 
            RecordingDeviceId = recordingDeviceId 
            RevisionYear = revisionYear
            TotalNumberOfChannels = totalNumberOfChannels
            NumberOfAnalogChannels = numberOfAnalogChannels
            NumberOfDigitalChannels = numberOfDigitalChannels
            AnalogChannels = analogChannels
            DigitalChannels = digitalChannels
            NominalFrequencyHz = nominalFrequencyHz
            SamplingRates = samplingRateInfo
            TimePrecission = timePrecission
            FirstSampleTimeStamp = firstSampleTimeStamp
            TriggerPointTimeStamp = triggerPointTimeStamp
            FileType = fileType
            MultiplicationFactor = multiplicationFactor
        }        

    // .dat file
    let mapDatFileLine (fileLine : string, numberOfAnalogChannels : int, numberOfDigitalChannels : int ) = 
        let fileLineSplitted = fileLine.Split(splitter)
        let firstAnalogChannelColumnIndex = 2
        let lastAnalogChannelColumnIndex = firstAnalogChannelColumnIndex + numberOfAnalogChannels - 1
        let firstDigitalChannelColumnIndex = lastAnalogChannelColumnIndex + 1
        let lastDigitalChannelColumnIndex = firstDigitalChannelColumnIndex + numberOfDigitalChannels - 1

        let mapBit (bitString : string) = 
            match bitString with
            | "0" -> Bit.Zero
            | "1" -> Bit.One
            | _ -> Bit.Zero

        // result: SampleLine
        {
            Number = fileLineSplitted.[0] |> int;
            TimeStamp = fileLineSplitted.[1] |> uint64;
            AnalogSampleValues = fileLineSplitted.[firstAnalogChannelColumnIndex..lastAnalogChannelColumnIndex] 
                |>  Array.map float;
            DigitalSampleValues = fileLineSplitted.[firstDigitalChannelColumnIndex..lastDigitalChannelColumnIndex] 
                |>  Array.map mapBit;
        }

    let mapAsciiDatFile (datFileLines : string [], numberOfAnalogChannels : int, numberOfDigitalChannels : int) = 
        let mapLineFn = fun line -> mapDatFileLine (line, numberOfAnalogChannels, numberOfDigitalChannels)
        let sampleLines = datFileLines |> Array.map mapLineFn
        // result : SampleLines
        { 
            SampleLines = sampleLines
        }
        
    let mapBinaryLines (binaryDatFile : byte[], numberOfAnalogChannels : int, numberOfDigitalChannels : int) =
        Array.empty<byte[]> // todo: divide binaryDatFile into lines of bytes - each byteLine = one sample

    let mapBinaryDatFile (datFileBinaryArray : byte [], numberOfAnalogChannels : int, numberOfDigitalChannels : int) = 
        //todo: convert byte[] to Ascii lines and then map using mapAsciiDatFile
        // result : SampleLines
        { 
            SampleLines = Array.empty 
        }

    let mapComtradeFile (directory : string, fileNameWithoutExtension : string) = 
        let filePathNoExtension = Path.Combine (directory, fileNameWithoutExtension)
        let cfgFilePath = filePathNoExtension + ".cfg" 
        let datFilePath = filePathNoExtension + ".dat" 
        let cfgFile = File.ReadAllLines(cfgFilePath) |> mapCfgFile
        let numberOfAnalogChannels = cfgFile.NumberOfAnalogChannels
        let numberOfDigitalChannels = cfgFile.NumberOfDigitalChannels
        let datFile = 
            match cfgFile.FileType with
            | FileType.ASCII -> 
                let asciiFile = File.ReadAllLines(datFilePath)
                mapAsciiDatFile (asciiFile, numberOfAnalogChannels, numberOfDigitalChannels)
            | FileType.BINARY -> 
                let binaryFile = File.ReadAllBytes(datFilePath)
                mapBinaryDatFile (binaryFile, numberOfAnalogChannels, numberOfDigitalChannels)
        // result : ComtradeFile
        {
            CfgFile = cfgFile
            DatFile = datFile
        }