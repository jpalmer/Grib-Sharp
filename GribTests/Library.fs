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

