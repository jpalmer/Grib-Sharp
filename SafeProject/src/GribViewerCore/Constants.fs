module Constants

open System
open System.Runtime
open Utils
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

let parameters =
    ReferenceFiles.Properties.Resources.GribDataTypes.Split( [|"\r";"\r\n";"\n"|],StringSplitOptions.RemoveEmptyEntries)
    |> Array.skip (1) //skip header line
    |> Array.map (fun t -> t.Split(','))
    |> Array.map (fun t -> (t.[0] |> int),t.[1])
    |> dict



let GetParameter t = 
    match t with
    |33 -> UWind
    |34 -> VWind
    |a  -> Other(parameters.[a])

let OtherSectionPresence t = 
    let GDSPresence = (t&&&128)=128
    let BMSPresence = (t&&&64)=64
    printfn "GDS present: %b BMSPresent %b" GDSPresence BMSPresence
    GDSPresence,BMSPresence

let ForecastTimeUnit t =
    match t with
    |0 -> "minute"
    |1 -> "hour"
    |2 -> "day"
    |3 -> "month"
    |4 -> "year"
    |5 -> "decade"
    |6 -> "normal (30Y)"
    |7 -> "century"
    |x when x > 7 && x<254 -> "reserved"
    |254 -> "second"
    |_ -> failwithf "Invalid forecast time unit"

let TimeRangeIndicator t = 
    match t with
    |0 -> "valid at reference time+P1 (P1>0),analysis for reference time (P1=0), image for refernce time (P1=0)"
    |1 -> "initialized analysis for reference time"
    |2 -> "Valid between reference time + P1 and reference time + P2"
    |3 -> "Average (reference time + P1, reference time + P2"
    |4 -> "Accumulation (reference time + P1 to reference time + P2)"
    |5 -> "Difference (reference time + P2 minus reference time + P1)"
    |6|7|8|9 -> "Reserved"
    |10 -> "P1 is octets 19 and 20, valid at reference time + P1"
    |x when x>10 && x < 51 -> "Reserved"
    |51 -> "Mean value - methodology is complex - fill in later"
    //some other formats, not bothering to explain for now
    |_ -> failwithf "Some more complicated formats of reference data - not bothering for now"

let getMapType gridID = 
    match gridID with
    |21 | 22 | 23 |24 |25 |26 |61|62|63|64 -> "IEG grid"
    |37|38|39|40|41|42|43|44 -> "thinned grid"
    |255 -> "file will define grid"
    |_ -> "Unknown grid type"

let readDate (t:System.IO.Stream) = 
    let yearofcentury = t.ReadByte()
    let monthOfYear = t.ReadByte()
    let dayofMonth = t.ReadByte()
    let hourofDay = t.ReadByte()
    let minuteofHour = t.ReadByte()
    new DateTime(yearofcentury+2000,monthOfYear,dayofMonth,hourofDay,minuteofHour,0,DateTimeKind.Utc)

let dataRepresentationType t =
    match t with
    |0 -> "lat/long grid"
    |1 -> "mercator grid"
    |2 -> "gnomic grid"
    |3 -> "Lambert conformal grid"
    |4 -> "Gaussian lat/long grid"
    |5 -> "Polar stereographic grid"
    |t when t>5 && t<13 -> "reserved"
    |13 -> "Oblique lambert conformal"
    |t when t>13 && t<50 -> "reserved"
    |50 -> "Spherical harmonic coefficients"
    |t when t>50 && t<90 -> "reserved"
    |90 -> "space view perspective/orthographic"
    |t when t>90 && t<255 -> "reserved"
    | _ -> failwithf "Unknown data representation type"

let getResolutionFlags t =
    let directionIncrementsgiven = (128 &&& t) = 128
    let oblatespheroid = (64 &&& t) = 64
    //bit 3,4 reserved
    let uvWindRelativeToGrid = (8 &&& t) = 8 //if false u=east,v=north
    printfn "direction increments %b, oblate spheroid %b, wind directions relative to grid %b" directionIncrementsgiven oblatespheroid uvWindRelativeToGrid
    directionIncrementsgiven,oblatespheroid,uvWindRelativeToGrid

let getScanningFlags t =
    let minusIdir = (128 &&& t) = 128
    let minusJdir = (64 &&& t) = 64
    let jPointsConsecutive = (32 &&& t) = 32
    printfn "scan in -i direction %b, scan in -j direction %b, j points consecutive %b" minusIdir minusJdir jPointsConsecutive
    minusIdir, minusJdir, jPointsConsecutive