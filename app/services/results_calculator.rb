class ResultsCalculator < ApplicationService

  def call
    sql = <<~SQL
      SELECT t.id, 'Trade' AS type, date, null AS 'factor', quantity, asset_price, total_amount, purchase, fees, a.id AS asset_id, tt.name AS 'trade_type_event' FROM trade t
      LEFT JOIN asset a ON t.asset_id = a.id
      LEFT JOIN trade_type tt ON t.trade_type_id = tt.id
      UNION ALL
      SELECT e.id, 'Event', date, factor, null, value AS 'asset_price', null, null, null, a.id AS asset_id, et.name FROM event e
      LEFT JOIN asset a ON e.asset_id = a.id
      LEFT JOIN event_type et ON e.event_type_id = et.id
      ORDER BY asset_id, date
    SQL

    rows = ActiveRecord::Base.connection.execute(sql)

    organized_result = {}
    end_year_positions = {}

    rows.each do |row|
      organized_result[row['asset_id']] = [] unless organized_result.key? row['asset_id']
      organized_result[row['asset_id']] << row
    end

    organized_result.each do |asset_id, trades|
      last_app_price = 0
      previous_position = 0
      actual_position = 0
      average_purchase_price = 0

      trades.each do |trade|
        if trade['type'] == 'Trade'

          if trade['purchase'] == 1
            actual_position = previous_position + trade['quantity']
            average_purchase_price = (((last_app_price * previous_position) + trade['total_amount'] + trade['fees'].to_d) / actual_position).round(4)
            last_app_price = average_purchase_price
          else
            actual_position = previous_position - trade['quantity']
            average_selling_price = (trade['total_amount'] - trade['fees'].to_d) / trade['quantity']
            net_profit = ((average_selling_price - last_app_price) * trade['quantity']).round(2)

            Trade.find(trade['id']).update(net_profit:)
          end
        elsif trade['type'] == 'Event'

          case trade['trade_type_event']
          when 'Grupamento'
            actual_position /= trade['factor']
            average_purchase_price = last_app_price * trade['factor']
          when 'Desdobramento'
            actual_position *= trade['factor']
            average_purchase_price = last_app_price / trade['factor']
          when 'Bonificação'
            actual_position = previous_position + trade['factor']
            average_purchase_price = ((last_app_price * previous_position) + (trade['asset_price'] * trade['factor'])) / actual_position
          end
        end

        previous_position = actual_position

        last_app_price = average_purchase_price

        end_year_position = EndYearPosition.new(
          year: trade['date'][0..3],
          average_price: last_app_price,
          position: actual_position,
          total_cost: actual_position * last_app_price,
          asset_id:
        )

        asset_year_hash = "#{asset_id}#{trade['date'][0..3]}"
        end_year_positions[asset_year_hash] = end_year_position
      end

      next unless actual_position.positive?

      FinancialAsset.find(asset_id).update(
        average_price: last_app_price.round(2),
        position: actual_position
      )
    end

    end_year_positions.each do |_, eyp|
      eyp.save if eyp.position.positive?
    end
  end
end