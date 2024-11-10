class PosicaoFimAnoQuery < ApplicationService
  def initialize(params: {})
    @params = params
  end

  def call
    posicoes_final_ano = EndYearPosition.select('
                                        ativo.codigo,
                                        ativo.cnpj,
                                        ativo.nome AS nome_ativo,
                                        posicao_fim_ano.posicao,
                                        ROUND(posicao_fim_ano.preco_medio, 2) AS preco_medio,
                                        ROUND(posicao_fim_ano.custo_total, 2) AS custo_total,
                                        tipo_ativo.nome AS tipo_ativo')
                                        .joins(financial_asset: :tipo_ativo)
                                        .where(ano: @params[:ano])
                                        .order('ativo.codigo')

    posicoes_final_ano.map do |i|
      p = i.attributes

      p['texto'] = case i.tipo_ativo
                   when 'Ação'
                     "#{i.posicao} AÇÕES DA #{i.nome_ativo}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.codigo}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.preco_medio.to_s.gsub('.', ',')} POR AÇÃO. CUSTO TOTAL DE R$ #{i.custo_total.to_s.gsub('.', ',')}"
                   when 'FII'
                     "#{i.posicao} COTAS DO FII #{i.nome_ativo}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.codigo}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.preco_medio.to_s.gsub('.', ',')} POR COTA. CUSTO TOTAL DE R$ #{i.custo_total.to_s.gsub('.', ',')}"
                   when 'ETF'
                     "#{i.posicao} COTAS DO ETF #{i.nome_ativo}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.codigo}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.preco_medio.to_s.gsub('.', ',')} POR COTA. CUSTO TOTAL DE R$ #{i.custo_total.to_s.gsub('.', ',')}"
                   else
                     'goiaba'
                   end

      p
    end
  end
end
