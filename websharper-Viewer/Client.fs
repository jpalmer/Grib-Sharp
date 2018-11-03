namespace websharper_Viewer

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.D3
[<JavaScript>]
module Client =
    type topoJsonPointDetails = 
        {
            ``type``: string;
            coordinates : double[];
        }
    type topoJsonPoint=
        {
            ``type`` : string;
            geometry : topoJsonPointDetails
        }
   
    type topoJsonParentObject =
        {
            ``type``: string;
            features : topoJsonPoint[];
        }
    [<Name "">]
    type ITopoJson =
        abstract feature : topology: obj * geoObject: obj -> obj
        abstract mesh : topology: obj * geoObject: obj * filter: (obj * obj -> bool) -> obj
        //This doesn't compile - how to fix???????????
    type dummy() = 
        [<Direct "g.Attr(\"transform\",\"translate(\"d3.event.translate\")\")" >]
        static member zoomed = X<_>
    let topojson : ITopoJson =
        JS.Global?topojson
    let projection = D3.Geo.Mercator()
    let Main () =
        let world = WebSharper.Json.Parse <| Server.GetWorld()
        let rvInput = Var.Create ""

        let width = 500
        let height = 500
        let svg = D3.Select("#map").Append("svg")
                    .Attr("width", width)
                    .Attr("height", height)

        let g = svg.Append("g"); //not sure what the g is for
        g.Append("path")
            .Datum(topojson.feature(world, world?objects?subunits))
            .Attr("d", D3.Geo.Path().Projection(projection)) |> ignore 
        let submit = Submitter.CreateOption rvInput.View
        let data = Server.GetWind "Pacific.wind.7days.grb"
        let path = D3.Geo.Path().Projection(projection).PointRadius(2.)

        let zoom =new System.Action<_>(fun  _ -> g.Attr("transform", "translate("+D3.Event.JS?translate+")") |> ignore)
        svg.Call(zoom) |> ignore
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
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> 
                    g.Append("path")
                        .Datum(TopoJsonData)
                        .Attr("d", path) |> ignore
                    async {return ""}
            )
        div [] [
            Doc.Input [] rvInput
            Doc.Button "Send" [] submit.Trigger
            hr [] []
            h4 [attr.``class`` "text-muted"] [text "The server responded:"]
            div [attr.``class`` "jumbotron"] [h1 [] [textView vReversed]]
        ]
