﻿

namespace ChatEjemploMovil.Models
{
    public class MessageModel
    {

        public int Id { get; set; }

        public string Sender { get; set; }

        public string Message { get; set; }

        // Propiedades auxiliares para la vista
        public string Display => $"{Sender}: {Message}";
        public bool IsMine { get; set; }
    }
}
