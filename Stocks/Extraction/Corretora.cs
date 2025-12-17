using System.Text.RegularExpressions;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    /// <summary>
    /// Classe abstrata que define a interface para as corretoras.
    /// Cada corretora deve implementar os métodos para extrair dados específicos de suas notas de corretagem.
    /// </summary>
    public abstract class Corretora
    {
        /// <summary>
        /// Extrai a data da nota de corretagem.
        /// </summary>
        /// <param name="dadosNota">Dados extraídos do arquivo PDF</param>
        /// <returns></returns>
        public abstract DateTime ExtrairDataNotaCorretagem(string[] dadosNota);

        /// <summary>
        /// Extrai as operações da nota de corretagem.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dadosNota"></param>
        /// <returns></returns>
        public abstract Task<List<Operacao>> ExtrairOperacoesNotaCorretagem(
            BancoContext db,
            string[] dadosNota
        );

        /// <summary>
        /// Extrai as taxas da nota de corretagem.
        /// </summary>
        /// <param name="dadosNota"></param>
        /// <returns></returns>
        public abstract decimal ExtrairTaxasNotaCorretagem(string[] dadosNota);

        /// <summary>
        /// Expressão regular para extrair os dados do IRRF das operações comuns.
        /// </summary>
        /// <returns></returns>
        public abstract Regex ExpressaoIrrf();

        /// <summary>
        /// Expressão regular para extrair os dados do IRRF das operações day trade.
        /// </summary>
        /// <returns></returns>
        public abstract Regex ExpressaoIrrfDayTrade();

        /// <summary>
        /// Valor de ajuste de posição do IRRF.
        /// </summary>
        /// <returns></returns>
        public abstract int AjustePosicaoIrrf();

        /// <summary>
        /// Fábrica para criar instâncias de corretoras específicas.
        /// </summary>
        /// <param name="nomeCorretora"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Corretora Factory(string nomeCorretora)
        {
            Corretora? objCorretora = nomeCorretora switch
            {
                "Nuinvest" => new Nuinvest(),
                "Clear" => new Clear(),
                _ => null,
            };

            if (objCorretora == null)
            {
                throw new ArgumentException($"Corretora {nomeCorretora} não implementada.");
            }

            return objCorretora;
        }

        /// <summary>
        /// Encontra todos os índices de um array que correspondem a um critério específico.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static int[] FindAllIndex<T>(T[] array, Predicate<T> match)
        {
            // TODO: Ver se é possível usar LINQ para simplificar isso.
            // TODO: Entender essa bagaça.
            return
            [
                .. array
                    .Select((value, index) => match(value) ? index : -1)
                    .Where(index => index != -1),
            ];
        }
    }
}
