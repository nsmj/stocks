class ProcessadorNotasCorretagem < ApplicationService
  def call
    Dir.glob('files/BrokerNotes/**/*').each do |caminho_arquivo|
      next if File.directory? caminho_arquivo

      corretora = NotaCorretagem.get_corretora(caminho_arquivo)

      nota_corretagem = NotaCorretagem.new(corretora)

      nota_corretagem.extrai_dados_arquivo(caminho_arquivo)

      nota_corretagem.operacoes.each do |t|
        raise StandardError, t.errors unless t.save
      end

      nota_corretagem.irrfs.each do |i|
        raise StandardError, i.errors unless i.save
      end
    end
  end
end
