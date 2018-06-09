module Core

open System
open Constants
open Utils
open BMS
open GDS
//GRIB file is downloaded from http://www.globalmarinenet.com/free-grib-file-downloads/
//XY Grib appears to be capable of viewing this file - is very useful
//TODO for tomorrow - tests around reading the grid section of a sample file





type PackingType = |GridPoint |SphericalHarmonic
type PackingOrder = |Simple |Complex
type OriginalDataType = |FloatingData |IntegerData
let getBDSFlags (t:byte) =
    let packingtype = match t &&& 1uy = 0uy with
                      |true -> GridPoint
                      |false -> SphericalHarmonic
    let packingOrder =match t &&& 2uy = 0uy with
                      |true -> Simple
                      |false -> Complex
    let dataType = match t &&& 4uy = 0uy with
                      |true -> FloatingData
                      |false -> IntegerData 
    let AdditionalFlags = match t &&& 8uy = 0uy with |true -> false |false -> true 
    if AdditionalFlags then 
        printfn "WARNING - addional flags for BDS - not handled for now - ignoring"
    printfn "Packing type %A" packingtype //grid point
    printfn "Packing order %A" packingOrder //simple
    printfn "data type %A" dataType //floating data
    packingtype,packingOrder,dataType,AdditionalFlags
//needs functions for total windspeed and direction
type Point = 
    {
        Lat : double; // in millidegrees
        Long : double; // in millidegrees
        UWind : float32; // in m/s
        VWind : float32; // in m/s
    }
    member x.Print =
        sprintf "Lat: %f Long %f UWind %f VWind %f" x.Lat x.Long x.UWind x.VWind

let BuildPointList (bms:BMS) (data: _ []) (gds : GDS) =
    //need to make configurable - but for now assume data is read in the sane order
    //potentially data could be stored in a different order in other files
    //Also need to improve to allow for vwind - as well as uwind
    let mutable dataindex = 0
    let output = new ResizeArray<_>()
    for j in ((gds.LatRange |> Array.length)  - 1) .. (-1) .. 0 do
        for i in 0 .. ((gds.LongRange|> Array.length) - 1) do
            if bms.checkpos i j then
                output.Add(
                    {
                        Lat = gds.LatRange.[j];
                        Long = gds.LongRange.[i];
                        UWind = data.[dataindex];
                        VWind = 0.0f;
                    })
                dataindex <- dataindex+1
    output

let readE b1 b2 = 
    //E is the following - initial sign bit and then 15 other bits (int16)
    let i1,i2 = int b1,int b2
    //manual conversion to 16bit integer
    i1<<<8 ||| i2
let readBDS (t:System.IO.Stream) =
    printfn "WARNING - BDS data present but ignored for now - data processing will attempt to continue"
    let secStartPos = t.Position
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore 
    let seclength = getlen buffer
    //bits 1-4 use a table, 5-8 give an empty nummber of bytes - ignoring for now
    let flags = t.ReadByte() |> byte |> getBDSFlags
    //next is binary scale factor
    let E = readE (t.ReadByte()) (t.ReadByte())
    //bits 7-10 are a floating point rep of minimum number - 4 bytes is a float32
    let floatarr = Array.zeroCreate 4
    t.Read(floatarr,0,4) |> ignore
    let minvalue = BitConverter.ToSingle(floatarr |> Array.rev,0) //this value is also called R
    let bitsperpoint = t.ReadByte() //12 in the sample file - WHY 12???
    if (bitsperpoint <> 12) then
        failwith "expecting 12 bits per point of data"
    //so now - we read data as a bit array - note - only get data when point is defined in BMS above
    //how the hell do we read 12 bits (1.5 bytes at a time - more generally - is this alway s 12)
    printfn "%i bits per point" bitsperpoint
    let remainingBytes = (secStartPos + (seclength |> int64) - t.Position) |> int
    let emptybuffer = Array.zeroCreate remainingBytes
    t.Read(emptybuffer,0,remainingBytes) |> ignore
    let output = new ResizeArray<_>()
    let bitarray = new System.Collections.BitArray(emptybuffer)
    for i in 0 .. bitsperpoint .. (bitarray.Length-bitsperpoint) do
        let mybits = new System.Collections.BitArray(bitsperpoint)
        for j in 0 .. (bitsperpoint-1) do
            mybits.[j] <- bitarray.[i+j]
        let myint = mybits |> Utils.getIntFromBitArray |> float32
        let value = minvalue + (myint * (pown 2.0f E))
        output.Add(value)

       // printfn "%f" value
    printfn "Finished reading BDS"
    output

//section one is the product definition section
let readSectionOne (t:System.IO.Stream) =
    let secOneStartPos = t.Position
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore //TODO: insert check here
    let sectonelength = getlen buffer
    printfn "Section one length is %i" sectonelength
    let paramtableversion = t.ReadByte()
    printfn "parameter table version is %i" paramtableversion
    let weathercenter = t.ReadByte()
    printfn "Weather center that produced this forecast is %s" (weathercenters.[weathercenter])
    let methodnumber = t.ReadByte() //can get some info on this, but would need to look it up - on a per country basis
    let gridID = t.ReadByte() |> getMapType
    printfn "%s" gridID
    let GDSPresent,BMSPresent = t.ReadByte() |> OtherSectionPresence
    
    let parameterType = parameters.[t.ReadByte()]
    printfn "Parameter is %s" parameterType
    //next 3 byets are information about heights for the measurement - ignore for now
    t.ReadByte() |> ignore
    t.ReadByte() |> ignore
    t.ReadByte() |> ignore
    //next is date of the forecast
    let ForecastDate = readDate t
    printfn "Forecast date is %O UTC" ForecastDate
    //some information now about when the information is for.  It looks like it is even possible for a single entry to contain multiple forecasts
    //for example, wind every 6 hours is possible
    let forecastTimeUnit = t.ReadByte() |> ForecastTimeUnit
    let numberofUnits = t.ReadByte()
    printfn "Forecast time unit is %i %s"  numberofUnits forecastTimeUnit
    let TimeBetweenForecasts = t.ReadByte()
    printfn "Time between forecasts is %i %s" TimeBetweenForecasts forecastTimeUnit
    let timeRangeIndicator = t.ReadByte() |> TimeRangeIndicator
    printfn "Forecast is %s" timeRangeIndicator
    let zbyte = t.ReadByte()
    if zbyte <> 0 then failwithf "Expected 0 (this means an averaging forecast which I don't know about)"
    let zbyte2 = t.ReadByte()
    if zbyte2 <> 0 then failwithf "Expected 0 (this means an averaging forecast which I don't know about)"
    let zbyte3 = t.ReadByte()
    if zbyte3 <> 0 then failwithf "Expected 0 (this is the number missing from an average)"
    let century = t.ReadByte()
    if century <> 21 then failwithf "Only doing forecasts in the 2000's for now"
    let sub_center = t.ReadByte() //don't know anything about this yet
    let mutable scalebit1 = t.ReadByte()
    if scalebit1 > 128 then
        scalebit1 <- -(scalebit1-128)
    let scalebit2 = t.ReadByte()
    let D = scalebit1*256 + scalebit2
    printfn "D value is %i" D
    let dummybytes = (secOneStartPos + (sectonelength |> int64) - t.Position) |> int
    let emptybuffer = Array.zeroCreate dummybytes
    t.Read(emptybuffer,0,dummybytes) |> ignore
    printfn "Finished reading section 1"
    let mutable gds = None
    let mutable bms = None
    if GDSPresent then
        gds <- readGDS t |> Some
        if BMSPresent then
            bms <- readBMS t gds.Value
    let points = readBDS t
    BuildPointList bms.Value (points.ToArray()) gds.Value
let readHeader (t:System.IO.Stream) =
    let buffer = Array.zeroCreate 8
    let checklen = t.Read(buffer,0,8)
    if (checklen <> 8) then
        failwithf "Unable to read any header data - not enough data in stream"
    else
        let gribchars = buffer.[0..3]
        let str = new string(System.Text.ASCIIEncoding.ASCII.GetChars(gribchars))
        if (str <> "GRIB") then
            failwithf "Header data is invalid - expected \"GRIB\" but got %s" str
        else
            let len = buffer.[4..6] |> Array.map(fun t ->int t)
            let totallen = buffer.[4..6] |> getlen
            printfn "Total length of this record is %i bytes" totallen
            let edition_number = buffer.[7]
            printfn "Edition is %i" edition_number
            readSectionOne t

 

let main argv = 
    let t = ReferenceFiles.Properties.Resources.Pacific_wind_7days
    readHeader(new System.IO.MemoryStream(t)).ForEach(fun r -> printfn "%s" r.Print)
    System.Console.ReadKey(true) |> ignore
    0 // return an integer exit code
