class LucroVendasAbaixo20kQuery < ApplicationService
  def initialize(params: {})
    @params = params
  end

  def call
    resultado = Operacao.find_by_sql ["
    SELECT
      PRINTF('%.2f', SUM(lucro)) AS valor
    FROM
    (
      SELECT
        STRFTIME('%m', data) mes,
        SUM(lucro_liquido) lucro,
        SUM(valor_total) valor_vendas
      FROM
        trade t
      LEFT JOIN ativo a
    ON t.ativo_id = a.id
      WHERE
        data BETWEEN ':ano-01-01' AND ':ano-12-31'
      AND compra = 0
      AND lucro_liquido > 0
      AND a.tipo_ativo_id = 1
      AND t.tipo_operacao_id = 1
      GROUP BY mes
      HAVING valor_vendas < 20000
    )
    ", { ano: @params[:ano].to_i }]

    resultado[0].valor
  end
end
