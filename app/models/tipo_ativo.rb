class TipoAtivo < ApplicationRecord
  has_many :ativos

  validates :nome, presence: true, uniqueness: true

  # rails_admin do
  #   configure :ativos do
  #     hide
  #   end
  # end
end
