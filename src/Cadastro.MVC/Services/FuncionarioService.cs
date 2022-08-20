﻿using Cadastro.MVC.Interfaces;
using Cadastro.MVC.Models.Request;
using Cadastro.MVC.Models.Response;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadastro.MVC.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly string BaseUrl;
        private readonly JsonSerializerOptions serializerOptions;
        public FuncionarioService(IConfiguration configuration)
        {
            BaseUrl = configuration.GetValue<string>("ServiceUrl");
            serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }
        public async Task<Response<bool>> AtualizarFuncionario(FuncionarioRequest request, string token)
        {
            var result = await $"{BaseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .PatchJsonAsync(request);
            if (result.StatusCode == (int)HttpStatusCode.OK)
                return Response<bool>.SuccessResult(true);
            return Response<bool>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<bool>> CadastrarFuncionario(FuncionarioRequest request, string token)
        {
            var result = await $"{BaseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .PostJsonAsync(request);
            if (result.StatusCode == (int)HttpStatusCode.OK)
                return Response<bool>.SuccessResult(true);
            return Response<bool>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<IEnumerable<FuncionarioResponse>>> ListarFuncionarios(string token)
        {
            var result = await $"{BaseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .GetAsync();
            if (result.StatusCode == (int)HttpStatusCode.OK)
                return Response<IEnumerable<FuncionarioResponse>>
                    .SuccessResult(JsonSerializer.Deserialize<IEnumerable<FuncionarioResponse>>(await result.ResponseMessage.Content.ReadAsStringAsync(), serializerOptions));
            return Response<IEnumerable<FuncionarioResponse>>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }

        public async Task<Response<FuncionarioResponse>> RecuperarFuncionario(Guid id, string token)
        {
            var result = await $"{BaseUrl}/api/v1/Funcionario/funcionario"
                                                                    .AllowAnyHttpStatus()
                                                                    .WithOAuthBearerToken(token)
                                                                    .AppendPathSegment(id)
                                                                    .GetAsync();
            if (result.StatusCode == (int)HttpStatusCode.OK)
            {
                var json = await result.ResponseMessage.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<FuncionarioResponse>(json, serializerOptions);
                return Response<FuncionarioResponse>
                    .SuccessResult(response);
            }
            return Response<FuncionarioResponse>.ErrorResult(result.ResponseMessage.ReasonPhrase);
        }
    }
}