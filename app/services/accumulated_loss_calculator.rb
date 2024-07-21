class AccumulatedLossCalculator < ApplicationService

  def initialize(result, year)
    @result = result
    @year = year
  end

  def call
    accumulated_loss_last_year = 0
    accumulated_loss = 0

    @result = @result.map(&:attributes)

    values_to_return = @result.map do |r|
      accumulated_loss = if (r['value']).negative?
                          accumulated_loss + r['value']
                        elsif (r['value']).positive?
                          [0, accumulated_loss + r['value']].min
                        else
                          accumulated_loss
                        end

      # Get accumulated loss just from the last year.
      accumulated_loss_last_year = accumulated_loss if r['year'].to_i == @year.to_i - 1

      # Don't show data from other years than the filtered one.
      next if @year != r['year']

      r['accumulated_loss'] = accumulated_loss

      r
    end.compact

    [values_to_return, accumulated_loss_last_year]
  end
end
