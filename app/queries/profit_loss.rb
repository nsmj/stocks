class ProfitLoss < ApplicationService
  def call
    Trade.select('asset.code, SUM(trade.net_profit) AS net_profit')
      .joins(:financial_asset)
      .where('net_profit IS NOT NULL')
      .group('asset.name')
      .order('net_profit DESC')
  end
end

