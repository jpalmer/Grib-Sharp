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