# Class related to Asset table.
class FinancialAssetsController < ApplicationController
  def new_average_price
    asset = FinancialAsset.find_by(code: filter_params[:code].upcase)

    render json: asset.new_average_price(
      filter_params[:invested_amount],
      asset.price
    )
  end

  def by_code
    render json: get_by_code(filter_params[:code])
  end

  private

  def get_by_code(code)
    FinancialAsset.find_by(code: code.upcase)
  end

  def filter_params
    params.permit(:code, :invested_amount)
  end
end
