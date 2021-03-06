﻿using System;
using Models.PublicAPI.Responses;
using Models.PublicAPI.Responses.Exceptions;

namespace BackEnd.Exceptions
{
    public class ApiLogicException : Exception
    {
        public ExceptionResponse ResponseModel { get; }

        public ApiLogicException(ExceptionResponse responseBase)
            : base(responseBase.Message)
        {
            ResponseModel = responseBase;
        }
        public static ApiLogicException Create(ExceptionResponse response) =>
            new ApiLogicException(response);

        public static ApiLogicException Create(ResponseStatusCode statusCode, string message = null) =>
            new ApiLogicException(new ExceptionResponse(statusCode, message));

        public static implicit operator ApiLogicException(ResponseStatusCode statusCode)
            => Create(statusCode);
    }
}
