class Nuinvest
  def extract_broker_note_date(note_data)
    date_position = note_data.find_index('Data Pregão') + 1

    Date.parse(note_data[date_position])
  end

  def extract_trades_broker_note(note_data)
    trades = []

    trades_entry = note_data.each_index.select { |i| note_data[i] == 'BOVESPA' }

    trades_entry.each do |position|
      trade = Trade.new

      match_obj = note_data[position + 3].match(/(\w*) (\w{2})/)

      # Purchase
      trade.purchase = note_data[position + 1] == 'C'

      # FinancialAsset
      asset_code = match_obj[1]

      # Remove the "F" at the end of the asset name, if any.
      asset_code = asset_code[..-2] if asset_code[-1] == 'F'

      financial_asset = FinancialAsset.find_by(code: asset_code)

      trade.financial_asset = financial_asset

      # Starting from 2 positions after BOVESPA (in order to skip
      # field C or V (Purchase or Sell). It goes sequentially
      # until it finds a blank line, so then it begins to go back
      # in order to find QUANTIDADE and PREÇO

      temp_position = position + 2

      temp_position += 1 while note_data[temp_position] != ''

      trade.quantity = note_data[temp_position - 4].to_d
      trade.asset_price = note_data[temp_position - 3].gsub(',', '.').to_d
      trade.total_amount = (trade.asset_price * trade.quantity).to_f

      trade.trade_type = if financial_asset.asset_type.name == 'FII'
                           TradeType.find_by(name: 'FII')
                         elsif note_data[temp_position - 5] == 'D'
                           TradeType.find_by(name: 'Day Trade')
                         else
                           TradeType.find_by(name: 'Swing Trade')
                         end

      trades << trade
    end

    trades
  end

  def extract_fees_broker_note(note_data)
    # TODO: Encapsulate.
    def fix_punctuation(value)
      value.gsub(',', '.')
    end

    position = note_data.index('Taxa de Liquidação') + 1
    settlement_fee = fix_punctuation(note_data[position]).to_d.abs

    position = note_data.index('Taxa de Registro') + 1
    registration_fee = fix_punctuation(note_data[position]).to_d.abs

    position = note_data.index('Total Bolsa') + 2
    bovespa_total = fix_punctuation(note_data[position]).to_d.abs

    position = note_data.index('Total Corretagem/Despesas') + 2
    operational_costs = fix_punctuation(note_data[position]).to_d.abs

    settlement_fee + registration_fee + bovespa_total + operational_costs
  end

  def irrf_expression
    %r{I.R.R.F. s/ operações. Base (.*)}
  end

  def irrf_expression_day_trade
    /klfsadjlkdsajlkfasd/ # TODO: This is not implemented yet.
  end

  def irrf_position_adjust
    1
  end
end
