namespace GribTests
open FsCheck.Xunit
open Xunit
module Core =
    [<Property>]
    let testlen_simple arg = 
        if arg |> Array.length < 3 then
            Core.getlen arg = 0
        else
            Core.getlen arg > 0
    [<Property>]
    let ``readLat = readlon`` a b c =
        Core.readlat a b c = Core.readlon a b c
    [<Property>]
    let ``readLatfine = readlonfine`` a b  =
        Core.readlatFine a b  = Core.readlonFine a b

