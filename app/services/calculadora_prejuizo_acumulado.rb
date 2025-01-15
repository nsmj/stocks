class CalculadoraPrejuizoAcumulado < ApplicationService
  def initialize(resultado, ano)
    @resultado = resultado
    @ano = ano
  end

  def call
    prejuizo_acumulado_ano_passado = 0
    prejuizo_acumulado = 0

    @resultado = @resultado.map(&:attributes)

    valores_a_retornar = @resultado.map do |r|
      prejuizo_acumulado = if (r["valor"]).negative?
                             prejuizo_acumulado + r["valor"]
      elsif (r["valor"]).positive?
                             [ 0, prejuizo_acumulado + r["valor"] ].min
      else
                             prejuizo_acumulado
      end

      # Obter prejuizo acumulado apenas do ano passado.
      prejuizo_acumulado_ano_passado = prejuizo_acumulado if r["ano"].to_i <= @ano.to_i - 1

      # Não mostrar dados de outros anos além do filtrado.
      next if @ano != r["ano"]

      r["prejuizo_acumulado"] = prejuizo_acumulado

      r
    end.compact

    [ valores_a_retornar, prejuizo_acumulado_ano_passado ]
  end
end
