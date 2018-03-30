open System
open Constants
// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
let file = "C:\Users\john\Desktop\GRIB Data\Pacific.wind.7days.grb\Pacific.wind.7days.grb"

let getlen (t:byte[]) =
    let intarr = t |> Array.map (int)
    intarr.[0]*256*256 + intarr.[1]*256 + intarr.[2]
//this should do some checking on return value
let readlat b1 b2 b3 = 
    if b1 &&& 128 = 0 then
        b1*256*256 + b2*256 + b3
    else
        -(b1-128)*256*256 + b2*256 + b3
//this should also check return value
let readlon b1 b2 b3 = 
    if b1 &&& 128 = 0 then
        b1*256*256 + b2*256 + b3
    else
        -(b1-128)*256*256 + b2*256 + b3

let readSectionTwo (t:System.IO.Stream) =
    let secTwoStartPos = t.Position
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore //TODO: insert check here
    let secTwolength = getlen buffer
    let numVertParams = t.ReadByte()
    let PV = t.ReadByte() //location of list of vert params or location of list of numbers of points in each row OR 255 if neither
    let datarepType = t.ReadByte() |> dataRepresentationType
    printfn "DataRepresentation type is %s" datarepType
    if datarepType <> "lat/long grid" then
        failwithf "only doing lat/long grids for now"
    let ni = 256 * t.ReadByte() + t.ReadByte()
    let nj = 256 * t.ReadByte() + t.ReadByte()
    let la1 = readlat <| t.ReadByte() <| t.ReadByte() <| t.ReadByte()
    let lo1 = readlon <| t.ReadByte() <| t.ReadByte() <| t.ReadByte()


    ()
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
    let GDSorMDSPresence = t.ReadByte() <> 0
    printfn "GDS/MSD is present %b" GDSorMDSPresence
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
    readSectionTwo t
let readHeader (t:System.IO.Stream) =
    let buffer = Array.zeroCreate 8
    let checklen = t.Read(buffer,0,8)
    if (checklen <> 8) then
        printf "Unable to read any header data - not enough data in stream"
    else
        let gribchars = buffer.[0..3]
        let str = new string(System.Text.ASCIIEncoding.ASCII.GetChars(gribchars))
        if (str <> "GRIB") then
            printfn "Header data is invalid - expected \"GRIB\" but got %s" str
        else
            let len = buffer.[4..6] |> Array.map(fun t ->int t)
            let totallen = buffer.[4..6] |> getlen
            printfn "Total length of this record is %i bytes" totallen
            let edition_number = buffer.[7]
            printfn "Edition is %i" edition_number
            readSectionOne t

 

[<EntryPoint>]
let main argv = 
    let data = System.IO.File.OpenRead(file)
    readHeader(data)
    
   
    System.Console.ReadKey(true) |> ignore
    0 // return an integer exit code
