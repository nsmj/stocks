class FinancialAsset < ApplicationRecord
  self.table_name = 'asset'

  validates :code, presence: true, uniqueness: true
  validates :name, presence: true
  validates :note_description, presence: true
  validates :average_price, numericality: true, allow_nil: true
  validates :ceiling_price_factor, numericality: true, allow_nil: true
  validates :average_ceiling_price, numericality: true, allow_nil: true
  validates :projective_ceiling_price, numericality: true, allow_nil: true

  belongs_to :asset_type, required: true
  has_many :trades, foreign_key: :asset_id
  has_many :events, foreign_key: :asset_id
  has_many :end_year_positions, foreign_key: :asset_id

  def new_average_price(invested_amount, price)
    invested_amount = invested_amount.gsub(',', '.').to_f if invested_amount.is_a? String
    price = price.gsub(',', '.').to_f if price.is_a? String

    quantity = invested_amount / price

    new_price = ((position * average_price) + invested_amount) / (quantity + position)

    { 'old_average_price' => average_price,
      'new_average_price' => new_price.round(2) }
  end

  # rails_admin do
  #   list do
  #     fields :id, :code, :name, :cnpj, :paying_source_cnpj, :asset_type,
  #            :average_ceiling_price, :projective_ceiling_price
  #   end
  # end

  def to_param
    code
  end

  def price
    conn = Faraday.new(
      url: 'https://brapi.dev',
      params: { token: ENV.fetch('BRAPI_DEV_TOKEN') }
    )

    response = conn.get("/api/quote/#{code}")
    JSON.parse(response.body)['results'].first['regularMarketPrice']
  end
end
