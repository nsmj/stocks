# All stuff directly related to IRPF.
class IrpfController < ApplicationController

  def index

    if filter_params[:year].present?

      @months = {
        1 => 'Janeiro',
        2 => 'Fevereiro',
        3 => 'Março',
        4 => 'Abril',
        5 => 'Maio',
        6 => 'Junho',
        7 => 'Julho',
        8 => 'Agosto',
        9 => 'Setembro',
        10 => 'Outubro',
        11 => 'Novembro',
        12 => 'Dezembro'
      }

      @accumulated_loss_last_year = {}

      @swing_trade = SwingTradeQuery.call
      @swing_trade, @accumulated_loss_last_year['swing_trade'] = AccumulatedLossCalculator.call(
        @swing_trade,
        filter_params[:year])

      @day_trade = DayTradeQuery.call(params: filter_params)

      @profit_from_sales_below_20k = ProfitSalesBelow20kQuery.call(params: filter_params)

      @irrf = IrrfQuery.call(params: filter_params)

      @fiis = FiisQuery.call
      @fiis, @accumulated_loss_last_year['fiis'] = AccumulatedLossCalculator.call(
        @fiis,
        filter_params[:year])

      @end_year_positions = EndYearPositionQuery.call(params: filter_params)
    end
  end

  private

  def filter_params
    params.permit(:year, :commit)
  end
end
