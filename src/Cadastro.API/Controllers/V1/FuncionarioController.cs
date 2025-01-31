﻿using Cadastro.API.Interfaces;
using Cadastro.API.Models.Request;
using Cadastro.API.Models.Response;
using Domain.Entities;
using Domain.ValueObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cadastro.API.Controllers.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FuncionarioController : ControllerBase
    {
        private readonly ILogger<FuncionarioController> _logger;
        private readonly IFuncionarioAppService _service;
        public FuncionarioController(ILogger<FuncionarioController> logger, IFuncionarioAppService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        [Route("funcionario")]
        [SwaggerResponse(200, "Funcionarios localizado", typeof(IEnumerable<FuncionarioResponse>))]
        [SwaggerResponse(400, "Funcionarios não localizado")]
        public async Task<IActionResult> Get([FromHeader] Guid correlationId)
        {
            try
            {
                var result = await _service.ObterTodos();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get");
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("funcionario/{id:guid}")]
        [SwaggerResponse(200, "Funcionario localizado", typeof(FuncionarioResponse))]
        [SwaggerResponse(400, "Funcionario não localizado")]
        public async Task<IActionResult> Get([FromHeader] Guid correlationId, Guid id)
        {
            try
            {
                var result = await _service.ObterPorId(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get");
                return BadRequest(ex);
            }

        }

        [HttpPost]
        [Route("funcionario")]
        [SwaggerResponse(200, "Funcionario recebido", typeof(bool))]
        [SwaggerResponse(400, "Funcionario não recebido")]
        public IActionResult Post([FromHeader] Guid correlationId, [FromBody] FuncionarioRequest funcionario)
        {
            try
            {
                var tels = new List<Telefone>();
                if (funcionario.Telefones != null && funcionario.Telefones.Any())
                    foreach (var telefone in funcionario.Telefones)
                        tels.Add(new Telefone(telefone.DDI, telefone.Telefone[..2], telefone.Telefone[2..], Guid.Parse(funcionario.UserId)));

                var result = _service.Cadastrar(new Funcionario(funcionario.UserId,
                                                                funcionario.Matricula,
                                                                funcionario.Cargo,
                                                                new Nome(funcionario.Nome, funcionario.SobreNome),
                                                                new DataNascimento(funcionario.DataNascimento),
                                                                new Email(funcionario.Email), tels,
                                                                new Endereco(funcionario.EnderecoResidencial?.Rua,
                                                                             funcionario.EnderecoResidencial?.Numero,
                                                                             funcionario.EnderecoResidencial?.CEP,
                                                                             funcionario.EnderecoResidencial?.Complemento,
                                                                             funcionario.EnderecoResidencial?.Bairro,
                                                                             funcionario.EnderecoResidencial?.Cidade,
                                                                             funcionario.EnderecoResidencial?.UF,
                                                                             Domain.Enums.TipoEnderecoEnum.Residencial,
                                                                             Guid.Parse(funcionario.UserId)),
                                                                new Endereco(funcionario.EnderecoComercial?.Rua,
                                                                             funcionario.EnderecoComercial?.Numero,
                                                                             funcionario.EnderecoComercial?.CEP,
                                                                             funcionario.EnderecoComercial?.Complemento,
                                                                             funcionario.EnderecoComercial?.Bairro,
                                                                             funcionario.EnderecoComercial?.Cidade,
                                                                             funcionario.EnderecoComercial?.UF,
                                                                             Domain.Enums.TipoEnderecoEnum.Comercial,
                                                                             Guid.Parse(funcionario.UserId))));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post");
                return BadRequest(ex);
            }

        }

        [HttpPatch]
        [Route("funcionario")]
        [SwaggerResponse(200, "Funcionario recebido", typeof(bool))]
        [SwaggerResponse(400, "Funcionario não recebido")]
        public IActionResult Patch([FromHeader] Guid correlationId, [FromBody] FuncionarioRequest funcionario)
        {
            try
            {
                var tels = new List<Telefone>();
                if (funcionario.Telefones != null && funcionario.Telefones.Any())
                    foreach (var telefone in funcionario.Telefones)
                        tels.Add(new Telefone(telefone.Id, telefone.DDI, telefone.Telefone[..2], telefone.Telefone[2..], Guid.Parse(funcionario.UserId)));

                var funcionarioModel = new Funcionario(funcionario.UserId,
                                                                funcionario.Matricula,
                                                                funcionario.Cargo,
                                                                new Nome(funcionario.Nome, funcionario.SobreNome),
                                                                new DataNascimento(funcionario.DataNascimento),
                                                                new Email(funcionario.Email), tels,
                                                                new Endereco(funcionario.EnderecoResidencial?.Id ?? 0,
                                                                             funcionario.EnderecoResidencial?.Rua,
                                                                             funcionario.EnderecoResidencial?.Numero,
                                                                             funcionario.EnderecoResidencial?.CEP,
                                                                             funcionario.EnderecoResidencial?.Complemento,
                                                                             funcionario.EnderecoResidencial?.Bairro,
                                                                             funcionario.EnderecoResidencial?.Cidade,
                                                                             funcionario.EnderecoResidencial?.UF,
                                                                             Domain.Enums.TipoEnderecoEnum.Residencial,
                                                                             Guid.Parse(funcionario.UserId)),
                                                                new Endereco(funcionario.EnderecoComercial?.Id ?? 0,
                                                                             funcionario.EnderecoComercial?.Rua,
                                                                             funcionario.EnderecoComercial?.Numero,
                                                                             funcionario.EnderecoComercial?.CEP,
                                                                             funcionario.EnderecoComercial?.Complemento,
                                                                             funcionario.EnderecoComercial?.Bairro,
                                                                             funcionario.EnderecoComercial?.Cidade,
                                                                             funcionario.EnderecoComercial?.UF,
                                                                             Domain.Enums.TipoEnderecoEnum.Comercial,
                                                                             Guid.Parse(funcionario.UserId)));

                var result = _service.Atualizar(funcionarioModel, string.Empty);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Patch");
                return BadRequest(ex);
            }
        }
    }
}
