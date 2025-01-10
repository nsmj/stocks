# Manipular coisas de PDF.
module Pdf
  def self.extrai_dados_pdf(caminho_arquivo)
    nome_arquivo = File.basename(caminho_arquivo).gsub(".pdf", ".txt")

    system(montar_comando(ENV.fetch("PDF_PASSWORD_1"), nome_arquivo, caminho_arquivo))

    resultado = obter_dados(nome_arquivo)

    if resultado.empty?
      system(montar_comando(ENV.fetch("PDF_PASSWORD_2"), nome_arquivo, caminho_arquivo))
      resultado = obter_dados(nome_arquivo)
    end

    resultado
  end

  private

  def self.montar_comando(senha_pdf, nome_arquivo, caminho_arquivo)
    "mutool convert -p #{senha_pdf} -F text -o txt/#{nome_arquivo} #{caminho_arquivo} 2> /dev/null"
  end

  def self.obter_dados(nome_arquivo)
    File.readlines(File.join("txt", nome_arquivo)).map(&:strip)
  end
end
