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

NiftySymbols.StartSymbolLoop();

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
                            NiftySymbols.lastReqTime = DateTime.Now;
                            var result = await WebSearch.Search(symbol);

                            await context.Response.WriteAsync(result);
                        }  );
                        endpoints.MapGet("/api/WebScraping/GetRuntimeNews/{symbol}", async context =>
                        {
                            var symbol = context.Request.RouteValues["symbol"]?.ToString();
                            NiftySymbols.lastReqTime = DateTime.Now;
                            var runtimeNews = await NiftySymbols.GetNews(symbol) ; // Replace with your actual logic
                            await context.Response.WriteAsync(runtimeNews);
                            Console.Write(runtimeNews);

                            Console.Write(""); Console.Write(""); Console.Write(""); Console.Write("");
                            Console.Write(DateTime.Now.ToString("h:mm:ss tt"));
                            Console.Write(""); Console.Write("");
                        });
                    });
                });
            })
            .RunConsoleAsync();