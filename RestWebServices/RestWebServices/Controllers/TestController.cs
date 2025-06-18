using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RestWebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("oracle-connect")]
        public async Task<IActionResult> TestOracleConnect()
        {
            var host = "adb.mx-queretaro-1.oraclecloud.com";
            var port = 1522;
            
            try
            {
                using (var client = new TcpClient())
                {
                    var stopwatch = Stopwatch.StartNew();
                    var connectTask = client.ConnectAsync(host, port);
                    
                    if (await Task.WhenAny(connectTask, Task.Delay(10000)) != connectTask)
                    {
                        return Ok(new 
                        { 
                            success = false, 
                            message = "Connection timed out after 10 seconds",
                            host,
                            port
                        });
                    }
                    
                    stopwatch.Stop();
                    
                    if (client.Connected)
                    {
                        return Ok(new 
                        { 
                            success = true, 
                            message = $"Successfully connected to {host}:{port} in {stopwatch.ElapsedMilliseconds}ms",
                            host,
                            port,
                            connectionTimeMs = stopwatch.ElapsedMilliseconds
                        });
                    }
                    
                    return Ok(new 
                    { 
                        success = false, 
                        message = "Connection failed",
                        host,
                        port
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new 
                { 
                    success = false, 
                    message = $"Error: {ex.Message}",
                    host,
                    port,
                    error = ex.ToString()
                });
            }
        }
    }
}
