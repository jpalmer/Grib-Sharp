namespace websharper_Viewer

open WebSharper
open System.Drawing

module Server =
    type Point = //websharper is not at all happy if this type is in a different assembly
        {Lat : double; Long : double}
    ///Try to get the parsed data
    [<Rpc>]
    let GetWind (arg:string) =  Core.readHeader(System.IO.File.OpenRead(arg)).ToArray() |> Array.map (fun t -> {Lat = t.Lat; Long = t.Long})
    ///gets topojson world
    [<Rpc>]
    let GetWorld () = System.IO.File.ReadAllText("World.json")
