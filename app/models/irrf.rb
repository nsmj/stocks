class Irrf < ApplicationRecord
  belongs_to :trade_type

  validates :date, presence: true
  validates :value, numericality: true
end
