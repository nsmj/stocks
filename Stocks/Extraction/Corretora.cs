using System.Text.RegularExpressions;
using Stocks.Data;
using Stocks.Models;

namespace Stocks.Extraction
{
    public abstract class Corretora
    {
        public abstract DateTime ExtrairDataNotaCorretagem(string[] dadosNota);
        public abstract Task<List<Operacao>> ExtrairOperacoesNotaCorretagem(
            BancoContext db,
            string[] dadosNota
        );
        public abstract decimal ExtrairTaxasNotaCorretagem(string[] dadosNota);
        public abstract Regex ExpressaoIrrf();
        public abstract Regex ExpressaoIrrfDayTrade();
        public abstract int AjustePosicaoIrrf();

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

        public static int[] FindAllIndex<T>(T[] array, Predicate<T> match)
        {
            // TODO: Entender essa bagaça.
            return array
                .Select((value, index) => match(value) ? index : -1)
                .Where(index => index != -1)
                .ToArray();
        }
    }
}
