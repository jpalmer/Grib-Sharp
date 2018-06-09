namespace GribTests
open FsCheck.Xunit
open Xunit
open Utils
module Core =
    [<Property>]
    let testlen_simple arg = 
        if arg |> Array.length < 3 then
            Utils.getlen arg = 0
        else
            Utils.getlen arg > 0
    [<Property>]
    let ``readLat = readlon`` a b c =
        readlat a b c = readlon a b c
    [<Property>]
    let ``readLatfine = readlonfine`` a b  =
        readlatFine a b  = readlonFine a b

