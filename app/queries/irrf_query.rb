class IrrfQuery < ApplicationService

  def initialize(relation: Irrf.all, params: {})
    @relation = relation
    @params = params
  end

  def call
    @relation.select("ROUND(SUM(value), 2) AS value, STRFTIME('%m', date) month, trade_type.name AS trade_type")
        .joins(:trade_type)
        .where("date >= ':year-01-01' AND date <= ':year-12-31'", { year: @params[:year].to_i })
        .group('month, trade_type.name')
        .order(:month)
  end
end