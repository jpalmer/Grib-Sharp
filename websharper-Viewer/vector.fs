namespace Viewer
open WebSharper.D3
open WebSharper
[<JavaScript>]
module Vector = 
    let keepinbounds x = 
        if x > 180.0 then x-360.0
        else if x < 180.0 then x + 360.0
        else x
    type Vector = 
        {
            x : double;
            y : double;
        }
        static member FromPolar r theta = {x = r * cos(theta); y = r * sin(theta)}
        static member Subtract a b = 
            {x=a.x-b.x;y=a.y-b.y }
        member x.Length = x.x*x.x+x.y*x.y |> sqrt    
        member x.Scale l = 
            let mylen = x.Length
            let scale = l / mylen
            {x=x.x*scale;y=x.y*scale}
        member x.Multiply l =
            {x = x.x * l;y=x.y*l}
        member x.Add y = 
            {x = x.x + y.x |> keepinbounds;y=x.y+y.y |> keepinbounds}
        member x.Project (projection:Projection) = 
            projection.Apply (x.x,x.y ) 