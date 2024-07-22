class EndYearPositionQuery < ApplicationService

  def initialize(params: {})
    @params = params
  end

  def call
    end_year_positions = EndYearPosition.select('
                                        asset.code,
                                        asset.cnpj,
                                        asset.name AS asset_name,
                                        end_year_position.position,
                                        ROUND(end_year_position.average_price, 2) AS average_price,
                                        ROUND(end_year_position.total_cost, 2) AS total_cost,
                                        asset_type.name AS asset_type')
                                        .joins(financial_asset: :asset_type)
                                        .where(year: @params[:year])
                                        .order('asset.code')

    end_year_positions.map do |i|
      p = i.attributes

      p['text'] = case i.asset_type
                  when 'Ação'
                    "#{i.position} AÇÕES DA #{i.asset_name}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.code}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.average_price.to_s.gsub('.', ',')} POR AÇÃO. CUSTO TOTAL DE R$ #{i.total_cost.to_s.gsub('.', ',')}"
                  when 'FII'
                    "#{i.position} COTAS DO FII #{i.asset_name}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.code}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.average_price.to_s.gsub('.', ',')} POR COTA. CUSTO TOTAL DE R$ #{i.total_cost.to_s.gsub('.', ',')}"
                  when 'ETF'
                    "#{i.position} COTAS DO ETF #{i.asset_name}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.code}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.average_price.to_s.gsub('.', ',')} POR COTA. CUSTO TOTAL DE R$ #{i.total_cost.to_s.gsub('.', ',')}"
                  else
                    'goiaba'
                  end

      p
    end
  end
end