﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class AuthNoSuchUserException : IApplicationException
    {
        public AuthNoSuchUserException() : base("auth", "NoSuchUser")
        {
        }
    }
}
