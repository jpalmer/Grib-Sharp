module Core

open System
open Constants
open Utils
open BMS
open GDS
open BDS
open System
open Shared
//GRIB file is downloaded from http://www.globalmarinenet.com/free-grib-file-downloads/
//XY Grib appears to be capable of viewing this file - is very useful
//TODO for tomorrow - tests around reading the grid section of a sample file






let BuildPointList (bms:BMS) (data: _ []) (gds : GDS) (secOne : SectionOne) =
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
                        UWind = match secOne.ParameterType with |UWind -> data.[dataindex] |> Some |_ -> None;
                        VWind = match secOne.ParameterType with |VWind -> data.[dataindex] |> Some |_ -> None;
                    })
                dataindex <- dataindex+1
    output

let mergePointLists a b= 
    //First build up dictionaries
    let d2 = b |> Seq.map (fun t -> (t.Lat, t.Long),t) |> dict
    a 
    |> Seq.map (fun elem ->
        let otherpoint = d2.[elem.Lat,elem.Long]
        if elem.UWind = None then
            {elem with UWind = otherpoint.UWind}
        else
            {elem with VWind = otherpoint.VWind}
        )
    
let readLastSection (t:System.IO.Stream) =
    let buffer = Array.zeroCreate 4
    t.Read(buffer,0,4) |> ignore
    let gribchars = buffer.[0..3]
    let str = new string(System.Text.ASCIIEncoding.ASCII.GetChars(gribchars))
    if (str <> "7777") then
        failwithf "Expected end of GRIB file (7777) but got %s" str
    else
        printfn "Finished parsing a section of the grib file"
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
    
    let parameterType = GetParameter <| t.ReadByte()
    printfn "Parameter is %A" parameterType
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
    let SecOne = 
        {
            D = D;
            ParameterType = parameterType
        }
    if GDSPresent then
        gds <- readGDS t |> Some
        if BMSPresent then
            bms <- readBMS t gds.Value
    let points = readBDS t SecOne
    readLastSection t
    BuildPointList bms.Value (points.ToArray()) gds.Value SecOne

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
    let stream =  new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(t));
    let points1 = readHeader(stream) //.ForEach(fun r -> printfn "%s" r.Print)
    let points2 = readHeader(stream)//.ForEach(fun r -> printfn "%s" r.Print)
    let merged = mergePointLists points1 points2
    merged
    |> Seq.iter (fun t -> printfn "%A" t)
    System.Console.ReadKey(true) |> ignore
    0 // return an integer exit code
