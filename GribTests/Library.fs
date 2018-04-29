namespace GribTests
open FsCheck.Xunit
module Core =
    open Xunit

    [<Property>]
    let ``Simple passing test``(xs:list<int>) =
      true
      
    [<Property>]
    let test (i:int) =
        GribViewerCore.Say.testfn i

    [<Xunit.FactAttribute>]
    let test3 () =
        GribViewerCore.Say.testytype().TestFunc()
    [<Xunit.FactAttribute>]
    let test4 () =
        true
