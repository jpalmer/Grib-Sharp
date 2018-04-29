namespace GribViewerCore

module Say =
    let hello name =
        printfn "Hello %s" name
    let testfn arg = 
        true
    type testytype() =
        member x.TestFunc() = true
        