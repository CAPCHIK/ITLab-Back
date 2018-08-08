﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using BackEnd.DataBase;
using BackEnd.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.People;
using Models.PublicAPI.Responses;
using Models.PublicAPI.Responses.General;
using Models.PublicAPI.Responses.People;
using Extensions;
using Newtonsoft.Json;
using BackEnd.Services.Interfaces;

namespace BackEnd.Controllers.Users
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : AuthorizeController
    {
        private readonly DataBaseContext dbContext;
        private readonly IUserRegisterTokens registerTokens;

        public UserController(UserManager<User> userManager,
                              DataBaseContext dbContext,
                              IUserRegisterTokens registerTokens) : base(userManager)
        {
            this.dbContext = dbContext;
            this.registerTokens = registerTokens;
        }
        [HttpGet]
        public async Task<ListResponse<UserView>> GetAsync(
            string email,
            string firstname,
            string lastname,
            int count = 5)
            => await userManager
                .Users
                .IfNotNull(email, users => users.Where(u => u.Email.ToUpper().Contains(email.ToUpper())))
                .IfNotNull(firstname, users => users.Where(u => u.FirstName.ToUpper().Contains(firstname.ToUpper())))
                .IfNotNull(lastname, users => users.Where(u => u.LastName.ToUpper().Contains(lastname.ToUpper())))
                .ResetToDefault(c => c <= 0, ref count, 5)
                .If(count > 0, users => users.Take(count))
                .ProjectTo<UserView>()
                .ToListAsync();

        [HttpGet("{id}")]
        public async Task<OneObjectResponse<UserView>> GetAsync(Guid id)
        => await userManager
            .Users
            .ProjectTo<UserView>()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw ResponseStatusCode.NotFound.ToApiException();

        [HttpPost]
        public OneObjectResponse<string> InviteUser([FromBody]string email)
        {
            return registerTokens.AddRegisterToken(email);
        }
    }
}