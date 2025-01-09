class LucroPrejuizo < ApplicationService
  def call
    Operacao.select('ativo.codigo, SUM(operacao.lucro_liquido) AS lucro_liquido')
            .joins(:ativo)
            .where('lucro_liquido IS NOT NULL')
            .group('ativo.nome')
            .order('lucro_liquido DESC')
  end
end
