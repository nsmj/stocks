class EndYearPosition < ApplicationRecord
  belongs_to :financial_asset, foreign_key: :asset_id

  validates :year, numericality: true
  validates :average_price, numericality: true
  validates :position, numericality: true
  validates :total_cost, numericality: true

  # rails_admin do
  #   configure :average_price do
  #     pretty_value do
  #       value.round(2)
  #     end
  #   end

  #   configure :total_cost do
  #     pretty_value do
  #       value.round(2)
  #     end
  #   end
  # end
end
