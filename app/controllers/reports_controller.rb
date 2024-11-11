require 'date'

# Class for generic reports.
class ReportsController < ApplicationController
  def monthly_results
    @results, @total = MonthlyResultsQuery.call(params: filter_params)
  end

  def profit_loss
    @results = ProfitLoss.call
  end

  private

  def filter_params
    params.permit(:ano, :mes)
  end
end
