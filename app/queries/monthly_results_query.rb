class MonthlyResultsQuery < ApplicationService

  def initialize(relation: Trade.all, params: {})
    @relation = relation
    @params = params
  end

  def call
    results = @relation.select('STRFTIME(\'%d/%m/%Y\', date) AS trade_date,
                            asset.code as ticker,
                            quantity,
                            asset_price,
                            purchase,
                            fees,
                            total_amount,
                            net_profit')
                   .joins(:financial_asset)
                   .where(date: Date.civil(@params[:year].to_i, @params[:month].to_i,
                                           1)..Date.civil(@params[:year].to_i, @params[:month].to_i, -1))
                   .order(:date, :ticker).map { |i| i.attributes.except('id') }
    # TODO: Encapsulate the "remove ID" logic

    total = results.map { |i| i['net_profit'] }.reduce do |accumulator, current|
      accumulator = 0 if accumulator.blank?
      accumulator += current unless current.blank?
      accumulator
    end

    [results, total]
  end
end