namespace Comtrade.LibFs.Mappers

open System
open Comtrade.LibFs.Types.TypesModule
open System.IO    

module MappersModule =

    let hello name =
        printfn "Hello %s" name

    let getRevisionYear (revisionYearString : string) =
        let revisionYearint = int revisionYearString
        match revisionYearint with
        | 1991 -> RevisionYear.Year1991
        | 1999 -> RevisionYear.Year1999
        | _ -> RevisionYear.Year1999

    let mapFirstLine (firstLine : string) = 
        let firstLineSplitted = firstLine.Split(splitter)
        let stationName = firstLineSplitted.[0]
        let recordingDeviceId = firstLineSplitted.[1]
        let revisionYear = getRevisionYear firstLineSplitted.[2]
        stationName, recordingDeviceId, revisionYear

    let mapSingleNumberOfChannels (numberOfChannelsString : string ) = 
        let stringLength = numberOfChannelsString.Length
        numberOfChannelsString.Substring(0, stringLength-1) |> int

    let mapNumberOfChannels (numberOfChannelsString : string) = 
        let numberOfChannelsLineSplitted = numberOfChannelsString.Split(splitter)
        let totalNumberOfChannels = numberOfChannelsLineSplitted.[0] |> int
        let numberOfAnalogChannels = numberOfChannelsLineSplitted.[1] |> mapSingleNumberOfChannels
        let numberOfDigitalChannels = numberOfChannelsLineSplitted.[2] |> mapSingleNumberOfChannels
        totalNumberOfChannels, numberOfAnalogChannels, numberOfDigitalChannels

    let mapPhase (phaseString : string) = 
        match phaseString with
        | "" -> None
        | _ -> Some phaseString

    let mapCircuitComponent (circuitComponentString : string) = 
        match circuitComponentString with
        | "" -> None
        | _ -> Some circuitComponentString

    let mapPrimarySecondaryIdentifier ( primarySecondaryIdentifierString : string) =
        match primarySecondaryIdentifierString with
        | "P" -> PhaseIdentifier.Primary
        | "S" -> PhaseIdentifier.Secondary
        | _ -> PhaseIdentifier.Primary

    let mapChannelInfo (channelInfoStringSplitted : string[]) = 
        {
            Index = channelInfoStringSplitted.[0] |> int
            Identifier = channelInfoStringSplitted.[1]
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
        let digitalChannelInfoSplitted = digitalChannelInfoString.Split(splitter)
        {
            ChannelInfo = digitalChannelInfoSplitted.[0..3] |> mapChannelInfo
            NormalState = digitalChannelInfoSplitted.[4] |> mapNormalState
        }

    let mapSamplingRates (samplingRateString : string) = 
        let samplingRateSplitted = samplingRateString.Split(splitter)
        {
            SampleRateHz = samplingRateSplitted.[0] |> float
            LastSampleNumber = samplingRateSplitted.[1] |> int
        }

    let mapNanoseconds (secondsFractionString : string) = 
        match secondsFractionString.Length with
        | 3 -> int secondsFractionString * 1_000_000, TimePrcission.Milliseconds
        | 6 -> int secondsFractionString * 1_000, TimePrcission.Microseconds
        | 9 -> int secondsFractionString, TimePrcission.Nanoseconds
        | _ -> int (secondsFractionString.Substring(0,3)), TimePrcission.Milliseconds

    let mapDateTimeWithNanoseconds (dateTimeString : string, revisionYear : RevisionYear) = 
        let dateTimeStringSplitted = dateTimeString.Split(splitter)
        let dateSplitted = dateTimeStringSplitted.[0].Split('/')
        let day, month, year =
            match revisionYear with
            | RevisionYear.Year1991 ->
                int dateSplitted.[1], int dateSplitted.[0], int ("19" + dateSplitted.[2])
            | _ -> 
                int dateSplitted.[0], int dateSplitted.[1], int dateSplitted.[2]
        let timeSplitteed = dateTimeStringSplitted.[1].Split(':')
        let hours = int timeSplitteed.[0]
        let minutes = int timeSplitteed.[1]

        let secondsSplitted = timeSplitteed.[2].Split('.')
        let seconds = int secondsSplitted.[0]
        let nanoseconds, timePrecission = secondsSplitted.[1] |> mapNanoseconds

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
        let stationName, recordingDeviceId, revisionYear = 
            mapFirstLine cfgFileLines.[0]
        
        let numberOfChannelsLineIndex = 1
        let totalNumberOfChannels, numberOfAnalogChannels, numberOfDigitalChannels =
            cfgFileLines.[numberOfChannelsLineIndex] 
            |> mapNumberOfChannels

        let firstAnalogChannelLineIndex = 2
        let lastAnalogChannelLineIndex = firstAnalogChannelLineIndex + numberOfAnalogChannels - 1
        let firstDigitalChannelLineIndex = lastAnalogChannelLineIndex + 1
        let lastDigitalChannelLineIndex = firstDigitalChannelLineIndex + numberOfDigitalChannels - 1

        let analogChannels = 
            match numberOfAnalogChannels with
            | 0 -> Array.empty<AnalogChannelInfo>
            | _ -> 
                cfgFileLines.[firstAnalogChannelLineIndex..lastAnalogChannelLineIndex]
                |> Array.map mapAnalogChannel

        let digitalChannels = 
            match numberOfDigitalChannels with
            | 0 -> Array.empty<DigitalChannelInfo>
            | _ -> 
                cfgFileLines.[firstDigitalChannelLineIndex..lastDigitalChannelLineIndex]
                |> Array.map mapDigitalChannel

        let nominalFrequencyHzLineIndex = lastDigitalChannelLineIndex + 1
        let nominalFrequencyHz = 
            float cfgFileLines.[nominalFrequencyHzLineIndex]

        let numberOfSamplingRatesLineIndex = nominalFrequencyHzLineIndex + 1
        let numberOfSamplingRates = 
            int cfgFileLines.[numberOfSamplingRatesLineIndex]

        let samplingRateInfo = 
            (cfgFileLines, numberOfSamplingRatesLineIndex, numberOfSamplingRates)
            |> mapSamplingRateInfo

        let firstSampleTimeStampLineIndex = numberOfSamplingRatesLineIndex + numberOfSamplingRates + 1

        let firstSampletimeStamp, timePrecission = 
            (cfgFileLines.[firstSampleTimeStampLineIndex], revisionYear)
            |> mapDateTimeWithNanoseconds

        let triggerPointTimeStampLineIndex = firstSampleTimeStampLineIndex + 1
        let trigerPointTimeStamp, timePrecission = 
            (cfgFileLines.[triggerPointTimeStampLineIndex], revisionYear)
            |> mapDateTimeWithNanoseconds

        let fileTypeLineIndex = triggerPointTimeStampLineIndex + 1
        let fileType = 
            cfgFileLines.[fileTypeLineIndex]
            |> mapFileType

        let multiplicationFactorLineIndex = fileTypeLineIndex + 1        

        let multiplicationFactor = 
            cfgFileLines.[multiplicationFactorLineIndex] |> float

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
            FirstSampleTimeStamp = firstSampletimeStamp
            TriggerPointTimeStamp = trigerPointTimeStamp
            FileType = fileType
            MultiplicationFactor = multiplicationFactor
        }


    // ================================================================================

    let mapDatFileLine (fileLine : string, numberOfAnalogChannels : int, numberOfDigitalChannels : int) = 
        let fileLineSplitted = fileLine.Split(splitter)
        let firstAnalogChannelColumnLineIndex = 2
        let lastAnalogChannelColumnLineIndex = firstAnalogChannelColumnLineIndex + numberOfAnalogChannels - 1
        let firstDigitalChannelColumnLineIndex = lastAnalogChannelColumnLineIndex + 1
        let lastDigitalChannelColumnLineIndex = firstDigitalChannelColumnLineIndex + numberOfDigitalChannels - 1

        let mapBit (bitString : string) = 
            match bitString with
            | "0" -> Bit.Zero
            | "1" -> Bit.One
            | _ -> Bit.Zero

        {
            Number =  fileLineSplitted.[0] |> int;
            TimeStamp = fileLineSplitted.[1] |> uint64
            AnalogSampleValues = fileLineSplitted.[firstAnalogChannelColumnLineIndex..lastAnalogChannelColumnLineIndex]
                |> Array.map float
            DigitalSampleValues = fileLineSplitted.[firstDigitalChannelColumnLineIndex.. lastDigitalChannelColumnLineIndex]
                |> Array.map mapBit
        }

    // Variant 1 : tupples as arguments    

    // let mapAsciiDatFile (datFileLines : string [], numberOfAnalogChannels : int, numberOfDigitalChannels : int) = 
    //     let mapLineFn = fun line -> mapDatFileLine (line, numberOfAnalogChannels, numberOfDigitalChannels)
    //     let sampleLines = datFileLines |> Array.map mapLineFn
    //     {
    //         SampleLines = sampleLines
    //     }

    // let mapBinaryLines (binarydatFile : byte[], numberOfAnalogChannels : int, numberOfDigitalChannels : int) = 
    //     Array.empty<byte[]>

    // let mapBinaryDatFile (datFileBinaryArray : byte[], numberOfAnalogChannels : int, numberOfDigitalChannels : int) = 
    //     {
    //         SampleLines = Array.empty
    //     }

    // let mapComtradeFile (directory : string, fileNameWithoutExtension : string) = 
    //     let filePathNoExtension = Path.Combine (directory, fileNameWithoutExtension)
    //     let cfgFilePath = filePathNoExtension + ".cfg"
    //     let datFilePath = filePathNoExtension + ".dat"
    //     let cfgFile = File.ReadAllLines(cfgFilePath) |> mapCfgFile
    //     let numberOfAnalogChannels = cfgFile.NumberOfAnalogChannels
    //     let numberOfDigitalChannels = cfgFile.NumberOfDigitalChannels
    //     let datFile = 
    //         match cfgFile.FileType with
    //         | FileType.ASCII ->
    //             let asciiFile = File.ReadAllLines(datFilePath)
    //             (asciiFile, numberOfAnalogChannels, numberOfDigitalChannels) |> mapAsciiDatFile

    //         | FileType.BINARY ->
    //             let binaryFile = File.ReadAllBytes(datFilePath)
    //             (binaryFile, numberOfAnalogChannels, numberOfDigitalChannels) |> mapBinaryDatFile
    //     {
    //         CfgFile = cfgFile
    //         DatFile = datFile
    //     }


    // Variant 2 : standard arguments    
    let mapAsciiDatFile (numberOfAnalogChannels : int) (numberOfDigitalChannels : int) (datFileLines : string []) = 
        let mapLineFn = fun line -> mapDatFileLine (line, numberOfAnalogChannels, numberOfDigitalChannels)
        let sampleLines = datFileLines |> Array.map mapLineFn
        // result : SampleLines
        { 
            SampleLines = sampleLines
        }
        
    let mapBinaryLines (numberOfAnalogChannels : int) (numberOfDigitalChannels : int) (binaryDatFile : byte[]) =
        Array.empty<byte[]> // todo: divide binaryDatFile into lines of bytes - each byteLine = one sample

    let mapBinaryDatFile (numberOfAnalogChannels : int) (numberOfDigitalChannels : int) (datFileBinaryArray : byte []) = 
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
                File.ReadAllLines(datFilePath)
                |> mapAsciiDatFile numberOfAnalogChannels numberOfDigitalChannels
            | FileType.BINARY -> 
                File.ReadAllBytes(datFilePath)
                |> mapBinaryDatFile numberOfAnalogChannels numberOfDigitalChannels
        // result : ComtradeFile
        {
            CfgFile = cfgFile
            DatFile = datFile
        }
