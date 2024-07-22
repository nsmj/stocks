class FiisQuery < ApplicationService

  def initialize(params: {})
    @params = params
  end

  def call
    Trade.select(
      'STRFTIME("%Y", date) year,
      STRFTIME("%m", date) month,
      SUM(net_profit) AS value'
      )
      .where('trade_type_id = 3
        AND purchase = 0')
      .group('year', 'month')
  end
end