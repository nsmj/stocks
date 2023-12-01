class Trade < ApplicationRecord
  belongs_to :financial_asset, foreign_key: :asset_id
  belongs_to :trade_type

  def total
    quantity * asset_price
  end

  def self.from_json(json_content)
    trade = new

    trade.financial_asset = FinancialAsset.find_by(code: json_content['asset_code'])
    trade.trade_type = TradeType.find_by(name: json_content['type'])
    trade.date = json_content['date']
    trade.purchase = json_content['purchase']
    trade.quantity = json_content['quantity']
    trade.asset_price = json_content['asset_price']
    trade.net_profit = json_content['net_profit']
    trade.total_amount = json_content['total_amount']

    trade
  end

  # rails_admin do
  #   configure :date do
  #     strftime_format '%d/%m/%Y'
  #   end

  #   # FIXME: The purchase filed for some reason make the model not to be shown.
  #   # list do
  #   #  fields :id, :date, :quantity, :asset, :asset_type, :purchase
  #   # end
  # end
end
