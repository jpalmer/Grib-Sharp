module BMS
open Utils
open GDS
type BMS (data:byte[] ,gds : GDS) =
    do printfn "x = %i y = %i expected size %i actual size %i" gds.X gds.Y ((gds.X * gds.Y)/8) (data.Length)
    //so - for some fun reason the C# bitarray is viewing the bits in the wrong order
    let bitarray = new ReversedBitrray(data)
    member this.checkpos i j = 
        bitarray.[j*gds.X+i] //this will depend on the order of the bits - may need some more checking later
    member this.print() =
        printfn "Bitmap from BMS - x length %i y length %i" gds.X gds.Y
        for j in (gds.Y-1)..(-1)..(0) do  
            for i in 0..(gds.X-1) do
                if i % 10 = 0 then 
                    printf " "
                match this.checkpos i j with
                |true -> printf "X"
                |false -> printf "."
            printfn ""
//The BMS defines a section in the following way - each bit lets you know if a point is available
//Useful for example - you have data which is sea temperature - black out the non-sea points
let readBMS (t:System.IO.Stream) gds =
    printfn "Reading BMS section (bitmap used for specifying which data is present)"
    let secStartPos = t.Position
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore //TODO: insert check here
    let unusedbytes = t.ReadByte() |> fun t -> 0 //this is the number of unused bytes - ignore for now to make things work - may need to think about this a little - why is the extra byte needed?
    let seclength = getlen buffer
    let rvalue =
        if (t.ReadByte() = 0 && t.ReadByte() = 0) then //otherwise there is a predefined bitmap
            let databytes = (secStartPos + (seclength |> int64) - t.Position - (int64 unusedbytes)) |> int
            let databuffer :byte [] = Array.zeroCreate databytes
            t.Read(databuffer,0,databytes) |> ignore
            let bms =  new BMS(databuffer,gds)
            bms |> fun t -> t.print()
            let unusedbytesarr = Array.zeroCreate unusedbytes
            t.Read(unusedbytesarr,0,unusedbytes) |> ignore
            Some(bms)
        else
            None
        //TODO: handle predefined bitmap
    if t.Position <> secStartPos + (seclength|>int64) then
        printfn "read too many bytes"

    printfn "Finished reading BMS"
    rvalue