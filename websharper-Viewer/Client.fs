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
[<JavaScript>]
module Client =
    let projection = D3.Geo.Mercator().Rotate(180.0,0.0,0.0)
    let test = {Lat=1.0;Long=1.0; WindSpeed = 1.0}.Test

    type windPoint =
        {
            position : Vector;
            velocity : Vector;
        }
        with 
            member x.update = 
                {position = x.position.Add(x.velocity.Multiply(1.0));velocity = x.velocity}
    type WindInterpolator(points: Server.Point[]) =
        //first - lets get the xcocrds
        let lats = points |> Array.map (fun t -> t.Lat) |> Array.distinct |> Array.sort
        let latmap = new Dictionary<_,_>() //do it this way because the F# dict function doesn't play nice with websharper
        do ( lats |> Array.iteri(fun i t -> latmap.Add (t,i)))
        let longs = points |> Array.map (fun t -> t.Long) |> Array.distinct |> Array.sort
        let longmap = new Dictionary<_,_>()
        do ( longs |> Array.iteri(fun i t -> longmap.Add (t,i)))
        //now lets create our wind vector map - now we have a 2D array
        let Data = Array2D.zeroCreate (lats.Length) (longs.Length)
        do points |> Array.iter(fun p -> Data.[latmap.[p.Lat],longmap.[p.Long]] <- {x=p.WindSpeed;y=0.0} )
        let points = points |> Array.map(fun t -> {position = {x=t.Long/1000.0;y=t.Lat/1000.0}; velocity = {x=t.WindSpeed;y=1.0}})
        member x.Points = points
    let Animate (selection:Selection<windPoint>) = 

        selection
            .Attr("x1",fun (t:windPoint) -> t.position.Project projection |> fst)
            .Attr("y1",fun (t:windPoint) -> t.position.Project projection |> snd)
            .Attr("x2",fun (t:windPoint) -> t.update.position.Project projection |> fst)
            .Attr("y2",fun (t:windPoint) -> t.update.position.Project projection |> snd)
            .Attr("stroke","black")
            .Style("opacity", 0.5)
            (*
            .Transition()
                .Ease("linear")
                .Delay(100)
                .Duration(1000)
                *)
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

        let width = 2000
        let height = 500
        let svg = D3.Select("#map").Append("svg")
                    .Attr("width", width)
                    .Attr("height", height)

        let g = svg.Append("g"); //not sure what the g is for but it makes things work
        g.Append("path")
     
            .Datum(topojson.feature(world, world?objects?subunits))
            .Attr("d", D3.Geo.Path().Projection(projection)) |> ignore 
        let submit = Submitter.CreateOption rvInput.View
        let data = Server.GetWind "Pacific.wind.7days.grb"
        let path = D3.Geo.Path().Projection(projection).PointRadius(2.)

        let zoomed = new System.Action(fun  _ -> g.Attr("transform", "translate("+D3.Event?translate+")scale(" + D3.Event?scale + ")") |> ignore)
        let zoom = D3.Behavior.Zoom().ScaleExtent(1., 8.).On(ZoomType.Zoom, zoomed)
        svg.Call(As<Function> zoom) |> ignore       
        let TopoJsonData = 
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
            (*
        function lineAnimate(selection) {
    selection
    .attr({x1: 200, x2: 200})
    .attr('y1', function(d) {return d;})
    .attr('y2', function(d) {return d;})
    .style('opacity', 0.5)
    .transition()
        .ease('linear')
        .duration(1000)
        .delay(function(d) {return d*10;})
        .attr('x2', 500)
    .transition()
        .duration(1000)
        .style('opacity', 0)
    .each('end', function() {d3.select(this).call(lineAnimate)});
}
*)
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> 
                    // add circles to svg
                    //WebSharper.JavaScript.Console.log data
                //    let contours = D3.contours()
        (*            contours = d3.contours()
    .size([grid.n, grid.m])
    .thresholds(thresholds)
  (grid)
    .map(transform)
    *)
                    let interpolator = WindInterpolator(data).Points
                    g.SelectAll("line")
                     .Data(interpolator)
                     .Enter()
                     .Append("line")
                     .Call(Animate)
                     |> ignore
                     
                    g.SelectAll("circle").Data(data)
                     .Enter()
                     .Append("circle")
                     .Attr("cx",fun (d:Server.Point) -> (projection.Apply (d.Long/1000.0,d.Lat/1000.0 ) |> fst ))
                     .Attr("cy",fun (d:Server.Point) -> (projection.Apply (d.Long/1000.0,d.Lat/1000.0 ) |> snd ))
                     .Attr("fill",fun d -> rgb d )
                     .Attr("r",fun d -> "1px" )
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
