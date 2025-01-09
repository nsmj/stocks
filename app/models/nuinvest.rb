class Nuinvest
  def extrai_data_nota_corretagem(dados_nota)
    posicao_data = dados_nota.find_index('Data Pregão') + 1

    Date.parse(dados_nota[posicao_data])
  end

  def extrai_operacoes_nota_corretagem(dados_nota)
    operacoes = []

    entradas_operacoes = dados_nota.each_index.select { |i| dados_nota[i] == 'BOVESPA' }

    entradas_operacoes.each do |posicao|
      operacao = Operacao.new

      match_obj = dados_nota[posicao + 3].match(/(\w*) (\w{2})/)

      # Compra
      operacao.compra = dados_nota[posicao + 1] == 'C'

      # Ativo
      codigo_ativo = match_obj[1]

      # Remove o "F" no final do nome do ativo, se houver.
      codigo_ativo = codigo_ativo[..-2] if codigo_ativo[-1] == 'F'

      ativo = Ativo.find_by(codigo: codigo_ativo)

      operacao.ativo = ativo

      # Começando a partir de 2 posições após BOVESPA (para pular
      # o campo C ou V (Compra ou Venda). Vai sequencialmente
      # até encontrar uma linha em branco, então começa a voltar
      # para encontrar QUANTIDADE e PREÇO

      posicao_temp = posicao + 2

      posicao_temp += 1 while dados_nota[posicao_temp] != ''

      operacao.quantidade = dados_nota[posicao_temp - 4].to_d
      operacao.preco_ativo = dados_nota[posicao_temp - 3].gsub(',', '.').to_d
      operacao.valor_total = (operacao.preco_ativo * operacao.quantidade).to_f

      operacao.tipo_operacao = if ativo.tipo_ativo.nome == 'FII'
                                 TipoOperacao.find_by(nome: 'FII')
                               elsif dados_nota[posicao_temp - 5] == 'D'
                                 TipoOperacao.find_by(nome: 'Day Trade')
                               else
                                 TipoOperacao.find_by(nome: 'Swing Trade')
                               end

      operacoes << operacao
    end

    operacoes
  end

  def extrai_taxas_nota_corretagem(dados_nota)
    # TODO: Encapsular.
    def corrigir_pontuacao(valor)
      valor.gsub(',', '.')
    end

    posicao = dados_nota.index('Taxa de Liquidação') + 1
    taxa_liquidacao = corrigir_pontuacao(dados_nota[posicao]).to_d.abs

    posicao = dados_nota.index('Taxa de Registro') + 1
    taxa_registro = corrigir_pontuacao(dados_nota[posicao]).to_d.abs

    posicao = dados_nota.index('Total Bolsa') + 2
    total_bovespa = corrigir_pontuacao(dados_nota[posicao]).to_d.abs

    posicao = dados_nota.index('Total Corretagem/Despesas') + 2
    custos_operacionais = corrigir_pontuacao(dados_nota[posicao]).to_d.abs

    taxa_liquidacao + taxa_registro + total_bovespa + custos_operacionais
  end

  def expressao_irrf
    %r{I.R.R.F. s/ operacoes. Base (.*)}
  end

  def expressao_irrf_day_trade
    /klfsadjlkdsajlkfasd/ # TODO: Isso ainda não está implementado.
  end

  def ajuste_posicao_irrf
    1
  end
end
