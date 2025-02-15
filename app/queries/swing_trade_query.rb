class SwingTradeQuery < ApplicationService
  def call
    Operacao.find_by_sql("
      SELECT
        ano,
        mes,
        CASE WHEN CAST(vendas_acoes AS decimal) > 20000 THEN
          lucro_acoes + lucro_opcoes + lucro_etf + prejuizo
        ELSE
          lucro_etf + lucro_opcoes + prejuizo
        END valor
        FROM
        (
          SELECT
            STRFTIME('%Y', data) ano,
            STRFTIME('%m', data) mes,
          PRINTF('%.2f', SUM(CASE WHEN a.tipo_ativo_id = 1 THEN
            valor_total
          ELSE
            0
          END)) AS vendas_acoes,
          PRINTF('%.2f', SUM(CASE WHEN lucro_liquido > 0 AND a.tipo_ativo_id = 1 THEN
                lucro_liquido
            ELSE
                0
            END)) AS lucro_acoes,
            PRINTF('%.2f', SUM(CASE WHEN lucro_liquido > 0 AND a.tipo_ativo_id IN (4, 5) THEN
                lucro_liquido
            ELSE
                0
            END)) AS lucro_opcoes,
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
            o.tipo_operacao_id = 1
            AND compra = 0
            AND a.tipo_ativo_id <> 2
            GROUP BY ano, mes
        )
        WHERE CAST(valor AS decimal) <> 0
    ")
  end
end
