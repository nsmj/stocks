class Event < ApplicationRecord
  belongs_to :event_type, required: true
  belongs_to :financial_asset, foreign_key: :asset_id, required: true

  validates :date, presence: true
  validates :factor, numericality: true
  validates :value, numericality: true, allow_nil: true

  def self.from_json(json_content)
    event = new

    event.financial_asset = FinancialAsset.find_by(code: json_content['asset_code'])
    event.event_type = EventType.find_by(name: json_content['type'])
    event.factor = json_content['factor']
    event.date = json_content['date']
    event.value = json_content['value']

    event
  end

  # rails_admin do
  #   configure :date do
  #     strftime_format '%d/%m/%Y'
  #   end
  # end
end
