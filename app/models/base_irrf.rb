class BaseIrrf
  attr_accessor :base_swing_trade, :base_day_trade, :base_fiis

  def initialize
    @base_swing_trade = 0
    @base_day_trade = 0
    @base_fiis = 0
  end
end
