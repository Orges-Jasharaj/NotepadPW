var builder = DistributedApplication.CreateBuilder(args);

//var seq = builder.AddSeq("seq")
//    .WithEnvironment("ACCEPT_EULA", "Y")
//    .WithEndpoint("http", o =>
//    {
//        o.Port = 5341;
//        o.TargetPort = 80;
//        o.UriScheme = "http";
//        o.IsExternal = true;

//    });


var mailput = builder.AddMailPit("mailpit", 8025, 1025).WithDataVolume("mailpit-data");

var api = builder.AddProject<Projects.DemoProject_API>("api",launchProfileName:"https");
var client = builder.AddProject<Projects.DemoProject_Client>("client", launchProfileName: "https")
    .WaitFor(api);

builder.Build().Run();
