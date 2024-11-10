class IrrfQuery < ApplicationService
  def initialize(relacao: Irrf.all, params: {})
    @relacao = relacao
    @params = params
  end

  def call
    @relacao.select("ROUND(SUM(valor), 2) AS valor, STRFTIME('%m', data) mes, tipo_operacao.nome AS tipo_operacao")
            .joins(:tipo_operacao)
            .where("data >= ':ano-01-01' AND data <= ':ano-12-31'", { ano: @params[:year].to_i })
            .group('mes, tipo_operacao.nome')
            .order(:mes)
  end
end
