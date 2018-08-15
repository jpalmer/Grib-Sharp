module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch

open Shared

open Fulma
//my additional opens
open Fable.Import //D3
open Fable.Core //needed for U3 used in fable d3

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = { Counter: ResizeArray<Point> option }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| InitialLoad of Result<ResizeArray<Point>, exn>


// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { Counter = None }
    let loadCountCmd =
        Cmd.ofPromise
            (fetchAs<ResizeArray<Point>> "/api/init")
            []
            (Ok >> InitialLoad)
            (Error >> InitialLoad)
    initialModel, loadCountCmd

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel.Counter, msg with
    | _, InitialLoad (Ok initialCount)-> //load initial model
        let nextModel = { Counter = Some initialCount }
        nextModel, Cmd.none
    | _ -> currentModel, Cmd.none
let width = 500.
let height = 100.
let random = new System.Random()
let dataset = Array.init 25 (fun _ -> (random.Next(3,25)))
let barHeight x = x * 5 
let barPadding = 1.
let dataSetLength = float dataset.Length
let svg = Fable.Import.D3.Globals.select("#display")
                                 .append("svg")
                                 .attr("width", U3.Case1 width)
                                 .attr("height", U3.Case1 height)
svg.selectAll("rect")
    .data(dataset)
|> fun x -> (unbox<D3.Selection.Update<int>> x).enter()
|> fun x -> x.append("rect")
|> fun x -> x.attr("width", fun _ _ _ -> U3.Case1 (System.Math.Abs(width / dataSetLength - barPadding)))
                .attr("height", fun data _ _ -> U3.Case1 (float data * 4.))
                .attr("x", fun _ x _ -> U3.Case1 (x * (width/dataSetLength))) 
                .attr("y", fun data _ _ -> U3.Case1 (height - float data * 4.))
                .attr("fill", fun data _ _ -> U3.Case2 (sprintf "rgb(63,%A,150)" (data * 10))) 
|> ignore
            

svg.selectAll("text")
    .data(dataset)
|> fun x -> (unbox<D3.Selection.Update<int>> x).enter()
|> fun x -> x.append("text")
|> fun x -> x.text(fun data _ _ -> U3.Case2 (string data))
             .attr("x", fun _ x _ -> U3.Case1 (x * (width/dataSetLength))) 
             .attr("y", fun data _ _ -> U3.Case1 (height - (float data * 4.)))
|> ignore
let safeComponents =
    let components =
        span [ ]
           [
             a [ Href "https://saturnframework.github.io/docs/" ] [ str "Saturn" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io/elmish/" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://mangelmaxime.github.io/Fulma" ] [ str "Fulma" ]
           ]

    p [ ]
        [ strong [] [ str "SAFE Template" ]
          str " powered by: "
          components ]

let show = function
| { Counter = Some x } -> string x
| { Counter = None   } -> "Loading..."

let AddGribData (m:Model) = 
    sprintf "%i points in Grib file" <| m.Counter.Value.Count 

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let view (model : Model) (dispatch : Msg -> unit) =
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "SAFE Template" ] ] ]

          Container.container []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Heading.h3 [] [ str ("Grib Data " + AddGribData model + svg.ToString()) ] ]
                Columns.columns []
                    [ Column.column [] [ str "-"  ]
                      Column.column [] [ str "+"  ] ] 
              ]
          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ]  ]


#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
