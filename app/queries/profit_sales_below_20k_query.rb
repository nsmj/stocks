class ProfitSalesBelow20kQuery < ApplicationService

  def initialize(params: {})
    @params = params
  end

  def call
    result = Trade.find_by_sql ["
    SELECT
      PRINTF('%.2f', SUM(profit)) AS value
    FROM
    (
      SELECT
        STRFTIME('%m', date) month,
        SUM(net_profit) profit,
        SUM(total_amount) sales_amount
      FROM
        trade t
      LEFT JOIN asset a
    ON t.asset_id = a.id
      WHERE
        date BETWEEN ':year-01-01' AND ':year-12-31'
      AND purchase = 0
      AND net_profit > 0
      AND a.asset_type_id = 1
      AND t.trade_type_id = 1
      GROUP BY month
      HAVING sales_amount < 20000
    )
    ", { year: @params[:year].to_i }]

    result[0].value
  end
end