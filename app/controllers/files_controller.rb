require 'pdf'
require 'zip'
require 'rake'

class FilesController < ApplicationController
  def import; end

  def import_do
    unzip_file(filter_params['file'])

    system('mkdir txt')

    Stocks::Application.load_tasks
    Rake::Task['db:reset'].invoke

    process_broker_notes
    process_json_files

    calculate_results

    redirect_to import_files_path
  end

  def unzip_file(file)
    File.binwrite(file.original_filename, file.read)

    Zip::File.open(file.original_filename) do |zip_file|
      zip_file.each do |f|
        f_path = File.join('files', f.name)
        FileUtils.mkdir_p(File.dirname(f_path))
        zip_file.extract(f, f_path) unless File.exist?(f_path)
      end
    end

    File.delete(file.original_filename)
  end

  def process_broker_notes
    Dir.glob('files/BrokerNotes/**/*').each do |file_path|
      next if File.directory? file_path

      broker = BrokerNote.get_broker(file_path)

      broker_note = BrokerNote.new(broker)

      broker_note.extract_file_data(file_path)

      broker_note.trades.each do |t|
        raise StandardError, t.errors unless t.save
      end

      broker_note.irrfs.each do |i|
        raise StandardError, i.errors unless i.save
      end
    end
  end

  def process_json_files
    Dir.glob('files/JsonFile/**/*').each do |file_path|
      json_file = JsonFile.new

      json_file.extract_file_data(file_path).each do |j|
        raise StandardError, j.errors unless j.save
      end
    end
  end

  def calculate_results
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

  def filter_params
    params.permit(:file)
  end
end
