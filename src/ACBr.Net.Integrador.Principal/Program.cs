using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACBr.Net.Integrador.Principal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ACBrIntegrador integrador = new ACBrIntegrador();
            integrador.Configuracoes.ChaveAcessoValidador = "25CFE38D-3B92-46C0-91CA-CFF751A82D3D";
            bool adicionarNumeroSessao = true;

            integrador.NomeComponente = "MF-e";
            integrador.NomeMetodo = "ConsultarSAT";
            

            IntegradorRetorno retorno =  integrador.Enviar(adicionarNumeroSessao);
            
        }
    }
}
