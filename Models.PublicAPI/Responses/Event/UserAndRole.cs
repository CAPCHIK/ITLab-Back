﻿using System;
using System.Collections.Generic;
using System.Text;
using Models.PublicAPI.Responses.People;

namespace Models.PublicAPI.Responses.Event
{
    public class UserAndRole
    {
        public UserView User { get; set; }
        public RoleView Role { get; set; }
    }
}
