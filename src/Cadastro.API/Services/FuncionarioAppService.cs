﻿using Cadastro.API.Interfaces;
using Cadastro.API.Models.Response;
using Cadastro.Domain.Enums;
using Cadastro.Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadastro.API.Services
{
    public class FuncionarioAppService : IFuncionarioAppService
    {
        private readonly IFuncionarioReadRepository _repository;
        private readonly IConnection _connection;
        private readonly ILogger<FuncionarioAppService> _logger;
        private readonly Tracer _trace;
        public FuncionarioAppService(IConnection connection, IFuncionarioReadRepository repository, ILogger<FuncionarioAppService> logger, Tracer trace)
        {
            _repository = repository;
            _connection = connection;
            _logger = logger;
            _trace = trace;
        }

        public bool Cadastrar(Funcionario funcionario)
        {
            using var span = _trace.StartSpan("Cadastrar", SpanKind.Internal);
            try
            {
                using var model = _connection.CreateModel();
                IBasicProperties props = model.CreateBasicProperties();
                props.ContentType = "text/json";
                props.DeliveryMode = 2;
                var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(funcionario));
                model.BasicPublish("cadastro", "cadastrar", props, messageBodyBytes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cadastrar");
                return false;
            }
        }

        public bool Atualizar(Funcionario funcionario, string currentUserId)
        {
            using var span = _trace.StartSpan("Atualizar", SpanKind.Internal);
            try
            {
                using var model = _connection.CreateModel();
                IBasicProperties props = model.CreateBasicProperties();
                props.ContentType = "text/json";
                props.DeliveryMode = 2;                
                var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(funcionario));
                model.BasicPublish("cadastro", "atualizar", props, messageBodyBytes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Atualizar");
                throw;
            }
        }

        public async Task<FuncionarioResponse> ObterPorId(Guid id)
        {
            using var span = _trace.StartSpan("ObterPorId", SpanKind.Internal);
            try
            {
                var connection = _repository.RecuperarConexao();
                var funcionario = await _repository.ObterPorId(null, id);
                if (funcionario == null)
                    return null;
                var enderecos = await _repository.ObterEnderecosPorFuncionarioId(null, id);
                var telefones = await _repository.ObterTelefonesPorFuncionarioId(null, id);

                if (telefones != null && telefones.Any())
                    funcionario.AtualizarTelefones(telefones);

                if (enderecos != null && enderecos.Any())
                {
                    if (enderecos.Any(x => x.TipoEndereco == TipoEnderecoEnum.Comercial))
                        funcionario.AtualizarEnderecoComercial(enderecos.FirstOrDefault(x => x.TipoEndereco == TipoEnderecoEnum.Comercial));

                    if (enderecos.Any(x => x.TipoEndereco == TipoEnderecoEnum.Residencial))
                        funcionario.AtualizarEnderecoResidencial(enderecos.FirstOrDefault(x => x.TipoEndereco == TipoEnderecoEnum.Residencial));
                }
                return new FuncionarioResponse(funcionario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterPorId");
                throw;
            }
        }

        public async Task<IEnumerable<FuncionarioResponse>> ObterTodos()
        {
            using var span = _trace.StartSpan("ObterTodos", SpanKind.Internal);
            try
            {
                var connection = _repository.RecuperarConexao();
                var funcionario = await _repository.ObterTodos(null);
                var result = new List<FuncionarioResponse>();
                foreach (var item in funcionario)
                {
                    result.Add(new FuncionarioResponse(item));
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ObterTodos");
                throw;
            }
        }
    }
}
