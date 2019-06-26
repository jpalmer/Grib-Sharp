namespace Viewer
open Vector
module Interpolator = 
    type Interpolator2D (data : (Vector * Vector)List ) =
        //What do we want - lets build up a list of the x and ycoords
        let xcoords = data |> Seq.map fst |> Seq.groupBy (fun t->t.x) |> Seq.sortBy fst |> toArray 
        let xcoords = data |> Seq.map fst |> Seq.groupBy (fun t->t.y) |> Seq.sortBy fst |> toArray 
        
        member x.Interpolate p = ()
            //So - how to interpolate - plan is just do something linear
            let largerXIndex = xcoords |> Seq.tryFindIndex (fun t -> (fst t).x > p.x)
            let largerYIndex = xcoords |> Seq.tryFindIndex (fun t -> (fst t).y > p.y)
            match largerXIndex, largerYIndex with
            |None,None | Some(0),_, | _,Some(0) -> None
            |Some(xc),Some(yc) -> Some(1.0)
