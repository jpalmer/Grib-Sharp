﻿module Core

open System
open Constants
// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
//TODO: automatically download this file
let file = "Pacific.wind.7days.grb"  //available from here: http://www.globalmarinenet.com/free-grib-file-downloads/

let getlen (t:byte[]) =
    if t.Length < 3 then 0
    else
        let intarr = t |> Array.map (int)
        intarr.[0]*256*256 + intarr.[1]*256 + intarr.[2]
//this should do some checking on return value
let readlat b1 b2 b3 = 
    if b1 &&& 128 = 0 then
        b1*256*256 + b2*256 + b3
    else
        -(b1-128)*256*256 + b2*256 + b3

let readlatFine b1 b2 = 
    if b1 &&& 128 = 0 then
        b1*256 + b2
    else
        -(b1-128)*256 + b2

//this should also check return value

let readlon b1 b2 b3 = 
    if b1 &&& 128 = 0 then
        b1*256*256 + b2*256 + b3
    else
        -(b1-128)*256*256 + b2*256 + b3

let readlonFine b1 b2  = 
    if b1 &&& 128 = 0 then
        b1*256 + b2
    else
        -(b1-128)*256 + b2

type BMS (data:byte[] ,x, y) =
    do printfn "x = %i y = %i expected size %i actual size %i" x y ((x * y)/8) (data.Length)
    member x.checkpos t = 
        let bytenumber = t/8;
        let bit = 1 <<< (t % 8) |> byte
        (data.[bytenumber] &&&  bit) <> 0uy
        //TODO - method to print based on grid size
    member this.print() =
        printfn "Bitmap from BMS - x length %i y length %i" x y
        for i in 0..(y-1) do 
            for j in 0..(x-1) do
                match this.checkpos <| i*x+j with
                |true -> printf "X"
                |false -> printf "."
            printfn ""
//The BMS defines a section in the following way - each bit lets you know if a point is available
//Useful for example - you have data which is sea temperature - black out the non-sea points
let readBMS (t:System.IO.Stream) xpoints ypoints =
    printfn "Reading BMS section (bitmap used for specifying which data is present)"
    let secStartPos = t.Position
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore //TODO: insert check here
    let unusedbytes = t.ReadByte() |> fun t -> 0 //this is the number of unused bytes - ignore for now to make things work
    let seclength = getlen buffer
    if (t.ReadByte() = 0 && t.ReadByte() = 0) then //otherwise there is a predefined bitmap
        let databytes = (secStartPos + (seclength |> int64) - t.Position - (int64 unusedbytes)) |> int
        let databuffer :byte [] = Array.zeroCreate databytes
        t.Read(databuffer,0,databytes - unusedbytes) |> ignore
        new BMS(databuffer,xpoints,ypoints) |> fun t -> t.print()
        let unusedbytesarr = Array.zeroCreate unusedbytes
        t.Read(unusedbytesarr,0,unusedbytes) |> ignore
    else
        //TODO: handle predefined bitmap
    printfn "Finished reading BMS"

let readBDS (t:System.IO.Stream) =
    printfn "WARNING - BDS data present but ignored for now - data processing will attempt to continue"
    let secStartPos = t.Position
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore //TODO: insert check here
    let seclength = getlen buffer
    let dummybytes = (secStartPos + (seclength |> int64) - t.Position) |> int
    let emptybuffer = Array.zeroCreate dummybytes
    t.Read(emptybuffer,0,dummybytes) |> ignore
    printfn "Finished reading BMS"


//grid description section - section 2
let readGDS (t:System.IO.Stream) = 
    let secTwoStartPos = t.Position
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore //TODO: insert check here
    let secTwolength = getlen buffer
    let numVertParams = t.ReadByte()
    let PV = t.ReadByte() //location of list of vert params or location of list of numbers of points in each row OR 255 if neither
    let datarepType = t.ReadByte() |> dataRepresentationType
    printfn "Data Representation type is %s" datarepType
    if datarepType <> "lat/long grid" then
        failwithf "only doing lat/long grids for now"
    //this part is specific to lat/long grids - other types of grids have different formatting
    let ni = 256 * t.ReadByte() + t.ReadByte()
    let nj = 256 * t.ReadByte() + t.ReadByte()
    let la1 = readlat <| t.ReadByte() <| t.ReadByte() <| t.ReadByte()
    let lo1 = readlon <| t.ReadByte() <| t.ReadByte() <| t.ReadByte()
    let flags = t.ReadByte() |> getResolutionFlags
    let la2 = readlat <| t.ReadByte() <| t.ReadByte() <| t.ReadByte()
    let lo2 = readlon <| t.ReadByte() <| t.ReadByte() <| t.ReadByte()
    let di = readlatFine <| t.ReadByte() <| t.ReadByte()
    let dj = readlonFine <| t.ReadByte() <| t.ReadByte()
    let scanflags = t.ReadByte() |> getScanningFlags
    let dummybytes = (secTwoStartPos + (secTwolength |> int64) - t.Position) |> int
    let emptybuffer = Array.zeroCreate dummybytes
    t.Read(emptybuffer,0,dummybytes) |> ignore
    printfn "Finished reading section 2"
    ni,nj
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
    if GDSPresent then
        let x,y = readGDS t
        if BMSPresent then
            readBMS t x y
    ()
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

 


let main argv = 
    let data = System.IO.File.OpenRead(file)
    readHeader(data)
    
   
    System.Console.ReadKey(true) |> ignore
    0 // return an integer exit code
