class DayTradeQuery < ApplicationService
  def initialize(params: {})
    @params = params
  end

  def call
    # TODO: Tratamento de opções.
    Trade.find_by_sql(["
      SELECT
        mes,
        lucro_acoes + lucro_etf + prejuizo AS valor
      FROM
      (
        SELECT
          STRFTIME('%m', data) mes,
        PRINTF(\"%.2f\", SUM(CASE WHEN lucro_liquido > 0 AND a.tipo_ativo_id = 1 THEN
              lucro_liquido
          ELSE
              0
          END)) AS lucro_acoes,
          PRINTF(\"%.2f\", SUM(CASE WHEN lucro_liquido > 0 AND a.tipo_ativo_id = 3 THEN
              lucro_liquido
          ELSE
              0
          END)) AS lucro_etf,
          PRINTF(\"%.2f\", SUM(CASE WHEN lucro_liquido < 0 THEN
              lucro_liquido
          ELSE
              0
          END)) AS prejuizo
        FROM
          trade t
        LEFT JOIN ativo a ON t.ativo_id = a.id
        WHERE
          data BETWEEN ':ano-01-01' AND ':ano-12-31'
        AND t.tipo_operacao_id = 2
          AND compra = 0
          AND a.tipo_ativo_id IN (1, 3)
          GROUP BY mes
      )
      WHERE CAST(valor AS decimal) <> 0
      ", { ano: @params[:ano].to_i }])
  end
end
