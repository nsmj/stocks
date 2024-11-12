# Class related to Asset table.
class FinancialAssetsController < ApplicationController
  def new_average_price
  end

  def calculate_new_average_price
    asset = FinancialAsset.find_by(codigo: filter_params[:codigo].upcase)

    render json: asset.new_average_price(
      filter_params[:invested_amount],
      asset.price
    )
  end

  def by_codigo
    render json: get_by_codigo(filter_params[:codigo])
  end

  private

  def get_by_codigo(codigo)
    FinancialAsset.find_by(codigo: codigo.upcase)
  end

  def filter_params
    params.permit(:codigo, :invested_amount)
  end
end
