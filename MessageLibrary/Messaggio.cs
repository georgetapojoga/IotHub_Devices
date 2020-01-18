using System;
using System.ComponentModel.DataAnnotations;

namespace MessageLibrary
{
    public class Messaggio
    {
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string Text { get; set; }

        public int ColorText { get; set; }
    }
}
