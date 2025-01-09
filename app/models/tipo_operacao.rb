class TipoOperacao < ApplicationRecord
  has_many :operacoes
  has_many :irrfs

  validates :nome, presence: true, uniqueness: true

  # rails_admin do
  #   configure :operacoes do
  #     hide
  #   end
  # end
end
