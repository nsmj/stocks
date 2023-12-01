require 'date'

# Class for generic reports.
class ReportsController < ApplicationController
  def monthly_results
    results = Trade.select('STRFTIME(\'%d/%m/%Y\', date) AS trade_date,
                            asset.code as ticker,
                            quantity,
                            asset_price,
                            purchase,
                            fees,
                            total_amount,
                            net_profit')
                   .joins(:financial_asset)
                   .where(date: Date.civil(2023, filter_params[:month].to_i,
                                           1)..Date.civil(2023, filter_params[:month].to_i, -1))
                   .order(:date, :ticker).map { |i| i.attributes.except('id') }
    # TODO: Encapsulate the "remove ID" logic

    render json: results
  end

  private

  def filter_params
    params.permit(:month)
  end
end
