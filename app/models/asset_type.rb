class AssetType < ApplicationRecord
  has_many :financial_assets

  validates :name, presence: true, uniqueness: true

  # rails_admin do
  #   configure :financial_assets do
  #     hide
  #   end
  # end
end
