class LucroPrejuizo < ApplicationService
  def call
    Trade.select('ativo.codigo, SUM(trade.lucro_liquido) AS lucro_liquido')
         .joins(:ativo)
         .where('lucro_liquido IS NOT NULL')
         .group('ativo.nome')
         .order('lucro_liquido DESC')
  end
end
