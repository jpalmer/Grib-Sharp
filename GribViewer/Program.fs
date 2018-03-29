// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
let file = "C:\Users\john\Desktop\GRIB Data\Pacific.wind.7days.grb\Pacific.wind.7days.grb"

let weathercenters =  
    dict 
        [
            (7,"US Weather Service - National Met. Center");
            (8,"US Weather Service - NWS Telecomms Gateway");
            (9,"US Weather Service - Field Stations");
            (34,"Japanese Meteorological Agency – Tokyo");
            (52,"National Hurricane Center, Miami");
            (54,"Canadian Meteorological Service – Montreal");
            (57,"U.S. Air Force - Global Weather Center58US Navy - Fleet Numerical Oceanography Center");
            (59,"NOAA Forecast Systems Lab, Boulder CO");
            (74,"U.K. Met Office – Bracknell");
            (85,"French Weather Service – Toulouse");
            (97,"European Space Agency (ESA)");
            (98,"European Center for Medium-Range Weather Forecasts – Reading");
            (99,"DeBilt, Netherlands");
        ] 
    
let getlen (t:byte[]) =
    let intarr = t |> Array.map (int)
    intarr.[0]*256*256 + intarr.[1]*256 + intarr.[2]

//section one is the product definition section
let readSectionOne (t:System.IO.Stream) =
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore //TODO: insert check here
    let sectonelength = getlen buffer
    printfn "Section one length is %i" sectonelength
    let paramtableversion = t.ReadByte()
    printfn "parameter table version is %i" paramtableversion
    let weathercenter = t.ReadByte()
    printfn "Weather center that produced this forecast is %s" (weathercenters.[weathercenter])
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
