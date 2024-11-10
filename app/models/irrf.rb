class Irrf < ApplicationRecord
  belongs_to :tipo_operacao

  validates :data, presence: true
  validates :valor, numericality: true
end
