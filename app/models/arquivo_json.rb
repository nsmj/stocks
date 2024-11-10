class ArquivoJson
  def extrai_dados_arquivo(caminho_arquivo)
    conteudo_json = JSON.parse(File.read(caminho_arquivo))

    resultado = []

    conteudo_json['operacoes']&.each do |t|
      resultado << Operacao.from_json(t)
    end

    conteudo_json['eventos']&.each do |e|
      resultado << Evento.from_json(e)
    end

    resultado
  end
end
