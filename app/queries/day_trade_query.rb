class DayTradeQuery < ApplicationService
  def initialize(params: {})
    @params = params
  end

  def call
    # TODO: Tratamento de opções.
    Operacao.find_by_sql("
      SELECT
        ano,
        mes,
        lucro_acoes + lucro_etf + prejuizo AS valor
      FROM
      (
        SELECT
          STRFTIME('%Y', data) ano,
          STRFTIME('%m', data) mes,
        PRINTF('%.2f', SUM(CASE WHEN lucro_liquido > 0 AND a.tipo_ativo_id = 1 THEN
              lucro_liquido
          ELSE
              0
          END)) AS lucro_acoes,
          PRINTF('%.2f', SUM(CASE WHEN lucro_liquido > 0 AND a.tipo_ativo_id = 3 THEN
              lucro_liquido
          ELSE
              0
          END)) AS lucro_etf,
          PRINTF('%.2f', SUM(CASE WHEN lucro_liquido < 0 THEN
              lucro_liquido
          ELSE
              0
          END)) AS prejuizo
        FROM
          operacao o
        LEFT JOIN ativo a ON o.ativo_id = a.id
        WHERE
          o.tipo_operacao_id = 2
          AND compra = 0
          AND a.tipo_ativo_id IN (1, 3)
          GROUP BY
            ano,
            mes
      )
      WHERE CAST(valor AS decimal) <> 0
      ")
  end
end
