﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Services.Interfaces;
using Models.PublicAPI.Requests.Account;

namespace BackEnd.Services
{
    public class SmsService : ISmsSender
    {
        const string accountSid = "AC3bfdaa516138e702b0b914bbd46387b4";
        const string authToken = "457c8801110c2bf2e440e3e6ea41bbd5";

        public Task SendSMSAsync(AccountCreateRequest model)
        {
            throw new NotImplementedException();
            //int code = new Random().Next(10000, 90000);
            //TwilioClient.Init(accountSid, authToken);

            //var to = new PhoneNumber(model.PhoneNumber);
            //var message = await MessageResource.CreateAsync(to,
                //from: new PhoneNumber("+14243292698"),
                //body: $"Your verification code: {code}");
        }
    }
}
