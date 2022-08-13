﻿using System;
using System.Collections.Generic;

namespace Cadastro.API.Models.Response
{
    public class FuncionarioResponse
    {
        public FuncionarioResponse()
        {
            Telefones = new List<TelefoneResponse>();
        }
        public string UserId { get; set; }
        public string Matricula { get; set; }
        public string Cargo { get; set; }
        public string Nome { get; set; }
        public string SobreNome { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public List<TelefoneResponse> Telefones { get; set; }
        public EnderecoResponse EnderecoComercial { get; set; }
        public EnderecoResponse EnderecoResidencial { get; set; }
        public bool Ativo { get; set; }
    }
}
