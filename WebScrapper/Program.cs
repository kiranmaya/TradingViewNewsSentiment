//AIzaSyB8dvt-KEDokb3Hb_aoXmVEvITA2Txx0gs
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using System.Drawing;
using System;
using WebScrapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;



Console.WriteLine("Started");


//await RestTest.CallFlaskApiAsync("This is good ");

await WebSearch.Search("HEROMOTOCO");

await Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://localhost:2002");
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/api/WebScraping/GetNews/{symbol}", async context =>
                        {
                            //http://localhost:2002/api/WebScraping/GetNews/AXISBANK
                            var symbol = context.Request.RouteValues["symbol"]?.ToString();
                            var result = await WebSearch.Search(symbol);
                            await context.Response.WriteAsync(result);
                        });
                    });
                });
            })
            .RunConsoleAsync();