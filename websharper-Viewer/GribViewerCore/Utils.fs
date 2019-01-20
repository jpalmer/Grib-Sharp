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
//Some of the data needs to be read from the bits with a bitarray that reads the bits in the reverse order - this code handles that
//TODO -  I think this will reverse the bytes in the input array - should actually create a copy
type ReversedBitrray (data:byte[]) =
     //this code apparently reverses a byte - magic bit twiddling from stack overflow 
     //(does seem crazy that 4 operations on 64 bit integers is a sane way to do this
    let newdata = data |> Array.map (fun t -> 
        ((((uint64 t) * 0x80200802uL) &&& 0x0884422110uL) * 0x0101010101uL >>> 32) |> byte)
    let bitarray = new System.Collections.BitArray(newdata)
    member this.Item
      with get(index) = bitarray.[index]
    member x.Length = bitarray.Length

type Parameters = |UWind |VWind |Other of string
type SectionOne = 
    {   
        D:int;
        ParameterType: Parameters
    }
