﻿namespace Example.WebAPI.Models
{
    public record class User(string FullName)
    {
        public int Id { get; set; }
        public bool isVerified { get; set; }
    };
}
