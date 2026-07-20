using System.Runtime.InteropServices;

namespace Stocks.Extraction;

public class PdfExtractor
{
    /// <summary>
    /// Extrai os dados do PDF da nota de negociação.
    /// </summary>
    /// <param name="caminhoArquivo"></param>
    /// <param name="senhaArquivo"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public string[] ExtrairDadosPdf(string caminhoArquivo, string senhaArquivo)
    {
        string mutoolPath;
        string arguments;
        string fileName = Path.GetFileName(caminhoArquivo).Replace(".pdf", ".txt");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            mutoolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mutool.exe");
            if (!File.Exists(mutoolPath))
            {
                throw new FileNotFoundException("O arquivo mutool.exe não foi encontrado.");
            }
            arguments = $"convert -p {senhaArquivo} -F text -o {fileName} {caminhoArquivo}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            mutoolPath = "/usr/bin/mutool";
            if (!File.Exists(mutoolPath))
            {
                throw new FileNotFoundException("O arquivo mutool não foi encontrado.");
            }
            arguments =
                $"-c \"mutool convert -p {senhaArquivo} -F text -o {fileName} {caminhoArquivo}\"";
        }
        else
        {
            throw new PlatformNotSupportedException("Sistema operacional não suportado.");
        }

        using (System.Diagnostics.Process pProcess = new())
        {
            pProcess.StartInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? mutoolPath
                : "/bin/bash";
            pProcess.StartInfo.Arguments = arguments;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.RedirectStandardError = true;
            pProcess.Start();
            pProcess.WaitForExit();
        }

        var dadosNota = File.ReadAllLines(fileName);
        File.Delete(fileName);

        return dadosNota;
    }
}
