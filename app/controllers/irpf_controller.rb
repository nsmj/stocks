# All stuff directly related to IRPF.
class IrpfController < ApplicationController

  def index

    if filter_params[:year].present?

      @months = {
        1 => 'Janeiro',
        2 => 'Fevereiro',
        3 => 'Março',
        4 => 'Abril',
        5 => 'Maio',
        6 => 'Junho',
        7 => 'Julho',
        8 => 'Agosto',
        9 => 'Setembro',
        10 => 'Outubro',
        11 => 'Novembro',
        12 => 'Dezembro'
      }

      @accumulated_loss_last_year = {}

      @swing_trade = swing_trade
      @day_trade = day_trade
      @irrf = irrf
      @fiis = fiis
      puts @swing_trade
    end
  end

  def reports
    @accumulated_loss_last_year = {}

    render json: {
      profit_from_sales_below_20k:,
      swing_trade:,
      day_trade:,
      fiis:,
      end_year_positions:,
      irrf:,
      accumulated_loss_last_year: @accumulated_loss_last_year
    }
  end

  def irrf
    Irrf.select("ROUND(SUM(value), 2) AS value, STRFTIME('%m', date) month, trade_type.name AS trade_type")
        .joins(:trade_type)
        .where("date >= ':year-01-01' AND date <= ':year-12-31'", { year: filter_params[:year].to_i })
        .group('month, trade_type.name')
        .order(:month).map do |i|
      i.attributes.except('id')
    end
  end

  def profit_from_sales_below_20k
    result = Trade.find_by_sql ["
      SELECT
        PRINTF('%.2f', SUM(profit)) AS value
      FROM
      (
        SELECT
          STRFTIME('%m', date) month,
          SUM(net_profit) profit,
          SUM(total_amount) sales_amount
        FROM
          trade t
        LEFT JOIN asset a
      ON t.asset_id = a.id
        WHERE
          date BETWEEN ':year-01-01' AND ':year-12-31'
        AND purchase = 0
        AND net_profit > 0
        AND a.asset_type_id = 1
        AND t.trade_type_id = 1
        GROUP BY month
        HAVING sales_amount < 20000
      )
      ", { year: filter_params[:year].to_i }]

    result[0].value
  end

  def day_trade
    # TODO: Options handling.
    Trade.find_by_sql(["
      SELECT
        month,
        stock_profit + etf_profit + loss AS value
      FROM
      (
        SELECT
          STRFTIME('%m', date) month,
        PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id = 1 THEN
              net_profit
          ELSE
              0
          END)) AS stock_profit,
          PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id = 3 THEN
              net_profit
          ELSE
              0
          END)) AS etf_profit,
          PRINTF(\"%.2f\", SUM(CASE WHEN net_profit < 0 THEN
              net_profit
          ELSE
              0
          END)) AS loss
        FROM
          trade t
        LEFT JOIN asset a ON t.asset_id = a.id
        WHERE
          date BETWEEN ':year-01-01' AND ':year-12-31'
        AND t.trade_type_id = 2
          AND purchase = 0
          AND a.asset_type_id IN (1, 3)
          GROUP BY month
      )
      WHERE CAST(value AS decimal) <> 0
      ", { year: filter_params[:year].to_i }]).map { |i| i.attributes.except('id') }
  end

  def swing_trade
    result = Trade.find_by_sql("
      SELECT
        year,
        month,
        CASE WHEN CAST(stock_sales AS decimal) > 20000 THEN
          stock_profit + options_profit + etf_profit + loss
        ELSE
          etf_profit + options_profit + loss
        END value
        FROM
        (
          SELECT
            STRFTIME('%Y', date) year,
            STRFTIME('%m', date) month,
          PRINTF(\"%.2f\", SUM(CASE WHEN a.asset_type_id = 1 THEN
            total_amount
          ELSE
            0
          END)) AS stock_sales,
          PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id = 1 THEN
                net_profit
            ELSE
                0
            END)) AS stock_profit,
            PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id IN (4, 5) THEN
                net_profit
            ELSE
                0
            END)) AS options_profit,
            PRINTF(\"%.2f\", SUM(CASE WHEN net_profit > 0 AND a.asset_type_id = 3 THEN
                net_profit
            ELSE
                0
            END)) AS etf_profit,
            PRINTF(\"%.2f\", SUM(CASE WHEN net_profit < 0 THEN
                net_profit
            ELSE
                0
            END)) AS loss
          FROM
            trade t
          LEFT JOIN asset a ON t.asset_id = a.id
          WHERE
            t.trade_type_id = 1
            AND purchase = 0
            AND a.asset_type_id <> 2
            GROUP BY year, month
        )
        WHERE CAST(value AS decimal) <> 0
    ").map { |i| i.attributes.except('id') }

    calculate_accumulated_loss(result, 'swing_trade', filter_params[:year])
  end

  def fiis
    result = Trade.select(
      'STRFTIME("%Y", date) year,
                 STRFTIME("%m", date) month,
                 SUM(net_profit) AS value'
    )
                  .where('trade_type_id = 3
                 AND purchase = 0')
                  .group('year', 'month').map { |i| i.attributes.except('id') }

    calculate_accumulated_loss(result, 'fiis', filter_params[:year])
  end

  def end_year_positions
    end_year_positions = EndYearPosition.select('
                                        asset.code,
                                        asset.cnpj,
                                        asset.name AS asset_name,
                                        end_year_position.position,
                                        ROUND(end_year_position.average_price, 2) AS average_price,
                                        ROUND(end_year_position.total_cost, 2) AS total_cost,
                                        asset_type.name AS asset_type')
                                        .joins(financial_asset: :asset_type)
                                        .where(year: filter_params[:year])
                                        .order('asset.code')

    end_year_positions.map do |i|
      p = i.attributes.except('id')

      p['text'] = case i.asset_type
                  when 'Ação'
                    "#{i.position} AÇÕES DA #{i.asset_name}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.code}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.average_price.to_s.gsub('.', ',')} POR AÇÃO. CUSTO TOTAL DE R$ #{i.total_cost.to_s.gsub('.', ',')}"
                  when 'FII'
                    "#{i.position} COTAS DO FII #{i.asset_name}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.code}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.average_price.to_s.gsub('.', ',')} POR COTA. CUSTO TOTAL DE R$ #{i.total_cost.to_s.gsub('.', ',')}"
                  when 'ETF'
                    "#{i.position} COTAS DO ETF #{i.asset_name}. CÓDIGO DE NEGOCIAÇÃO B3: #{i.code}. CNPJ #{i.cnpj}. PREÇO MÉDIO DE R$ #{i.average_price.to_s.gsub('.', ',')} POR COTA. CUSTO TOTAL DE R$ #{i.total_cost.to_s.gsub('.', ',')}"
                  else
                    'goiaba'
                  end

      p.except('asset_type')
      p
    end
  end

  private

  def filter_params
    params.permit(:year, :commit)
  end

  def calculate_accumulated_loss(result, trade_type, year)
    accumulated_loss = 0

    result.map do |r|
      accumulated_loss = if (r['value']).negative?
                           accumulated_loss + r['value']
                         elsif (r['value']).positive?
                           [0, accumulated_loss + r['value']].min
                         else
                           accumulated_loss
                         end

      # Get accumulated loss just from the last year.
      if r['year'].to_i == year.to_i - 1
        @accumulated_loss_last_year[trade_type] = 0 unless @accumulated_loss_last_year.key? trade_type
        @accumulated_loss_last_year[trade_type] = accumulated_loss
      end

      # Don't show data from other years than the filtered one.
      next if year != r['year']

      r['accumulated_loss'] = accumulated_loss

      r
    end.compact
  end
end
