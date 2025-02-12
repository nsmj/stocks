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
            switch (nomeCorretora)
            {
                case "Nuinvest":
                    return new Nuinvest();
                case "Clear":
                    return new Clear();
                default:
                    return null; // FIXME: Tirar esse NULL
            }
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
