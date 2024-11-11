class Operacao < ApplicationRecord
  belongs_to :ativo, foreign_key: :ativo_id
  belongs_to :tipo_operacao

  def total
    quantidade * preco_ativo
  end

  def self.from_json(conteudo_json)
    operacao = new

    operacao.ativo_financeiro = Ativo.find_by(codigo: conteudo_json['codigo_ativo'])
    operacao.tipo_operacao = TipoOperacao.find_by(nome: conteudo_json['tipo'])
    operacao.data = conteudo_json['data']
    operacao.compra = conteudo_json['compra']
    operacao.quantidade = conteudo_json['quantidade']
    operacao.preco_ativo = conteudo_json['preco_ativo']
    operacao.lucro_liquido = conteudo_json['lucro_liquido']
    operacao.valor_total = conteudo_json['valor_total']

    operacao
  end

  # rails_admin do
  #   configure :data do
  #     strftime_format '%d/%m/%Y'
  #   end

  #   # FIXME: O campo de compra por algum motivo faz com que o modelo não seja exibido.
  #   # list do
  #   #  fields :id, :data, :quantidade, :ativo, :tipo_ativo, :compra
  #   # end
  # end
end
