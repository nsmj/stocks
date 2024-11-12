require 'date'

# Class for generic reports.
class ReportsController < ApplicationController
  def resultados_mensais
    @results, @total = ResultadosMensaisQuery.call(params: filter_params)
  end

  def lucro_prejuizo
    @results = LucroPrejuizo.call
  end

  private

  def filter_params
    params.permit(:ano, :mes)
  end
end
