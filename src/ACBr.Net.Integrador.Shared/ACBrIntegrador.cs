﻿// ***********************************************************************
// Assembly         : ACBr.Net.Integrador
// Author           : RFTD
// Created          : 02-19-2018
//
// Last Modified By : RFTD
// Last Modified On : 02-19-2018
// ***********************************************************************
// <copyright file="ACBrIntegrador.cs" company="ACBr.Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2016 Grupo ACBr.Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	 The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using ACBr.Net.Core;
using ACBr.Net.Core.Exceptions;
using ACBr.Net.Core.Extensions;
using ACBr.Net.Core.Logging;
using ACBr.Net.Integrador.Events;

#if !NETSTANDARD2_0
using System.Drawing;
#endif

namespace ACBr.Net.Integrador
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Classe ACBrIntegrador, responsavel por comunicar com o integrador fiscal do Ceará.
    /// </summary>
    /// <seealso cref="T:ACBr.Net.Core.ACBrComponent" />
    /// <seealso cref="T:ACBr.Net.Core.Logging.IACBrLog" />
#if !NETSTANDARD2_0
    [ToolboxBitmap(typeof(ACBrIntegrador), "ACBr.Net.Integrador.ACBrIntegrador")]
#endif

    public sealed class ACBrIntegrador : ACBrComponent, IACBrLog
    {
        #region Fields

        private const string VFPName = "VFP-e";

        #endregion Fields

        #region Events

        /// <summary>
        /// Ocorre que é necessario pegar o número da sessão.
        /// </summary>
        public event EventHandler<NumeroSessaoEventArgs> OnGetNumeroSessao;

        #endregion Events

        #region Properties

        /// <summary>
        /// Configurações para comunicação do integrador.
        /// </summary>
        [Category("Configurações")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IntegradorConfig Configuracoes { get; private set; }

        /// <summary>
        /// Retorna o número da sessão atual.
        /// </summary>
        [Browsable(false)]
        public int NumeroSessao { get; private set; }

        /// <summary>
        /// Define/retorna o nome do componente que vai ser utilizado.
        /// </summary>
        [Category("Configurações")]
        public string NomeComponente { get; set; }

        /// <summary>
        /// Define/retorna o nome do metodo que vai ser utilizado.
        /// </summary>
        [Category("Configurações")]
        public string NomeMetodo { get; set; }

        /// <summary>
        /// Parametros do metodo.
        /// </summary>
        [Browsable(false)]
        public IntegradorParametroCollection Parametros { get; private set; }

        /// <summary>
        /// Retorna o comando enviado para o integrador fiscal.
        /// </summary>
        [Browsable(false)]
        public string ComandoEnviado { get; internal set; }

        /// <summary>
        /// Retorna a última resposta do integrador fiscal.
        /// </summary>
        //[Browsable(false)]
        //public string UltimaResposta { get; internal set; }

        #endregion Properties

        #region Methods

        #region Public

        /// <summary>
        /// Consulta a sessão informada do integrador.
        /// </summary>
        /// <param name="numeroSessao">Número da sessão para consultar.</param>
        /// <returns></returns>
        public IntegradorRetorno ConsultarNumeroSessaoIntegrador(int numeroSessao)
        {
            NomeComponente = "ConsultaNumeroSessao";
            NomeMetodo = "numeroSessao";

            Parametros.Clear();
            Parametros.AddParametro("numeroSessao", numeroSessao.ToString());
            return Enviar(false);
        }

        /// <summary>
        /// Enviar as informações de pagamento para o integrador fiscal.
        /// </summary>
        /// <param name="chaveRequisicao"></param>
        /// <param name="estabelecimento"></param>
        /// <param name="serialPOS"></param>
        /// <param name="cnpj"></param>
        /// <param name="icmsBase"></param>
        /// <param name="valorTotalVenda"></param>
        /// <param name="origemPagamento"></param>
        /// <param name="habilitarMultiplosPagamentos"></param>
        /// <param name="habilitarControleAntiFraude"></param>
        /// <param name="codigoMoeda"></param>
        /// <param name="emitirCupomNFCE"></param>
        /// <returns></returns>
        public IntegradorRetorno EnviarPagamento(string chaveRequisicao, string estabelecimento, string serialPOS, string cnpj, decimal icmsBase, decimal valorTotalVenda, string origemPagamento,
            bool habilitarMultiplosPagamentos = true, bool habilitarControleAntiFraude = false, string codigoMoeda = "BRL", bool emitirCupomNFCE = false)
        {
            NomeComponente = VFPName;
            NomeMetodo = "EnviarPagamento";

            Parametros.Clear();
            Parametros.AddParametro("chaveAcessoValidador", Configuracoes.ChaveAcessoValidador);
            Parametros.AddParametro("ChaveRequisicao", chaveRequisicao);
            Parametros.AddParametro("Estabelecimento", estabelecimento);
            Parametros.AddParametro("SerialPOS", serialPOS);
            Parametros.AddParametro("Cnpj", cnpj);
            Parametros.AddParametro("IcmsBase", $"{icmsBase:0.00}".Replace('.', ','));
            Parametros.AddParametro("ValorTotalVenda", $"{valorTotalVenda:0.00}".Replace('.', ','));
            Parametros.AddParametro("HabilitarMultiplosPagamentos", habilitarMultiplosPagamentos ? "true" : "false");
            Parametros.AddParametro("HabilitarControleAntiFraude", habilitarControleAntiFraude ? "true" : "false");
            Parametros.AddParametro("CodigoMoeda", codigoMoeda);
            Parametros.AddParametro("OrigemPagamento", origemPagamento);
            Parametros.AddParametro("EmitirCupomNFCE", emitirCupomNFCE ? "true" : "false");
            return Enviar();
        }

        /// <summary>
        /// Verifica a situação do validador.
        /// </summary>
        /// <param name="idFila"></param>
        /// <param name="cnpj"></param>
        /// <returns></returns>
        public IntegradorRetorno VerificarStatusValidador(int idFila, string cnpj)
        {
            NomeComponente = VFPName;
            NomeMetodo = "VerificarStatusValidador";

            Parametros.Clear();
            Parametros.AddParametro("chaveAcessoValidador", Configuracoes.ChaveAcessoValidador);
            Parametros.AddParametro("idFila", idFila.ToString());
            Parametros.AddParametro("cnpj", cnpj);

            return Enviar();
        }

        /// <summary>
        /// Envia a situação do pagamento.
        /// </summary>
        /// <param name="codigoAutorizacao"></param>
        /// <param name="bin"></param>
        /// <param name="donoCartao"></param>
        /// <param name="dataExpiracao"></param>
        /// <param name="instituicaoFinanceira"></param>
        /// <param name="parcelas"></param>
        /// <param name="codigoPagamento"></param>
        /// <param name="valorPagamento"></param>
        /// <param name="idFila"></param>
        /// <param name="tipo"></param>
        /// <param name="ultimosQuatroDigitos"></param>
        /// <returns></returns>
        public IntegradorRetorno EnviarStatusPagamento(string codigoAutorizacao, string bin, string donoCartao,
            string dataExpiracao, string instituicaoFinanceira, int parcelas, string codigoPagamento, decimal valorPagamento, int idFila, string tipo, int ultimosQuatroDigitos)
        {
            NomeComponente = VFPName;
            NomeMetodo = "EnviarStatusPagamento";

            Parametros.Clear();
            Parametros.AddParametro("chaveAcessoValidador", Configuracoes.ChaveAcessoValidador);
            Parametros.AddParametro("CodigoAutorizacao", codigoAutorizacao);
            Parametros.AddParametro("Bin", bin);
            Parametros.AddParametro("DonoCartao", donoCartao);
            Parametros.AddParametro("DataExpiracao", dataExpiracao);
            Parametros.AddParametro("InstituicaoFinanceira", instituicaoFinanceira);
            Parametros.AddParametro("Parcelas", parcelas.ToString());
            Parametros.AddParametro("CodigoPagamento", codigoPagamento);
            Parametros.AddParametro("ValorPagamento", $"{valorPagamento:0.00}".Replace('.', ','));
            Parametros.AddParametro("IdFila", idFila.ToString());
            Parametros.AddParametro("Tipo", tipo);
            Parametros.AddParametro("UltimosQuatroDigitos", ultimosQuatroDigitos.ToString());

            return Enviar();
        }

        /// <summary>
        /// Pega a resposta fiscal do pagamento.
        /// </summary>
        /// <param name="idFila"></param>
        /// <param name="chaveAcesso"></param>
        /// <param name="nsu"></param>
        /// <param name="numeroAprovacao"></param>
        /// <param name="bandeira"></param>
        /// <param name="adquirinte"></param>
        /// <param name="cnpj"></param>
        /// <param name="impressaofiscal"></param>
        /// <param name="numeroDocumento"></param>
        /// <returns></returns>
        public IntegradorRetorno RespostaFiscal(int idFila, string chaveAcesso, string nsu,
            string numeroAprovacao, string bandeira, string adquirinte, string cnpj, string impressaofiscal, string numeroDocumento)
        {
            NomeComponente = VFPName;
            NomeMetodo = "EnviarStatusPagamento";

            Parametros.Clear();
            Parametros.AddParametro("chaveAcessoValidador", Configuracoes.ChaveAcessoValidador);
            Parametros.AddParametro("idFila", idFila.ToString());
            Parametros.AddParametro("ChaveAcesso", chaveAcesso);
            Parametros.AddParametro("Nsu", nsu);
            Parametros.AddParametro("NumerodeAprovacao", numeroAprovacao);
            Parametros.AddParametro("Bandeira", bandeira);
            Parametros.AddParametro("Adquirente", adquirinte);
            Parametros.AddParametro("Cnpj", cnpj);
            Parametros.AddParametro("ImpressaoFiscal", impressaofiscal);
            Parametros.AddParametro("NumeroDocumento", numeroDocumento);

            return Enviar();
        }

        /// <summary>
        /// Envia o comando para o integrador ficasl.
        /// </summary>
        /// <param name="adicionarNumeroSessao">Se adicionar o número da sessão nos parametros.</param>
        /// <returns></returns>
        public IntegradorRetorno Enviar(bool adicionarNumeroSessao = true)
        {
            Guard.Against<ArgumentNullException>(NomeComponente.IsEmpty(), "Componente não definido.");
            Guard.Against<ArgumentNullException>(NomeMetodo.IsEmpty(), "Metodo não definido.");

            var envio = NovoEnvio();
            if (adicionarNumeroSessao)
            {
                GerarNumeroSessao();
                Parametros.InsertParametro(0, "numeroSessao", NumeroSessao.ToString());
            }

            envio.Componente.Metodo.Parametros.AddRange(Parametros);

            EnviarComando(envio);

            return IntegradorRetorno.Load(AguardarResposta(envio.Identificador.Valor));
        }

        /// <summary>
        /// Gera um novo número de sessão.
        /// </summary>
        public void GerarNumeroSessao()
        {
            NumeroSessao = StaticRandom.Next(1, 999999);

            var e = new NumeroSessaoEventArgs(NumeroSessao);
            OnGetNumeroSessao.Raise(this, e);
            NumeroSessao = e.Sessao;
        }

        #endregion Public

        #region Private

        private IntegradorEnvio NovoEnvio()
        {
            var envio = new IntegradorEnvio
            {
                Identificador = { Valor = Guid.NewGuid().ToString() },
                Componente =
                {
                    Nome = NomeComponente,
                    Metodo = {Nome = NomeMetodo }
                }
            };

            return envio;
        }

        private void EnviarComando(IntegradorEnvio envio)
        {
            if (!Directory.Exists(Path.Combine(Configuracoes.PastaInput, "Enviados")))
                Directory.CreateDirectory(Path.Combine(Configuracoes.PastaInput, "Enviados"));

            envio.Save(Path.Combine(Configuracoes.PastaInput, "Enviados", $"{envio.Componente.Metodo.Nome}_{envio.Identificador.Valor}.xml"));

            var file = Path.Combine(Configuracoes.PastaInput, $"{envio.Componente.Metodo.Nome}_{envio.Identificador.Valor}.tmp");
            envio.Save(file);

            ComandoEnviado = File.ReadAllText(file);

            File.Move(file, $"{file.Substring(0, file.Length - 4)}.xml");
        }

        private string AguardarResposta(string identificacao)
        {
            if (!Directory.Exists(Path.Combine(Configuracoes.PastaOutput, "Processados")))
                Directory.CreateDirectory(Path.Combine(Configuracoes.PastaOutput, "Processados"));

            var resposta = string.Empty;

            var timeLimit = DateTime.Now.AddMilliseconds(Configuracoes.TimeOut);
            var identificacaoXml = $"<Valor>{identificacao}</Valor>";

            do
            {
                Thread.Sleep(500);

                var files = Directory.GetFiles(Configuracoes.PastaOutput, "*.xml");
                if (files.Length < 1) continue;

                foreach (var file in files)
                {
                    try
                    {
                        var resp = File.ReadAllText(file);
                        if (!resp.Contains(identificacaoXml)) continue;

                        resposta = resp;
                        File.Move(file, Path.Combine(Configuracoes.PastaOutput, "Processados", $"{new FileInfo(file).Name}.xml"));
                        break;
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
                
                Guard.Against<TimeoutException>(Configuracoes.TimeOut > 0 && resposta.IsEmpty() && DateTime.Now >= timeLimit);

            } while (resposta.IsEmpty());

            return resposta;
        }

        #endregion Private

        #region Override

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            Configuracoes = new IntegradorConfig();
            Parametros = new IntegradorParametroCollection();
            NomeComponente = "";
            NomeMetodo = "";
        }

        /// <inheritdoc />
        protected override void OnDisposing()
        {
        }

        #endregion Override

        #endregion Methods
    }
}