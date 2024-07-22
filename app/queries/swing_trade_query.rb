class SwingTradeQuery < ApplicationService

  def call
    Trade.find_by_sql("
      SELECT
        year,
        month,
        CASE WHEN CAST(stock_sales AS decimal) > 20000 THEN
          stock_profit + options_profit + etf_profit + loss
        ELSE
          etf_profit + options_profit + loss
        END value
        FROM
        (
          SELECT
            STRFTIME('%Y', date) year,
            STRFTIME('%m', date) month,
          PRINTF(\"%.2f\", SUM(CASE WHEN a.asset_type_id = 1 THEN
            total_amount
          ELSE
            0
          END)) AS stock_sales,
          PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id = 1 THEN
                net_profit
            ELSE
                0
            END)) AS stock_profit,
            PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id IN (4, 5) THEN
                net_profit
            ELSE
                0
            END)) AS options_profit,
            PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id = 3 THEN
                net_profit
            ELSE
                0
            END)) AS etf_profit,
            PRINTF(\"%.2f\", SUM(CASE WHEN net_profit < 0 THEN
                net_profit
            ELSE
                0
            END)) AS loss
          FROM
            trade t
          LEFT JOIN asset a ON t.asset_id = a.id
          WHERE
            t.trade_type_id = 1
            AND purchase = 0
            AND a.asset_type_id <> 2
            GROUP BY year, month
        )
        WHERE CAST(value AS decimal) <> 0
    ")
  end
end