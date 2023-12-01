class BaseIrrf
  attr_accessor :swing_trade_base, :day_trade_base, :fiis_base

  def initialize
    @swing_trade_base = 0
    @day_trade_base = 0
    @fiis_base = 0
  end
end
