class DayTradeQuery < ApplicationService

  def initialize(params: {})
    @params = params
  end

  def call
    # TODO: Options handling.
    Trade.find_by_sql(["
      SELECT
        month,
        stock_profit + etf_profit + loss AS value
      FROM
      (
        SELECT
          STRFTIME('%m', date) month,
        PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id = 1 THEN
              net_profit
          ELSE
              0
          END)) AS stock_profit,
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
          date BETWEEN ':year-01-01' AND ':year-12-31'
        AND t.trade_type_id = 2
          AND purchase = 0
          AND a.asset_type_id IN (1, 3)
          GROUP BY month
      )
      WHERE CAST(value AS decimal) <> 0
      ", { year: @params[:year].to_i }])
  end
end