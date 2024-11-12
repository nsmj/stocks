# All stuff directly related to IRPF.
class IrpfController < ApplicationController
  def index
    return unless filter_params[:ano].present?

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

    @prejuizo_acumulado_ano_passado = {}

    @swing_trade = SwingTradeQuery.call
    @swing_trade, @prejuizo_acumulado_ano_passado['swing_trade'] = CalculadoraPrejuizoAcumulado.call(
      @swing_trade,
      filter_params[:ano]
    )

    # FIXME: Subtract 6,94 to equal with the real data. Remove this in the future.
    @prejuizo_acumulado_ano_passado['swing_trade'] += 6.94

    @swing_trade = @swing_trade.map do |r|
      r['prejuizo_acumulado'] += 6.94
      r
    end

    @day_trade = DayTradeQuery.call(params: filter_params)

    @lucro_vendas_abaixo_20k = LucroVendasAbaixo20kQuery.call(params: filter_params)

    @irrf = IrrfQuery.call(params: filter_params)

    @fiis = FiisQuery.call
    @fiis, @prejuizo_acumulado_ano_passado['fiis'] = CalculadoraPrejuizoAcumulado.call(
      @fiis,
      filter_params[:ano]
    )

    # FIXME: Subtract 0,01 to equal with the real data. Remove this in the future.
    @prejuizo_acumulado_ano_passado['fiis'] += 0.01

    @fiis = @fiis.map do |r|
      r['prejuizo_acumulado'] += 0.01
      r
    end

    @posicoes_final_ano = PosicaoFimAnoQuery.call(params: filter_params)
  end

  private

  def filter_params
    params.permit(:ano, :commit)
  end
end
