namespace WebSharper.AspNetCore.Tests

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open WebSharper.AspNetCore        
open WebSharper.AspNetCore.WebSocket


module Startup = 
    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)
        // Add services to the container.
        builder.Services.AddWebSharper()
            .AddSitelet<Website.MyWebsite>()
            .AddAuthentication("WebSharper")
            .AddCookie("WebSharper", fun options -> ())
        |> ignore

        let app = builder.Build()

        // Configure the HTTP request pipeline.
        if not (app.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Error")
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                .UseHsts()
            |> ignore

        if builder.Environment.IsDevelopment() then app.UseDeveloperExceptionPage() |> ignore

        app.UseHttpsRedirection()
            .UseAuthentication()
            .UseWebSockets()
            .UseWebSharper(fun ws ->
                ws.UseWebSocket("ws", fun wsws -> 
                    wsws.Use(WebSocketServer.Start())
                        .JsonEncoding(JsonEncoding.Readable)
                    |> ignore
                )
                |> ignore
            )
            .UseStaticFiles()
            .Run(fun context ->
                context.Response.StatusCode <- 404
                context.Response.WriteAsync("Fell through :("))

        app.Run()

        0 // Exit code
