require 'pdf'

# Classe que representa uma nota de corretagem.
class NotaCorretagem
  attr_accessor :operacoes, :irrfs, :data, :total_taxas, :broker,
                :base_irrf_operacoes, :irrf_indefinido

  def initialize(broker)
    @operacoes = []
    @irrfs = []
    @broker = broker
    @base_irrf_operacoes = BaseIrrf.new
  end

  def self.get_corretora(caminho_arquivo)
    caminho_arquivo.match(%r{(.*)/BrokerNotes/(.*)/(.*)}).captures[1].constantize.new
  end

  def extrai_dados_arquivo(caminho_arquivo)
    dados_nota = Pdf.extrai_dados_pdf(caminho_arquivo)

    @data = @broker.extrai_data_nota_corretagem(dados_nota)

    @operacoes = @broker.extract_operacoes_broker_note(dados_nota)
    @total_taxas = @broker.extrai_taxas_nota_corretagem(dados_nota)
    rateia_taxas

    @irrfs = extrai_irrf(dados_nota)

    @operacoes = @operacoes.map do |operacao|
      operacao.data = @data
      operacao
    end
  end

  def rateia_taxas
    operacoes_total = 0

    operacoes_total = @operacoes.map(&:total).reduce(:+)

    @operacoes.each_with_index do |operacao, index|
      percentual_operacao = operacao.total / operacoes_total
      @operacoes[index].fees = (@total_taxas * percentual_operacao).round(2)
    end
  end

  def calcula_base_irrf
    @operacoes.each do |trade|
      next if operacao.purchase == 1

      case operacao.tipo_operacao.nome
      when 'Swing Trade'
        base_irrf_operacoes.base_swing_trade += operacao.total_amount
      when 'FII'
        base_irrf_operacoes.base_fiis += operacao.total_amount
      end
    end

    base_irrf_operacoes.base_swing_trade = base_irrf_operacoes.base_swing_trade.round(2)
    base_irrf_operacoes.base_fiis = base_irrf_operacoes.base_fiis.round(2)
  end

  def extrai_irrf(dados_nota)
    def replacements(s)
      s.gsub('.', '').gsub(',', '.')
    end

    calcula_base_irrf

    irrfs = []

    dados_nota.each_with_index do |k, i|
      # Swing Trade e FIIs.
      obj = k.match @broker.expressao_irrf

      if obj

        valor_base = replacements(obj[1]).to_d

        next unless valor_base.positive?

        valor_irrf = replacements(dados_nota[i + @broker.ajuste_posicao_irrf]).to_d

        next unless valor_irrf.positive?

        irrf = Irrf.new

        irrf.valor = valor_irrf
        irrf.data = @data

        case valor_base
        when base_irrf_operacoes.base_swing_trade
          irrf.tipo_operacao = TipoOperacao.find_by(nome: 'Swing Trade')
        when base_irrf_operacoes.base_fiis
          irrf.tipo_operacao = TipoOperacao.find_by(nome: 'FII')
        else
          irrf_indefinido = true
        end

        irrfs << irrf unless irrf_indefinido == true
      end

      # Day Trade
      obj = k.match @broker.expressao_irrf_day_trade

      next unless obj

      valor = replacements(obj[3]).to_d

      next unless valor.positive?

      irrf = Irrf.new

      irrf.tipo_operacao = TipoOperacao.find_by(nome: 'Day Trade')
      irrf.valor = valor
      irrf.data = @data

      irrfs << irrf
    end

    irrfs
  end
end
