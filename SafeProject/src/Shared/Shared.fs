namespace Shared

type Counter = int

//needs functions for total windspeed and direction
type Point = 
    {
        Lat : double; // in millidegrees
        Long : double; // in millidegrees
        UWind : float32 Option; // in m/s
        VWind : float32 Option; // in m/s
    }
    member x.Print =
        sprintf "Lat: %f Long %f UWind %A VWind %A" x.Lat x.Long x.UWind x.VWind