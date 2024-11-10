class ResultadosMensaisQuery < ApplicationService
  def initialize(relation: Trade.all, params: {})
    @relation = relation
    @params = params
  end

  def call
    resultados = @relation.select('STRFTIME(\'%d/%m/%Y\', data) AS data_operacao,
                            ativo.codigo,
                            quantidade,
                            preco_ativo,
                            compra,
                            taxas,
                            valor_total,
                            lucro_liquido')
                          .joins(:ativo)
                          .where(data: Date.civil(@params[:ano].to_i, @params[:mes].to_i,
                                                  1)..Date.civil(@params[:ano].to_i, @params[:mes].to_i, -1))
                          .order(:data, :codigo).map { |i| i.attributes.except('id') }
    # TODO: Encapsular a lógica de "remover ID"

    total = resultados.map { |i| i['lucro_liquido'] }.reduce do |acumulador, atual|
      acumulador = 0 if acumulador.blank?
      acumulador += atual unless atual.blank?
      acumulador
    end

    [resultados, total]
  end
end
