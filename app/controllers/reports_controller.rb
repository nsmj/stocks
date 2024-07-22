require 'date'

# Class for generic reports.
class ReportsController < ApplicationController
  def monthly_results
    @results, @total = MonthlyResultsQuery.call(params: filter_params)
  end

  private

  def filter_params
    params.permit(:year, :month)
  end
end
