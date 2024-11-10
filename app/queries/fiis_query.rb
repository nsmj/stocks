class FiisQuery < ApplicationService
  def initialize(params: {})
    @params = params
  end

  def call
    Trade.select(
      'STRFTIME("%Y", data) ano,
      STRFTIME("%m", data) mes,
      SUM(lucro_liquido) AS valor'
    )
         .where('tipo_operacao = 3
        AND compra = 0')
         .group('ano', 'mes')
  end
end
