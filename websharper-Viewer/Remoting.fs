namespace Viewer

open WebSharper
open System.Drawing

module Server =
    type Point = //websharper is not at all happy if this type is in a different assembly
        {Lat : double; Long : double; WindSpeed: double}
        [<JavaScript>]
        member x.Test = 1
    ///Try to get the parsed data
    [<Rpc>]
    let GetWind (arg:string) =  
        let t = Core.readHeader(System.IO.File.OpenRead(arg)).ToArray() |> Array.iter (fun t -> t.Print |> printfn "%s")
        Core.readHeader(System.IO.File.OpenRead(arg)).ToArray() 
        |> Array.filter (fun t -> t.UWind.IsSome && t.UWind.IsSome) // TODO - bug only gets uwind at the moment
        |> Array.map (fun t -> 
            {Lat = t.Lat; Long = t.Long; 
            WindSpeed = 
             let vx = Option.get t.UWind in
             let vy = Option.get t.UWind in
             vx*vx + vy*vy |> sqrt |> double})
    ///gets topojson world
    [<Rpc>]
    let GetWorld () = System.IO.File.ReadAllText("World.json")
