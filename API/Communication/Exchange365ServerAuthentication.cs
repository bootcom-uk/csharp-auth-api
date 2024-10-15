﻿using API.Interfaces;

namespace API.Communication
{
    public class Exchange365ServerAuthentication : IServerAuthentication
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
