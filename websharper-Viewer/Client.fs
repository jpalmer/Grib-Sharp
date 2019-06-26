namespace Viewer
open System.Collections.Generic
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.D3
open System.Numerics
open Viewer.Vector
open Server
open Viewer.topoJson
open WebSharper.D3
[<JavaScript>]
module Client =
    type MyLine<'T> = 
        inherit Line<'T>
        [<Inline("$this.x($1)")>]
        member x.MyX (arg: ('T * int) -> double) : MyLine<'T> = 
            failwith "Not implemented"
        [<Inline("$this.y($1)")>]
        member x.MyY (arg: ('T * int) -> double) : MyLine<'T> = 
            failwith "Not implemented"
        [<Inline("d3.svg.line()")>]
        static member BuildLine() : MyLine<'T>= 
            failwith "Not implemented"
    let projection = D3.Geo.Mercator().Rotate(180.0, 0.0,0.0)
    let test = {Lat=1.0;Long=1.0; WindSpeed = 1.0}.Test
    type WindPoint =
        {
            position : Vector;
            velocity : Vector;
        }
        with 
            member x.Update (interp: WindInterpolator)= 
                    let newBasePoint = x.position.Add(x.velocity.Multiply(0.1))
                    let newWind = interp.ClosestPoint  <| newBasePoint
                    {position = newBasePoint; velocity = newWind }
            member x.UpdateN i interp = 
                [0..i] |> Seq.fold(fun (state:WindPoint) _ -> state.Update interp) x
    and WindInterpolator(points: Server.Point[]) =
        //first - lets get the xcocrds
        let lats = points |> Array.map (fun t -> t.Lat) |> Array.distinct |> Array.sort
        let latmap = new Dictionary<_,_>() //do it this way because the F# dict function doesn't play nice with websharper
        do ( lats |> Array.iteri(fun i t -> latmap.Add (t / 1000.0, i)))
        let longs = points |> Array.map (fun t -> t.Long) |> Array.distinct |> Array.sort
        let longmap = new Dictionary<_,_>()
        do ( longs |> Array.iteri(fun i t -> longmap.Add (t/1000.0, i)))
        //now lets create our wind vector map - now we have a 2D array
        let Data = Array2D.zeroCreate (lats.Length) (longs.Length)
        do points |> Array.iter(fun p -> Data.[latmap.[p.Lat/1000.0],longmap.[p.Long/1000.0]] <- {x=p.WindSpeed;y=0.0} )
        let points = points |> Array.map(fun t -> {position = {x=t.Long/1000.0;y=t.Lat/1000.0}; velocity = {x=t.WindSpeed;y=1.0}})
        member x.Points = points
        member x.ClosestPoint (point:Vector) =
            let lat = lats |> Array.minBy(fun t -> Math.Abs(t - point.x))
            let long = longs |> Array.minBy(fun t -> Math.Abs(t - point.y))
            Data.[latmap.[lat/1000.0],longmap.[long/1000.0]]
    let RNG = System.Random()
    let mutable myInterpolator = None
    let interp() = myInterpolator.Value
    //animated lines of wind
    let Animate (selection:Selection<WindPoint>) = 
        let line = MyLine.BuildLine()
                    .MyX(fun (t:WindPoint,index:int) -> (t.UpdateN (index |> int) <| interp()).position.x )
                    .MyY(fun (t:WindPoint,index:int) -> (t.UpdateN (index |> int) <| interp()).position.y )
                    .Interpolate(Interpolation.Cardinal) //cardinal interpolation looks like some sort of spline

        //basically the way that this works is that this function is only called once, but all the inner functions are called once per point
        let totalLength = "10"
        selection
            .Append("line")
            
            //x1/y1 are start points of line, x2/y2 are end
            .Attr("x1", fun (t:WindPoint) -> t.position.Project projection |> fst)
            .Attr("y1", fun (t:WindPoint) -> t.position.Project projection |> snd)
            .Attr("x2", fun (t:WindPoint) -> t.Update <| interp() |> fun a -> a.position.Project projection |> fst)
            .Attr("y2", fun (t:WindPoint) -> t.Update <| interp() |> fun a -> a.position.Project projection |> snd)
            .Attr("stroke", "red")
            .Style("opacity", 0.5)
            //this is magic.  need to produce a string like  "100 100" where 100 is the length of the line
            .Attr("stroke-dasharray", fun (t:WindPoint) -> 
                let (sx, sy) = t.position.Project projection
                let (ex, ey) = (t.Update <| interp()).position.Project projection
                let len = {x=sx-ex;y=sy-ey}.Length |> int
                sprintf "%i %i" len len
                )
            // Set the intial starting position so that only the gap is shown by offesetting by the total length of the line
            .Attr("stroke-dashoffset", fun (t:WindPoint) -> 
                let (sx, sy) = t.position.Project projection
                let (ex, ey) = (t.Update <| interp()).position.Project projection
                {x=sx-ex;y=sy-ey}.Length
                )
            // Then the following lines transition the line so that the gap is hidden...
            .Transition()
            .Duration(5000) //duration is pretty arbitrary
            .Ease("linear") //may not be the best, but should be simplest / fastest
            .Attr("stroke-dashoffset", 0) //we are transitioning to a case where there is no offset
            
            |> ignore
        ()
    let topojson : ITopoJson =
        JS.Global?topojson
    let rgb (point: Server.Point) = 
        let speed = point.WindSpeed |> int
        sprintf "rgb(%i,%i,%i)" speed speed speed
    let Main () =
        let world = WebSharper.Json.Parse <| Server.GetWorld()
        let rvInput = Var.Create ""
        //create the container for the svg
        let width = 2000
        let height = 500
        let svg = D3.Select("#map").Append("svg")
                    .Attr("width", width)
                    .Attr("height", height)

        let g = svg.Append("g"); //not sure what the g is for but it makes things work
        //create the world map
        g.Append("path")
     
            .Datum(topojson.feature(world, world?objects?subunits))
            .Attr("d", D3.Geo.Path().Projection(projection)) |> ignore 
        let submit = Submitter.CreateOption rvInput.View
        let data = Server.GetWind "Pacific.wind.7days.grb"
        let path = D3.Geo.Path().Projection(projection).PointRadius(2.)
        //set up the zoom with the mousewheel stuff
        let zoomed = new System.Action(fun  _ -> g.Attr("transform", "translate("+D3.Event?translate+")scale(" + D3.Event?scale + ")") |> ignore)
        let zoom = D3.Behavior.Zoom().ScaleExtent(1., 8.).On(ZoomType.Zoom, zoomed)
        svg.Call(As<Function> zoom) |> ignore       
        //data converted to topojson (not currently used, but could be useful later)
        let topoJsonData = 
            {
                ``type`` = "FeatureCollection";
                features = data |> Array.map (fun p ->
                    {
                        ``type`` = "Feature";
                        geometry =
                            {
                                ``type`` = "Point" ;
                                coordinates = [|p.Long/1000.0;p.Lat/1000.0|]
                            }               
                    })
            }
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> 
                    //animated arrows for wind direction
                    myInterpolator <- WindInterpolator(data) |> Some
                    let data = interp().Points |> Array.filter(fun _ -> RNG.NextDouble() < 0.1) //performance optimise - don't show all points
                    g.SelectAll("line")
                     .Data(data)
                     .Enter()
                     .Call(Animate)
                     |> ignore
                    //circles showing data points (really only for debugging)
                    g.SelectAll("circle").Data(data)
                     .Enter()
                     .Append("circle")
                     .Attr("cx", fun (d:Server.Point) -> (projection.Apply (d.Long/1000.0, d.Lat/1000.0 ) |> fst ))
                     .Attr("cy", fun (d:Server.Point) -> (projection.Apply (d.Long/1000.0, d.Lat/1000.0 ) |> snd ))
                     .Attr("fill", fun d -> rgb d )
                     .Attr("r", fun d -> "0.1px" )
                     |> ignore


                    async {return ""}
            )
        div [] [
            Doc.Input [] rvInput
            Doc.Button "Send" [] submit.Trigger
            hr [] []
            h4 [attr.``class`` "text-muted"] [text "The server responded:"]
            div [attr.``class`` "jumbotron"] [h1 [] [textView vReversed]]
        ]
