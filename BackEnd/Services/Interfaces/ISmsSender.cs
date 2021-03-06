﻿using Models.PublicAPI.Requests.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services.Interfaces
{
    interface ISmsSender
    {
        Task SendSMSAsync(AccountCreateRequest model);
    }
}
