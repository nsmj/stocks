require 'pdf'

# Class that represents a Broker Note.
class BrokerNote
  attr_accessor :trades, :irrfs, :date, :total_fees, :broker,
                :trades_irrf_base, :undefined_irrf

  def initialize(broker)
    @trades = []
    @irrfs = []
    @broker = broker
    @trades_irrf_base = BaseIrrf.new
  end

  def self.get_broker(file_path)
    file_path.match(%r{(.*)/BrokerNotes/(.*)/(.*)}).captures[1].constantize.new
  end

  def extract_file_data(file_path)
    note_data = Pdf.extract_pdf_data(file_path)

    @date = @broker.extract_broker_note_date(note_data)

    @trades = @broker.extract_trades_broker_note(note_data)
    @total_fees = @broker.extract_fees_broker_note(note_data)
    prorate_fees

    @irrfs = extract_irrf(note_data)

    @trades = @trades.map do |trade|
      trade.date = @date
      trade
    end
  end

  def prorate_fees
    trades_total = 0

    trades_total = @trades.map(&:total).reduce(:+)

    @trades.each_with_index do |trade, index|
      trade_percentage = trade.total / trades_total
      @trades[index].fees = (@total_fees * trade_percentage).round(2)
    end
  end

  def calculate_irrf_base
    @trades.each do |trade|
      next if trade.purchase == 1

      case trade.trade_type.name
      when 'Swing Trade'
        trades_irrf_base.swing_trade_base += trade.total_amount
      when 'FII'
        trades_irrf_base.fiis_base += trade.total_amount
      end
    end

    trades_irrf_base.swing_trade_base = trades_irrf_base.swing_trade_base.round(2)
    trades_irrf_base.fiis_base = trades_irrf_base.fiis_base.round(2)
  end

  def extract_irrf(note_data)
    def replacements(s)
      s.gsub('.', '').gsub(',', '.')
    end

    calculate_irrf_base

    irrfs = []

    note_data.each_with_index do |k, i|
      # Swing Trade and FIIs.
      obj = k.match @broker.irrf_expression

      next unless obj

      base_value = replacements(obj[1]).to_d

      next unless base_value.positive?

      irrf_value = replacements(note_data[i + @broker.irrf_position_adjust]).to_d

      next unless irrf_value.positive?

      irrf = Irrf.new

      irrf.value = irrf_value
      irrf.date = @date

      case base_value
      when trades_irrf_base.swing_trade_base
        irrf.trade_type = TradeType.find_by(name: 'Swing Trade')
      when trades_irrf_base.fiis_base
        irrf.trade_type = TradeType.find_by(name: 'FII')
      else
        undefined_irrf = true
      end

      irrfs << irrf unless undefined_irrf == true

      # Day Trade
      obj = k.match @broker.irrf_expression_day_trade

      next unless obj

      value = replacements(obj[3]).to_d

      next unless value.positive?

      irrf = Irrf.new

      irrf.trade_type = Trade.find_by(name: 'FII')
      irrf.value = value
      irrf.date = @date

      irrfs << irrf
    end

    irrfs
  end
end
