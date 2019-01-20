namespace Viewer
open WebSharper
[<JavaScript>]
module topoJson =
    //Some types so that we can work with topojson
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