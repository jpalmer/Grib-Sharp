module Utils
open System
//TODO - this can be tested
let getIntFromBitArray (bitArray:System.Collections.BitArray) =
    if bitArray.Length > 32 then
        raise <| ArgumentException("Argument length shall be at most 32 bits.")
    let  array = Array.zeroCreate 1
    bitArray.CopyTo(array, 0)
    array.[0]

