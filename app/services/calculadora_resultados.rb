class CalculadoraResultados < ApplicationService
  def call
    sql = <<~SQL
      SELECT t.id, 'Operacao' AS tipo, data, null AS 'fator', quantidade, preco_ativo, valor_total, compra, taxas, a.id AS ativo_id, tt.nome AS 'tipo_operacao_evento' FROM trade t
      LEFT JOIN asset a ON t.asset_id = a.id
      LEFT JOIN trade_type tt ON t.trade_type_id = tt.id
      UNION ALL
      SELECT e.id, 'Evento', data, fator, null, valor AS 'preco_ativo', null, null, null, a.id AS ativo_id, et.nome FROM event e
      LEFT JOIN asset a ON e.asset_id = a.id
      LEFT JOIN event_type et ON e.event_type_id = et.id
      ORDER BY ativo_id, data
    SQL

    linhas = ActiveRecord::Base.connection.execute(sql)

    resultado_organizado = {}
    posicoes_fim_ano = {}

    linhas.each do |linha|
      resultado_organizado[linha['ativo_id']] = [] unless resultado_organizado.key? linha['ativo_id']
      resultado_organizado[linha['ativo_id']] << linha
    end

    resultado_organizado.each do |ativo_id, operacoes|
      ultimo_preco_medio = 0
      posicao_anterior = 0
      posicao_atual = 0
      preco_medio_compra = 0

      operacoes.each do |operacao|
        if operacao['tipo'] == 'Operacao'

          if operacao['compra'] == 1
            posicao_atual = posicao_anterior + operacao['quantidade']
            preco_medio_compra = (((ultimo_preco_medio * posicao_anterior) + operacao['valor_total'] + operacao['taxas'].to_d) / posicao_atual).round(4)
            ultimo_preco_medio = preco_medio_compra
          else
            posicao_atual = posicao_anterior - operacao['quantidade']
            preco_medio_venda = (operacao['valor_total'] - operacao['taxas'].to_d) / operacao['quantidade']
            lucro_liquido = ((preco_medio_venda - ultimo_preco_medio) * operacao['quantidade']).round(2)

            Operacao.find(operacao['id']).update(lucro_liquido:)
          end
        elsif operacao['tipo'] == 'Evento'

          case operacao['tipo_operacao_evento']
          when 'Grupamento'
            posicao_atual /= operacao['fator']
            preco_medio_compra = ultimo_preco_medio * operacao['fator']
          when 'Desdobramento'
            posicao_atual *= operacao['fator']
            preco_medio_compra = ultimo_preco_medio / operacao['fator']
          when 'Bonificação'
            posicao_atual = posicao_anterior + operacao['fator']
            preco_medio_compra = ((ultimo_preco_medio * posicao_anterior) + (operacao['preco_ativo'] * operacao['fator'])) / posicao_atual
          end
        end

        posicao_anterior = posicao_atual

        ultimo_preco_medio = preco_medio_compra

        posicao_fim_ano = PosicaoFimAno.new(
          ano: operacao['data'][0..3],
          preco_medio: ultimo_preco_medio,
          posicao: posicao_atual,
          custo_total: posicao_atual * ultimo_preco_medio,
          ativo_id: ativo_id
        )

        hash_ano_ativo = "#{ativo_id}#{operacao['data'][0..3]}"
        posicoes_fim_ano[hash_ano_ativo] = posicao_fim_ano
      end

      next unless posicao_atual.positive?

      AtivoFinanceiro.find(ativo_id).update(
        preco_medio: ultimo_preco_medio.round(2),
        posicao: posicao_atual
      )
    end

    posicoes_fim_ano.each_value do |pfa|
      pfa.save if pfa.posicao.positive?
    end
  end
end