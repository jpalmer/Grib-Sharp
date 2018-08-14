module GDS
open Utils
open Constants
type GDS =
    {
        LatRange : double[];
        LongRange : double[];
    }
    member this.X = this.LongRange |> Array.length
    member this.Y = this.LatRange |> Array.length


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
    //CONFUSING NOTE - i is used for east west, but latitude is read first (N/S)
    let ni = 256 * t.ReadByte() + t.ReadByte() //number of points for lat/lon
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
    printfn "Starting latitude is %d with %i increments of %d and a final latitude of %d" la1 nj dj la2
    printfn "Starting longitude is %d with %i increments of %d and a final longitude of %d" lo1 ni di lo2
    printfn "Finished reading section 2"
    {
        LatRange = seq{ la1 ..dj..la2 } |> Array.ofSeq |> Array.map double;
        LongRange = seq{ lo1 .. di .. lo2} |> Array.ofSeq |> Array.map double;
    }