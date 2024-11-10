class Clear
  def extract_broker_note_date(note_data)
    date_position = note_data.find_index('Data pregão') + 2

    note_data[date_position].to_date
  end

  def extract_trades_broker_note(note_data)
    trades = []

    trades_entry = note_data.each_index.select { |i| note_data[i] == '1-BOVESPA' }

    trades_entry.each do |position|
      # What is in
      # noteData[position + 1]
      # should be "C" or "V". However, sometimes it is something like "C FRACIONARIO". In other
      # words, it mixes with the information that should come imediately after.
      # Because of that, the verification below is done to fix the problem when it happens.

      purchase = ''
      asset_name_position = nil
      market_type = ''

      if note_data[position + 1].length > 1
        asset_name_position = position + 2
        market_type = note_data[position + 1][2..]
        purchase = note_data[position + 1][0]
      else
        asset_name_position = position + 3
        market_type = note_data[position + 2]
        purchase = note_data[position + 1]
      end

      match_obj = note_data[asset_name_position].match(/(.*?)(\s{10})(\w{2,6})/)

      tipo_acao = match_obj[3]
      note_description = match_obj[1]

      # If it is a subscription right sale, ignore it — so many IRPF problems!
      next if tipo_acao == 'DO'

      trade = Trade.new
      # asset = Asset.new

      if ['OPCAO DE COMPRA', 'OPCAO DE VENDA'].include?(market_type)

        conversion = { 'OPCAO DE COMPRA' => 'CALL', 'OPCAO DE VENDA' => 'PUT' }
        asset_type = AssetType.find_by(name: conversion[market_type])

        match_obj_option = note_data[asset_name_position].match(%r{\d{2}/\d{2} \w{5}\d{3}})
        note_description =  match_obj_option[0]

        financial_asset = FinancialAsset.find_or_create_by(
          code: match_obj_option[0],
          name: match_obj_option[0],
          note_description: match_obj_option[0],
          asset_type:,
          cnpj: ''
        )
      end

      financial_assets = FinancialAsset.where(note_description:)

      # Alguns ativos têm o mesmo valor no campo DescricaoNota, por isso nesses casos
      # é necessário filtrar também pelo tipo_acao. Não dá pra filtrar direto pelo
      # tipo_acao porque às vezes o arquivo PDF traz um valor em branco no que deveria
      # ser o tipo_acao.
      if financial_assets.length > 1
        financial_asset = financial_assets.where(share_type: tipo_acao).first
      else
        financial_asset = financial_assets.first
      end

      trade.financial_asset = financial_asset

      # Purchase
      trade.purchase = purchase == 'C'

      # Starting from 2 positions after 1-BOVESPA (in order to skip the field "C" or "V"
      # (Purchase or Sell)), goes sequentially until find the letter "D" (debit) or
      # "C" (credit), so then to go backwards and find the fields QUANTIDADE and PREÇO.

      # As the fields after the asset name are uncertain (sometimes it has "observation",
      # sometimes doesn't), the safest place to find the other fields is "1-BOVESPA" of the
      # next asset or, if it doesn't exist (in case we are on the last asset), we use
      # "NOTA DE NEGOCIAÇÃO".

      temp_position = position + 1

      temp_position += 1 until ['1-BOVESPA',
                                'NOTA DE NEGOCIAÇÃO',
                                'NOTA DE CORRETAGEM'].include? note_data[temp_position]

      # Case there are observations — at the moment it is used just to indicate when it is Day Trade.
      if temp_position - 7 == asset_name_position
        observations_position = temp_position - 6
        observations = note_data[observations_position].gsub('#', '').gsub('2', '')

        trade.trade_type = TradeType.find_by(name: 'Day Trade') if observations == 'D'
      end

      trade.quantity = note_data[temp_position - 5].gsub('.', '').to_d
      trade.asset_price = note_data[temp_position - 4].gsub(',', '.').to_d
      trade.total_amount = (trade.asset_price * trade.quantity).to_f

      if financial_asset.asset_type.name == 'FII'
        trade.trade_type = TradeType.find_by(name: 'FII')
      elsif trade.trade_type != TradeType.find_by(name: 'Day Trade')
        # In case it was not marked as Day Trade above by observations.
        trade.trade_type = TradeType.find_by(name: 'Swing Trade')
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

    position = note_data.index('Taxa de liquidação') - 1
    settlement_fee = fix_punctuation(note_data[position]).to_d.abs

    position = note_data.index('Taxa de Registro') - 1
    registration_fee = fix_punctuation(note_data[position]).to_d.abs

    position = note_data.index('Total Bovespa / Soma') - 1
    bovespa_total = fix_punctuation(note_data[position]).to_d.abs

    date_note = extract_broker_note_date(note_data)

    # Operational Costs Total
    # Until day 23/12/2019, it was written "Total corretagem" instead of
    # "Total Custos"

    position = if date_note > '23/12/2019'.to_date
                 note_data.index('Total Custos / Despesas') - 1
               else
                 note_data.index('Total corretagem / Despesas') - 1
               end

    operational_costs = fix_punctuation(note_data[position]).to_d

    settlement_fee + registration_fee + bovespa_total + operational_costs
  end

  def irrf_expression
    %r{I.R.R.F. s/ operações, base R\$(.*)}
  end

  def irrf_expression_day_trade
    /IRRF Day Trade: Base R\$ (.*),(.*) Projeção R\$ (.*)/
  end

  def irrf_position_adjust
    -1
  end
end
