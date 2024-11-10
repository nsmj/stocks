class TipoEvento < ApplicationRecord
  has_many :eventos

  validates :nome, presence: true, uniqueness: true
end
