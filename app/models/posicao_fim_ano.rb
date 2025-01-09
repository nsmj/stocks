class PosicaoFimAno < ApplicationRecord
  belongs_to :ativo, foreign_key: :ativo_id

  validates :ano, numericality: true
  validates :preco_medio, numericality: true
  validates :posicao, numericality: true
  validates :custo_total, numericality: true

  # rails_admin do
  #   configure :preco_medio do
  #     pretty_value do
  #       valor.round(2)
  #     end
  #   end

  #   configure :custo_total do
  #     pretty_value do
  #       valor.round(2)
  #     end
  #   end
  # end
end
