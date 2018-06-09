module Utils
open System
open System.IO
//TODO - this can be tested
let getIntFromBitArray (bitArray:System.Collections.BitArray) =
    if bitArray.Length > 32 then
        raise <| ArgumentException("Argument length shall be at most 32 bits.")
    let  array = Array.zeroCreate 1
    bitArray.CopyTo(array, 0)
    array.[0]
let StringToStream (s:string) =
    let stream = new MemoryStream()
    let writer = new StreamWriter(stream)
    writer.Write(s)
    writer.Flush()
    stream.Position <- 0L
    stream


let getlen (t:byte[]) =
    if t.Length < 3 then 0
    else
        let intarr = t |> Array.map (int)
        intarr.[0]*256*256 + intarr.[1]*256 + intarr.[2]

//this should do some checking on return value
let readlat b1 b2 b3 = 
    printfn "%i %i %i" b1 b2 b3
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