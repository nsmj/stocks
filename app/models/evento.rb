class Evento < ApplicationRecord
  belongs_to :tipo_evento, required: true
  belongs_to :ativo, foreign_key: :ativo_id, required: true

  validates :data, presence: true
  validates :fator, numericality: true
  validates :valor, numericality: true, allow_nil: true

  def self.from_json(conteudo_json)
    evento = new

    evento.ativo = Ativo.find_by(codigo: conteudo_json['codigo_ativo'])
    evento.tipo_evento = TipoEvento.find_by(nome: conteudo_json['tipo'])
    evento.fator = conteudo_json['fator']
    evento.data = conteudo_json['data']
    evento.valor = conteudo_json['valor']

    evento
  end

  # rails_admin do
  #   configure :data do
  #     strftime_format '%d/%m/%Y'
  #   end
  # end
end
