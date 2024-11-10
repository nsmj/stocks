class Clear
  def extrai_data_nota_corretagem(dados_nota)
    posicao_data = dados_nota.find_index('Data pregão') + 2

    dados_nota[posicao_data].to_date
  end

  def extrai_operacoes_nota_corretagem(dados_nota)
    operacoes = []

    operacoes_entry = dados_nota.each_index.select { |i| dados_nota[i] == '1-BOVESPA' }

    operacoes_entry.each do |posicao|
      # O que está em
      # noteData[position + 1]
      # deve ser "C" ou "V". No entanto, às vezes é algo como "C FRACIONARIO". Em outras
      # palavras, mistura-se com a informação que deveria vir imediatamente depois.
      # Por causa disso, a verificação abaixo é feita para corrigir o problema quando isso acontece.

      compra = ''
      posicao_nome_ativo = nil
      tipo_mercado = ''

      if dados_nota[posicao + 1].length > 1
        posicao_nome_ativo = posicao + 2
        tipo_mercado = dados_nota[posicao + 1][2..]
        compra = dados_nota[posicao + 1][0]
      else
        posicao_nome_ativo = posicao + 3
        tipo_mercado = dados_nota[posicao + 2]
        compra = dados_nota[posicao + 1]
      end

      match_obj = dados_nota[posicao_nome_ativo].match(/(.*?)(\s{10})(\w{2,6})/)

      tipo_acao = match_obj[3]
      descricao_nota = match_obj[1]

      # Se for uma venda de direito de subscrição, ignore - muitos problemas no IRPF.
      next if tipo_acao == 'DO'

      operacao = Operacao.new

      if ['OPCAO DE COMPRA', 'OPCAO DE VENDA'].include?(tipo_mercado)

        conversion = { 'OPCAO DE COMPRA' => 'CALL', 'OPCAO DE VENDA' => 'PUT' }
        tipo_ativo = AssetType.find_by(nome: conversion[tipo_mercado])

        match_obj_option = dados_nota[posicao_nome_ativo].match(%r{\d{2}/\d{2} \w{5}\d{3}})
        descricao_nota = match_obj_option[0]

        Ativo.find_or_create_by(
          codigo: match_obj_option[0],
          nome: match_obj_option[0],
          descricao_nota: match_obj_option[0],
          tipo_ativo:,
          cnpj: ''
        )
      end

      ativos = Ativo.where(descricao_nota:)

      # Alguns ativos têm o mesmo valor no campo DescricaoNota, por isso nesses casos
      # é necessário filtrar também pelo tipo_acao. Não dá pra filtrar direto pelo
      # tipo_acao porque às vezes o arquivo PDF traz um valor em branco no que deveria
      # ser o tipo_acao.
      ativo = if ativos.length > 1
                ativos.where(tipo_acao:).first
              else
                ativos.first
              end

      operacao.ativo = ativo

      # Compra
      operacao.compra = compra == 'C'

      # Começando a partir de 2 posições após 1-BOVESPA (para pular o campo "C" ou "V"
      # (Compra ou Venda)), vai sequencialmente até encontrar a letra "D" (debito) ou
      # "C" (credito), para então voltar e encontrar os campos QUANTIDADE e PRECO.

      # Como os campos após o nome do ativo são incertos (às vezes tem "observacao",
      # às vezes não), o lugar mais seguro para encontrar os outros campos é "1-BOVESPA" do
      # próximo ativo ou, se não existir (caso estejamos no último ativo), usamos
      # "NOTA DE NEGOCIACAO".

      posicao_temporaria = posicao + 1

      posicao_temporaria += 1 until ['1-BOVESPA',
                                     'NOTA DE NEGOCIAÇÃO',
                                     'NOTA DE CORRETAGEM'].include? dados_nota[posicao_temporaria]

      # Caso haja observações — no momento é usado apenas para indicar quando é Day Trade.
      if posicao_temporaria - 7 == posicao_nome_ativo
        posicao_observacoes = posicao_temporaria - 6
        observacoes = dados_nota[posicao_observacoes].gsub('#', '').gsub('2', '')

        operacao.tipo_operacao = TipoOperacao.find_by(nome: 'Day Trade') if observacoes == 'D'
      end

      operacao.quantidade = dados_nota[posicao_temporaria - 5].gsub('.', '').to_d
      operacao.preco_ativo = dados_nota[posicao_temporaria - 4].gsub(',', '.').to_d
      operacao.valor_total = (operacao.preco_ativo * operacao.quantidade).to_f

      if ativo.tipo_ativo.nome == 'FII'
        operacao.tipo_operacao = TipoOperacao.find_by(nome: 'FII')
      elsif operacao.tipo_operacao != TipoOperacao.find_by(nome: 'Day Trade')
        # Nesse caso não foi marcado como Day Trade acima pelas observações.
        operacao.tipo_operacao = TipoOperacao.find_by(nome: 'Swing Trade')
      end

      operacoes << operacao
    end

    operacoes
  end

  def extrai_taxas_nota_corretagem(dados_nota)
    # TODO: Encapsulate.
    def corrige_pontuacao(value)
      value.gsub(',', '.')
    end

    posicao = dados_nota.index('Taxa de liquidação') - 1
    taxa_liquidacao = corrige_pontuacao(dados_nota[posicao]).to_d.abs

    posicao = dados_nota.index('Taxa de Registro') - 1
    taxa_registro = corrige_pontuacao(dados_nota[posicao]).to_d.abs

    posicao = dados_nota.index('Total Bovespa / Soma') - 1
    bovespa_total = corrige_pontuacao(dados_nota[posicao]).to_d.abs

    data_nota = extract_broker_note_date(dados_nota)

    # Custos Operacionais Totais
    # Até o dia 23/12/2019, estava escrito "Total corretagem" em vez de
    # "Total Custos"

    posicao = if data_nota > '23/12/2019'.to_date
                dados_nota.index('Total Custos / Despesas') - 1
              else
                dados_nota.index('Total corretagem / Despesas') - 1
              end

    custos_operacionais = corrige_pontuacao(dados_nota[posicao]).to_d

    taxa_liquidacao + taxa_registro + bovespa_total + custos_operacionais
  end

  def expressao_irrf
    %r{I.R.R.F. s/ operações, base R\$(.*)}
  end

  def expressao_irrf_day_trade
    /IRRF Day Trade: Base R\$ (.*),(.*) Projeção R\$ (.*)/
  end

  def ajuste_posicao_irrf
    -1
  end
end
