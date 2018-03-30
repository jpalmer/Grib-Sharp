module Constants

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
    let reader = new Microsoft.VisualBasic.FileIO.TextFieldParser("GribDataTypes.csv")
    reader.SetDelimiters(",")
    let entryList = new ResizeArray<_>()
    reader.ReadLine() |> ignore //ignore header line
    while reader.EndOfData |> not do
        let fields = reader.ReadFields()
        let ID = fields.[0] |> int
        let measurementType = fields.[1]
        entryList.Add(ID,measurementType)
    done
    dict entryList

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