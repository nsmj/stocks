class TradeType < ApplicationRecord
  has_many :trades
  has_many :irrfs

  validates :name, presence: true, uniqueness: true

  # rails_admin do
  #   configure :trades do
  #     hide
  #   end
  # end
end
