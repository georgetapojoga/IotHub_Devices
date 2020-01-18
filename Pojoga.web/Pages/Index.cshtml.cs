using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MessageLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Pojoga.web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public string[] Devices = new string[] { "PojogaDevice1", "PojogaDevice2" };

        [TempData]
        public string Message { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }


        public class InputModel
        {
            [Required]
            public string DeviceId { get; set; }

            public int ColorBackground { get; set; }
            public int ColorText { get; set; }

            public string text { get; set; }
        }


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {

            var registry = RegistryManager.CreateFromConnectionString(_configuration.GetConnectionString("iothub"));
            var twin = await registry.GetTwinAsync(Input.DeviceId);
            twin.Properties.Desired["color"] = Input.ColorBackground ;
            await registry.UpdateTwinAsync(Input.DeviceId, twin, twin.ETag);
            return Page();
        }

        public async Task<IActionResult> OnPostSendMessage()
        {

            if (ModelState.IsValid)
            {
                var serviceClient = ServiceClient.CreateFromConnectionString(
                                        _configuration.GetConnectionString("iothub"),
                                        TransportType.Amqp);
                var messaggioLibrary = new Messaggio();

                messaggioLibrary.DeviceId = Input.DeviceId;
                messaggioLibrary.ColorText = Input.ColorText;
                messaggioLibrary.Text = Input.text;


                var json = JsonConvert.SerializeObject(messaggioLibrary);
                var text = System.Text.UTF8Encoding.UTF8.GetBytes(json);
                var message = new Message(text);

                await serviceClient.SendAsync(Input.DeviceId, message);

                Message = $"Messaggio inviato al dispositivo {Input.DeviceId} con successo";

                return RedirectToPage();
            }


            return Page();
        }
    }
}
