(function()
{
 "use strict";
 var Global,websharper_Viewer,Client,topoJsonPointDetails,topoJsonPoint,topoJsonParentObject,SC$1,d3,JSON,WebSharper,Remoting,AjaxRemotingProvider,UI,Var$1,geo,Submitter,behavior,Arrays,View,Concurrency,Doc,AttrProxy;
 Global=self;
 websharper_Viewer=Global.websharper_Viewer=Global.websharper_Viewer||{};
 Client=websharper_Viewer.Client=websharper_Viewer.Client||{};
 topoJsonPointDetails=Client.topoJsonPointDetails=Client.topoJsonPointDetails||{};
 topoJsonPoint=Client.topoJsonPoint=Client.topoJsonPoint||{};
 topoJsonParentObject=Client.topoJsonParentObject=Client.topoJsonParentObject||{};
 SC$1=Global["StartupCode$websharper-Viewer$Client"]=Global["StartupCode$websharper-Viewer$Client"]||{};
 d3=Global.d3;
 JSON=Global.JSON;
 WebSharper=Global.WebSharper;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 UI=WebSharper&&WebSharper.UI;
 Var$1=UI&&UI.Var$1;
 geo=d3&&d3.geo;
 Submitter=UI&&UI.Submitter;
 behavior=d3&&d3.behavior;
 Arrays=WebSharper&&WebSharper.Arrays;
 View=UI&&UI.View;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
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
 Client.Main=function()
 {
  var world,rvInput,svg,g,submit,data,path,TopoJsonData,vReversed;
  function zoomed(u)
  {
   g.attr("transform","translate("+d3.event.Translate+")scale("+d3.event.scale+")");
  }
  world=JSON.parse((new AjaxRemotingProvider.New()).Sync("websharper-Viewer:websharper_Viewer.Server.GetWorld:-383728790",[]));
  rvInput=Var$1.Create$1("");
  svg=d3.select("#map").append("svg").attr("width",500).attr("height",500);
  g=svg.append("g");
  g.append("path").datum(Client.topojson().feature(world,world.objects.subunits)).attr("d",geo.path().projection(Client.projection()));
  submit=Submitter.CreateOption(rvInput.get_View());
  data=(new AjaxRemotingProvider.New()).Sync("websharper-Viewer:websharper_Viewer.Server.GetWind:971175128",["Pacific.wind.7days.grb"]);
  path=geo.path().projection(Client.projection()).pointRadius(2);
  svg.call(function()
  {
   behavior.zoom().scaleExtent([1,8]).on("zoom",zoomed);
  });
  TopoJsonData=topoJsonParentObject.New("FeatureCollection",Arrays.map(function(p)
  {
   return topoJsonPoint.New("Feature",topoJsonPointDetails.New("Point",[p.Long/1000,p.Lat/1000]));
  },data));
  vReversed=View.MapAsync(function(a)
  {
   var b,b$1;
   return a!=null&&a.$==1?(g.append("path").datum(TopoJsonData).attr("d",path),b=null,Concurrency.Delay(function()
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
 Client.projection=function()
 {
  SC$1.$cctor();
  return SC$1.projection;
 };
 Client.topojson=function()
 {
  SC$1.$cctor();
  return SC$1.topojson;
 };
 SC$1.$cctor=function()
 {
  SC$1.$cctor=Global.ignore;
  SC$1.topojson=self.topojson;
  SC$1.projection=geo.mercator();
 };
}());

//# sourceMappingURL=websharper-Viewer.map