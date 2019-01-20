(function()
{
 "use strict";
 var Global,Viewer,Server,Point,topoJson,topoJsonPointDetails,topoJsonPoint,topoJsonParentObject,Vector,Vector$1,Client,windPoint,WebSharper,Obj,WindInterpolator,SC$1,IntelliFactory,Runtime,Math,Arrays,Collections,Dictionary,JSON,Remoting,AjaxRemotingProvider,UI,Var$1,d3,geo,Submitter,behavior,View,Random,Concurrency,Doc,AttrProxy,Operators;
 Global=self;
 Viewer=Global.Viewer=Global.Viewer||{};
 Server=Viewer.Server=Viewer.Server||{};
 Point=Server.Point=Server.Point||{};
 topoJson=Viewer.topoJson=Viewer.topoJson||{};
 topoJsonPointDetails=topoJson.topoJsonPointDetails=topoJson.topoJsonPointDetails||{};
 topoJsonPoint=topoJson.topoJsonPoint=topoJson.topoJsonPoint||{};
 topoJsonParentObject=topoJson.topoJsonParentObject=topoJson.topoJsonParentObject||{};
 Vector=Viewer.Vector=Viewer.Vector||{};
 Vector$1=Vector.Vector=Vector.Vector||{};
 Client=Viewer.Client=Viewer.Client||{};
 windPoint=Client.windPoint=Client.windPoint||{};
 WebSharper=Global.WebSharper;
 Obj=WebSharper&&WebSharper.Obj;
 WindInterpolator=Client.WindInterpolator=Client.WindInterpolator||{};
 SC$1=Global["StartupCode$websharper-Viewer$Client"]=Global["StartupCode$websharper-Viewer$Client"]||{};
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 Math=Global.Math;
 Arrays=WebSharper&&WebSharper.Arrays;
 Collections=WebSharper&&WebSharper.Collections;
 Dictionary=Collections&&Collections.Dictionary;
 JSON=Global.JSON;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 UI=WebSharper&&WebSharper.UI;
 Var$1=UI&&UI.Var$1;
 d3=Global.d3;
 geo=d3&&d3.geo;
 Submitter=UI&&UI.Submitter;
 behavior=d3&&d3.behavior;
 View=UI&&UI.View;
 Random=WebSharper&&WebSharper.Random;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
 Operators=WebSharper&&WebSharper.Operators;
 Point=Server.Point=Runtime.Class({
  get_Test:function()
  {
   return 1;
  }
 },null,Point);
 Point.New=function(Lat,Long,WindSpeed)
 {
  return new Point({
   Lat:Lat,
   Long:Long,
   WindSpeed:WindSpeed
  });
 };
 topoJsonPointDetails.New=function(type,coordinates)
 {
  return{
   type:type,
   coordinates:coordinates
  };
 };
 topoJsonPoint.New=function(type,geometry)
 {
  return{
   type:type,
   geometry:geometry
  };
 };
 topoJsonParentObject.New=function(type,features)
 {
  return{
   type:type,
   features:features
  };
 };
 Vector$1=Vector.Vector=Runtime.Class({
  Project:function(projection)
  {
   return projection([this.x,this.y]);
  },
  Add:function(y)
  {
   return Vector$1.New(Vector.keepinbounds(this.x+y.x),Vector.keepinbounds(this.y+y.y));
  },
  Multiply:function(l)
  {
   return Vector$1.New(this.x*l,this.y*l);
  },
  Scale:function(l)
  {
   var scale;
   scale=l/this.get_Length();
   return Vector$1.New(this.x*scale,this.y*scale);
  },
  get_Length:function()
  {
   return Math.sqrt(this.x*this.x+this.y*this.y);
  }
 },null,Vector$1);
 Vector$1.FromPolar=function(r,theta)
 {
  return Vector$1.New(r*Math.cos(theta),r*Math.sin(theta));
 };
 Vector$1.New=function(x,y)
 {
  return new Vector$1({
   x:x,
   y:y
  });
 };
 Vector.keepinbounds=function(x)
 {
  return x>180?x-360:x<180?x+360:x;
 };
 windPoint=Client.windPoint=Runtime.Class({
  get_update:function()
  {
   return windPoint.New(this.position.Add(this.velocity.Multiply(1)),this.velocity);
  }
 },null,windPoint);
 windPoint.New=function(position,velocity)
 {
  return new windPoint({
   position:position,
   velocity:velocity
  });
 };
 WindInterpolator=Client.WindInterpolator=Runtime.Class({
  get_Points:function()
  {
   return this.points;
  }
 },Obj,WindInterpolator);
 WindInterpolator.New=Runtime.Ctor(function(points)
 {
  var lats,latmap,longs,longmap,Data;
  Obj.New.call(this);
  lats=Arrays.sort(Arrays.distinct(Arrays.map(function(t)
  {
   return t.Lat;
  },points)));
  latmap=new Dictionary.New$5();
  Arrays.iteri(function(i,t)
  {
   return latmap.Add(t,i);
  },lats);
  longs=Arrays.sort(Arrays.distinct(Arrays.map(function(t)
  {
   return t.Long;
  },points)));
  longmap=new Dictionary.New$5();
  Arrays.iteri(function(i,t)
  {
   return longmap.Add(t,i);
  },longs);
  Data=Arrays.zeroCreate2D(Arrays.length(lats),Arrays.length(longs));
  Arrays.iter(function(p)
  {
   Arrays.set2D(Data,latmap.get_Item(p.Lat),longmap.get_Item(p.Long),Vector$1.New(p.WindSpeed,0));
  },points);
  this.points=Arrays.map(function(t)
  {
   return windPoint.New(Vector$1.New(t.Long/1000,t.Lat/1000),Vector$1.New(t.WindSpeed,1));
  },points);
 },WindInterpolator);
 Client.Main=function()
 {
  var world,rvInput,svg,g,submit,data,vReversed;
  world=JSON.parse((new AjaxRemotingProvider.New()).Sync("websharper-Viewer:Viewer.Server.GetWorld:-1627845098",[]));
  rvInput=Var$1.Create$1("");
  svg=d3.select("#map").append("svg").attr("width",2000).attr("height",500);
  g=svg.append("g");
  g.append("path").datum(Client.topojson().feature(world,world.objects.subunits)).attr("d",geo.path().projection(Client.projection()));
  submit=Submitter.CreateOption(rvInput.get_View());
  data=(new AjaxRemotingProvider.New()).Sync("websharper-Viewer:Viewer.Server.GetWind:1230251650",["Pacific.wind.7days.grb"]);
  geo.path().projection(Client.projection()).pointRadius(2);
  svg.call(behavior.zoom().scaleExtent([1,8]).on("zoom",function()
  {
   g.attr("transform","translate("+d3.event.translate+")scale("+d3.event.scale+")");
  }));
  Arrays.map(function(p)
  {
   return topoJsonPoint.New("Feature",topoJsonPointDetails.New("Point",[p.Long/1000,p.Lat/1000]));
  },data);
  vReversed=View.MapAsync(function(a)
  {
   var interpolator,b,b$1;
   return a!=null&&a.$==1?(new Random.New(),interpolator=Arrays.filter(function()
   {
    return Math.random()<0.1;
   },(new WindInterpolator.New(data)).get_Points()),g.selectAll("line").data(interpolator).enter().append("line").call(Client.Animate),g.selectAll("circle").data(data).enter().append("circle").attr("cx",function(d)
   {
    return((Client.projection())([d.Long/1000,d.Lat/1000]))[0];
   }).attr("cy",function(d)
   {
    return((Client.projection())([d.Long/1000,d.Lat/1000]))[1];
   }).attr("fill",Client.rgb).attr("r",function()
   {
    return"0.3px";
   }),b=null,Concurrency.Delay(function()
   {
    return Concurrency.Return("");
   })):(b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Return("");
   }));
  },submit.view);
  return Doc.Element("div",[],[Doc.Input([],rvInput),Doc.Button("Send",[],function()
  {
   submit.Trigger();
  }),Doc.Element("hr",[],[]),Doc.Element("h4",[AttrProxy.Create("class","text-muted")],[Doc.TextNode("The server responded:")]),Doc.Element("div",[AttrProxy.Create("class","jumbotron")],[Doc.Element("h1",[],[Doc.TextView(vReversed)])])]);
 };
 Client.rgb=function(point)
 {
  var speed;
  speed=Operators.toInt(point.WindSpeed);
  return((((Runtime.Curried(function($1,$2,$3,$4)
  {
   return $1("rgb("+Global.String($2)+","+Global.String($3)+","+Global.String($4)+")");
  },4))(Global.id))(speed))(speed))(speed);
 };
 Client.topojson=function()
 {
  SC$1.$cctor();
  return SC$1.topojson;
 };
 Client.Animate=function(selection)
 {
  var _this;
  function getlength(t)
  {
   var p,sy,sx,p$1,ey,ex;
   p=t.position.Project(Client.projection());
   sy=p[1];
   sx=p[0];
   p$1=t.get_update().position.Project(Client.projection());
   ey=p$1[1];
   ex=p$1[0];
   return Math.sqrt((sx-ex)*(sx-ex)+(sy-ey)*(sy-ey));
  }
  (_this=selection.attr("x1",function(t)
  {
   return(t.position.Project(Client.projection()))[0];
  }).attr("y1",function(t)
  {
   return(t.position.Project(Client.projection()))[1];
  }).attr("x2",function(t)
  {
   return(t.get_update().position.Project(Client.projection()))[0];
  }).attr("y2",function(t)
  {
   return(t.get_update().position.Project(Client.projection()))[1];
  }).attr("stroke","black").attr("stroke-dasharray",function(t)
  {
   var len,c;
   len=(c=getlength(t),Global.String(c));
   return len+" "+len;
  }).attr("stroke-dashoffset",getlength).transition().duration(2000),_this.ease.apply(_this,["linear"])).attr("stroke-dashoffset",0);
 };
 Client.test=function()
 {
  SC$1.$cctor();
  return SC$1.test;
 };
 Client.projection=function()
 {
  SC$1.$cctor();
  return SC$1.projection;
 };
 SC$1.$cctor=function()
 {
  SC$1.$cctor=Global.ignore;
  SC$1.projection=geo.mercator().rotate([180,0,0]);
  SC$1.test=Point.New(1,1,1).get_Test();
  SC$1.topojson=self.topojson;
 };
}());

//# sourceMappingURL=websharper-Viewer.map