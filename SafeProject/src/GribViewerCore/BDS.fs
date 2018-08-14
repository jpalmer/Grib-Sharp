module BDS
open Utils
open Constants
open System


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

let readE b1 b2 = 
    //E is the following - initial sign bit and then 15 other bits (int16)
    let i1,i2 = int b1,int b2
    //manual conversion to 16bit integer
    i1<<<8 ||| i2

let readBDS (t:System.IO.Stream) (secOne:SectionOne)=
    let secStartPos = t.Position
    let buffer = Array.zeroCreate 3
    t.Read(buffer,0,3) |> ignore 
    let seclength = getlen buffer
    //bits 1-4 use a table, 5-8 give an empty nummber of bytes - ignoring for now
    let flags = t.ReadByte() |> byte |> getBDSFlags
    //next is binary scale factor
    let E = readE (t.ReadByte()) (t.ReadByte())
    //bits 7-10 are a floating point rep of minimum number - 4 bytes is a float32
    printfn "Scale factor E is %i" E
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
    let bitarray = ReversedBitrray(emptybuffer)
    let decfactor = pown 10 secOne.D |> float32 |> fun t -> 1.0f/t
    for i in 0 .. bitsperpoint .. (bitarray.Length-bitsperpoint) do
        let mybits = new System.Collections.BitArray(bitsperpoint)
        for j in 0 .. (bitsperpoint-1) do
            mybits.[j] <- bitarray.[i+j]
        let myint = mybits |> Utils.getIntFromBitArray |> float32
        let value = (minvalue + (myint * (pown 2.0f E))) * decfactor
        output.Add(value)

       // printfn "%f" value
    printfn "Finished reading BDS"
    output

