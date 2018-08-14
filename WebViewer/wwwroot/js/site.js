// Write your Javascript code.

//vector field from here: http://bl.ocks.org/newby-jay/767c5ffdbbe43b65902f
//Apache licence

//Will need to construct some sort of interpolator object to get this to work - beyond my JS for now

// Jay Newby 2014
// http://people.mbi.ohio-state.edu/newby.23/
// http://www.math.utah.edu/~newby/
// https://github.com/newby-jay
// Based on the global weather visualization,
// http://earth.nullschool.net/ by Cameron Beccario
//////////////////////////////////////////////////
(function animation(data) {
    // vector field data
    var gamma = 2, c = 10, dt = 0.005,
        X0 = [], Y0 = [], // to store initial starting locations
        X = [], Y = [], // to store current point for each curve
        xb = 1.5, yb = 1.3;
    //// curve ////
    var N = 50, // 25^2 curves
        // discretize the vfield coords
        xp = d3.range(N).map(
            function (i) {
                return xb * (-1 + i * 2 / N);
            }),
        yp = d3.range(N).map(
            function (i) {
                return yb * (-1 + i * 2 / N);
            });
    // array of starting positions for each curve on a uniform grid
    for (var i = 0; i < N; i++) {
        for (var j = 0; j < N; j++) {
            X.push(xp[j]), Y.push(yp[i]);
            X0.push(xp[j]), Y0.push(yp[i]);
        }
    }
    // vfield
    function F(x, y) { //TODO - this should be an interpolator using our existing values that get fed in
        var rho = Math.sqrt(x * x + y * y),
            r = rho - 1,
            a = gamma * (1 - rho * rho),
            b = -gamma * c * r * r + 1;
        return [x * a - b * y, y * a + b * x];
    }
    //// frame setup
    var width = 960,
        height = 500,
        mw = 0, // if we want a margin
        g = d3.select("#animation").node().getContext("2d"); // initialize a "canvas" element
    g.fillStyle = "rgba(0, 0, 0, 0.05)"; // for fading curves
    g.lineWidth = 0.7;
    g.strokeStyle = "#FF8000"; // html color code
    //// mapping from vfield coords to web page coords
    var xMap = d3.scale.linear()
        .domain([-xb, xb])
        .range([mw, width - mw]),
        yMap = d3.scale.linear()
            .domain([-yb, yb])
            .range([height - mw, mw]);
    //// animation setup
    var animAge = 0,
        frameRate = 30, // ms per timestep (yeah I know it's not really a rate)
        M = X.length,
        MaxAge = 100, // # timesteps before restart
        age = [];
    for (var i = 0; i < M; i++) { age.push(randage()); }
    var drawFlag = true;
    // setInterval(function () {if (drawFlag) {draw();}}, frameRate);
    d3.timer(function () { if (drawFlag) { draw(); } }, frameRate);
    d3.select("#animation")
        .on("click", function () { drawFlag = (drawFlag) ? false : true; })
    function randage() {
        // to randomize starting ages for each curve
        return Math.round(Math.random() * 100);
    }
    // for info on the global canvas operations see
    // http://bucephalus.org/text/CanvasHandbook/CanvasHandbook.html#globalcompositeoperation
    g.globalCompositeOperation = "source-over";
    function draw() {
        g.fillRect(0, 0, width, height); // fades all existing curves by a set amount determined by fillStyle (above), which sets opacity using rgba
        for (var i = 0; i < M; i++) { // draw a single timestep for every curve
            var elem = data[i];
            var dr = F(X[i], Y[i]);
            g.beginPath();
            //g.moveTo(xMap(X[i]), yMap(Y[i])); // the start point of the path
            g.moveTo(elem.X, elem.Y); //startpoint
            //g.lineTo(xMap(X[i] += dr[0] * dt), yMap(Y[i] += dr[1] * dt)); // the end point
            g.lineTo(elem.X )
            g.stroke(); // final draw command
            if (age[i]++ > MaxAge) {
                // incriment age of each curve, restart if MaxAge is reached
                age[i] = randage();
                X[i] = X0[i], Y[i] = Y0[i];
            }
        }
    }
})()