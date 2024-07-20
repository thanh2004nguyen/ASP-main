using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Group5.Data;

namespace Group5.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime? Timestamp { get; set; }
        public Employee? FromUser { get; set; }
        public int? ToRoomId { get; set; }
        [ForeignKey("ToRoomId")]
        public Room? ToRoom { get; set; }
        public bool Seen { get; set; }
    }
}
